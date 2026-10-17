using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Deal.Worker
{
    public class DetachFromOrder(ApplicationDbContext dbContext) : Endpoint<DetachFromOrderRequest>
    {
        public override void Configure()
        {
            Patch("api/worker/order/{orderId}");
            AllowAnonymous();
            Validator<DetachFromOrderValidator>();

        }


        public override async Task HandleAsync(DetachFromOrderRequest req, CancellationToken ct)
        {
            // Get all orders which connects with the user.
            var workerId = await dbContext.Workers.Where(x => x.TgId == req.TelegramId).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            if (workerId is null)
            {
                Logger.LogWarning("Worker with TelegramId {TelegramId} not found.", req.TelegramId);
                await Send.ForbiddenAsync();
                return;
            }
            var specOrder = await dbContext.Orders.Where(x => x.Id == req.OrderId && x.WorkerId == workerId.Value).FirstOrDefaultAsync(ct);

            if (specOrder is null)
            {
                Logger.LogWarning("Order with Id {OrderId} not found or does not belong to the worker with Id {WorkerId}.", req.OrderId, workerId.Value);
                await Send.NotFoundAsync();
                return;
            }

            specOrder.WorkerSurname = null;
            specOrder.WorkerName = null;
            specOrder.WorkerId = null;
            specOrder.Status = OrderStatus.Stopped;

            Logger.LogInformation("Worker with Id {WorkerId} detached from order with Id {OrderId}.", workerId.Value, req.OrderId);
            await dbContext.SaveChangesAsync(ct);
            await Send.OkAsync();


        }
    }

    public class DetachFromOrderValidator : Validator<DetachFromOrderRequest>
    {
        public DetachFromOrderValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId required!");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId required!");
        }
    }






    public sealed record DetachFromOrderRequest
    {
        [FromHeader("Worker-Telegram-Id")]
        public long TelegramId { get; init; }
        [BindFrom("orderId")]
        public Guid OrderId { get; init; }
    }



}

