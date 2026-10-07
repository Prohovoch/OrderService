using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Deal.Worker
{
    public class ChangeOrderStatus(ApplicationDbContext dbContext) : Endpoint<ChangeOrderStatusRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/worker/order/{orderId}");
            AllowAnonymous();
            Validator<ChangeOrderStatusValidator>();
        }


        private static readonly HashSet<(OrderStatus From, OrderStatus To)> AllowedTransitions =
        [
            (OrderStatus.Created, OrderStatus.Processing),
            (OrderStatus.Processing, OrderStatus.Stopped),
            (OrderStatus.Processing, OrderStatus.Cooked),
            (OrderStatus.Stopped, OrderStatus.Processing),
            (OrderStatus.Cooked, OrderStatus.Closed),
        ];


        public override async Task HandleAsync(ChangeOrderStatusRequest req, CancellationToken ct)
        {


            var entityId = await _dbContext.Workers.Where(x => x.TgId == req.TelegramId).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            if (entityId is null)
            {
                Logger.LogWarning("Worker with TelegramId {TelegramId} not found.", req.TelegramId);

                await Send.NotFoundAsync();
                return;
            }

            var specOrder = await _dbContext.Orders.Where(x => x.Id == req.OrderId && x.WorkerId == entityId.Value).FirstOrDefaultAsync(ct);
            if (specOrder is null)
            {
                Logger.LogWarning("Order with Id {OrderId} not found or does not belong to the worker with TelegramId {TelegramId}.", req.OrderId, req.TelegramId);
                AddError("OrderId:", "Order not found or does not belong to the worker.");
                await Send.ForbiddenAsync();
                return;
            }

            if (!Enum.TryParse<OrderStatus>(req.Status.ToString(), out var newStatus))
            {
                Logger.LogWarning("Invalid status value {Status} provided for order with Id {OrderId}.", req.Status, req.OrderId);
                AddError("Status", "Invalid status value.");
                await Send.ErrorsAsync(400, ct);
                return;
            }
            // doing some fsm magic on my kneel.

            var isValid = AllowedTransitions.Contains((specOrder.Status, newStatus));
            if (!isValid)
            {
                Logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for order with Id {OrderId}.", specOrder.Status, newStatus, req.OrderId);
                AddError("Transition:", "Cannot convert transition");
                await Send.ErrorsAsync(statusCode: 422);
                return;
            }
            specOrder.Status = newStatus;
        

            if (newStatus == OrderStatus.Cooked)
            {
                Logger.LogInformation("Order with Id {OrderId} has been marked as Cooked.", req.OrderId);
                specOrder.CompletedAt = DateTimeOffset.UtcNow;

                var customerTgId = await _dbContext.Customers.Where(x => x.Id == specOrder.CustomerId).Select(x => (long?)x.TgId).FirstOrDefaultAsync(ct);
                if(customerTgId is not null)
                {
                    Logger.LogInformation("Publishing OrderCompletedEvent for order with Id {OrderId} to customer with TelegramId {CustomerTgId}.", req.OrderId, customerTgId.Value);
                    await PublishAsync(new OrderCompletedEventObj
                    {
                        OrderNumber = specOrder.DisplayOrderNumber,
                        TelegramId = customerTgId.Value,
                    }, Mode.WaitForNone); // we need to log this somehow and test it.

                };    

                }
              
                Logger.LogInformation("Saving changes to the database for order with Id {OrderId}.", req.OrderId);
                await _dbContext.SaveChangesAsync(ct);
                await Send.NoContentAsync();


            }
        }

        public class ChangeOrderStatusValidator : Validator<ChangeOrderStatusRequest>
        {
            public ChangeOrderStatusValidator()
            {
                RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required!");
                RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required!");
                RuleFor(x => x.Status).IsInEnum().WithMessage("Status must be a valid enum value.");
            }
        }




        public enum StatusType
        {
            Processing, Cooked, Stopped, Closed
        }

        public sealed record ChangeOrderStatusRequest
        {
            [FromHeader("Worker-Telegram-Id")]
            public long TelegramId { get; init; }
            [BindFrom("orderId")]
            public Guid OrderId { get; init; }

            public StatusType Status { get; init; }

        }
        public sealed record OrderCompletedEventObj
        {
            public string OrderNumber { get; init; } = null!;
            public long TelegramId { get; init; }
            
        }


    }
}



