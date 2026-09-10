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
            Patch("api/worker/{telegramId}/order/{orderId}");
            AllowAnonymous();
            Validator<ChangeOrderStatusValidator>();
        }


        private static readonly HashSet<(OrderStatus From, StatusType To)> AllowedTransitions =
        [
            (OrderStatus.Created, StatusType.Processing),
            (OrderStatus.Processing, StatusType.Stopped),
            (OrderStatus.Processing, StatusType.Cooked),
            (OrderStatus.Stopped, StatusType.Processing),
            (OrderStatus.Cooked, StatusType.Closed),
        ];
    
        
        public override async Task HandleAsync(ChangeOrderStatusRequest req, CancellationToken ct)
        {
            // Get all orders which connects with the user.
          
            var specOrder = await _dbContext.Orders.Where(x => x.Id == req.OrderId).FirstOrDefaultAsync(ct);
            var workerExists = await _dbContext.Workers.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (!workerExists)
            {
                AddError("TelegramId", "No worker found with the given Telegram ID.");
                await Send.ErrorsAsync(400, ct);
                return;
            }
            if (specOrder is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            // doing some fsm magic on my kneel.

            var isValid = AllowedTransitions.Contains((specOrder.Status, req.Status)); //idk,
            if (!isValid)
            {
                AddError("Transition:", "Convert convert transition");
                await Send.ErrorsAsync();
                return;
            }
            specOrder.Status = Enum.Parse<OrderStatus>(req.Status.ToString()); // omg

            await _dbContext.SaveChangesAsync(ct);
            await Send.NoContentAsync();


        }
    }

    public class ChangeOrderStatusValidator : Validator<ChangeOrderStatusRequest>
    {
        public ChangeOrderStatusValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId required!");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId required!");
            RuleFor(x => x.Status).IsInEnum().WithMessage("Status must be a valid enum value.");
        }
    }




    public enum StatusType
    {
        Processing, Cooked, Stopped, Closed
    }

    public sealed record ChangeOrderStatusRequest
    {
        [BindFrom("telegramId")]
        public long TelegramId { get; init; }
        [BindFrom("orderId")]
        public Guid OrderId { get; init; }

        public StatusType Status { get; init; }
        
    }



}

