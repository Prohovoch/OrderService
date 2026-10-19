using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Deal.Worker
{
    public class PickUpOrder(ApplicationDbContext dbContext) : Endpoint<PickUpOrderRequest>
    {

        public override void Configure()
        {
            Patch("api/worker/order/{orderId}/pickup");
            AllowAnonymous();
            Validator<PickUpOrderValidator>();

        }


        public override async Task HandleAsync(PickUpOrderRequest req, CancellationToken ct)
        {
            // using pessimistic locking

            using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
            try
            {


                // Get all orders which connects with the user.
                var entityId = await dbContext.Workers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
                if (entityId is null)
                {
                    Logger.LogWarning("No entity were found for TelegramId: {TelegramId}", req.TelegramId);
                    await Send.ForbiddenAsync();
                    return;
                }
                var workerData = await dbContext.WorkerProfiles.Where(x => x.WorkerId == entityId.Value).AsNoTracking().Select(x => new { x.Name, x.Surname }).FirstOrDefaultAsync(ct);
                if (workerData is null)
                {
                    Logger.LogWarning("No profile were found for Worker with Id: {WorkerId}", entityId.Value);
                    await Send.NotFoundAsync();
                    return;
                }
                var specOrder = await dbContext.Orders.FromSqlInterpolated($@"SELECT * FROM ""order"" WHERE ""id"" = {req.OrderId} FOR UPDATE NOWAIT").FirstOrDefaultAsync(ct);
                    
                    // Where(x => x.Id == req.OrderId).FirstOrDefaultAsync(ct);  status, worker_name, worker_surname order

                if (specOrder is null)
                {
                    Logger.LogWarning("No order were found for OrderId: {OrderId}", req.OrderId);
                    await Send.ForbiddenAsync();
                    return;
                }
                

                if (specOrder.Status is OrderStatus.Closed)
                {   
                    Logger.LogWarning("Trying to pick up a closed order with Id: {OrderId}", req.OrderId);
                    await Send.ForbiddenAsync();
                    return;
                }
                
                int activeOrdersCount = await dbContext.Orders.Where(x => x.WorkerId == entityId.Value && x.Status == OrderStatus.Processing).CountAsync(ct);
                if (activeOrdersCount >= 3)
                {
                    Logger.LogWarning("Worker with Id: {WorkerId} has reached the maximum number of active orders (3).", entityId.Value);
                    AddError("You have reached the maximum number of active orders (3). Please complete or close existing orders before picking up the new ones.");
                    await Send.ErrorsAsync();
                    return;
                }
                

                specOrder.WorkerId = entityId.Value;
                specOrder.Status = OrderStatus.Processing;
                specOrder.WorkerName = workerData.Name;
                specOrder.WorkerSurname = workerData.Surname;
                await dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await Send.NoContentAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.LockNotAvailable)
            {
                Logger.LogWarning("Order is already locked by another process.");
                await transaction.RollbackAsync(ct);
                await Send.StatusCodeAsync(StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while picking up the order.");
                await transaction.RollbackAsync(ct);
                await Send.StatusCodeAsync(StatusCodes.Status500InternalServerError);

            }
            }
    }

    public class PickUpOrderValidator : Validator<PickUpOrderRequest>
    {
        public PickUpOrderValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId required!");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId required!");
        }
    }



   


    public sealed record PickUpOrderRequest
    {
        [FromHeader("Worker-Telegram-Id")]
        public long TelegramId { get; init; }
        [BindFrom("orderId")]
        public Guid OrderId { get; init; }
    }



}

