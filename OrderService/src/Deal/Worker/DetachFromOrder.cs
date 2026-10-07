using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Deal.Worker
{
    public class DetachFromOrder(ApplicationDbContext dbContext) : Endpoint<DetachFromOrderRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/worker/order/{orderId}");
            AllowAnonymous();
            Validator<DetachFromOrderValidator>();

        }


        public override async Task HandleAsync(DetachFromOrderRequest req, CancellationToken ct)
        {
            // Get all orders which connects with the user.
            var workerId = await _dbContext.Workers.Where(x => x.TgId == req.TelegramId).Select(x => new {x.Id}).FirstOrDefaultAsync(ct);
            if (workerId is null)
            {
                Logger.LogWarning("Worker with TelegramId {TelegramId} not found.", req.TelegramId);
                await Send.ForbiddenAsync();
                return;
            }
            var specOrder = await _dbContext.Orders.Where(x => x.Id == req.OrderId && x.WorkerId == workerId.Id).FirstOrDefaultAsync(ct);

            if (specOrder is null)
            {
                Logger.LogWarning("Order with Id {OrderId} not found or does not belong to the worker with TelegramId {TelegramId}.", req.OrderId, req.TelegramId);
                await Send.NotFoundAsync();
                return;
            }


            specOrder.WorkerId = null;
            specOrder.Status = OrderStatus.Stopped;

            Logger.LogInformation("Worker with TelegramId {TelegramId} detached from order with Id {OrderId}.", req.TelegramId, req.OrderId);
            await _dbContext.SaveChangesAsync(ct);
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

