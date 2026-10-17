using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
            var specOrder = await dbContext.Orders.Where(x => x.Id == req.OrderId).FirstOrDefaultAsync(ct);

            if (specOrder is null)
            {
                await Send.ForbiddenAsync();
                return;
            }

            int activeOrdersCount =  await dbContext.Orders.Where(x => x.WorkerId == entityId.Value && x.Status == OrderStatus.Processing).CountAsync(ct);
            if (activeOrdersCount >= 3)
            {
                AddError("You have reached the maximum number of active orders (3). Please complete or close existing orders before picking up new ones.");
                await Send.ErrorsAsync();
                return;
            }
            if (specOrder.WorkerId is not null)
            {
                await Send.ForbiddenAsync();
                return;
            }

            specOrder.WorkerId = entityId.Value;
            specOrder.Status = OrderStatus.Processing;
            specOrder.WorkerName = workerData.Name;
            specOrder.WorkerSurname = workerData.Surname;
            await dbContext.SaveChangesAsync(ct);
            await Send.NoContentAsync();


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

