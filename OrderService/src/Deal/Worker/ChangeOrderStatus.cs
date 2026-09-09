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


        private static readonly HashSet<(string From, string To)> AllowedTransitions = new()
        {
            (nameof(OrderStatus.Created), nameof(OrderStatus.Processing)),
            (nameof(OrderStatus.Processing), nameof(OrderStatus.Stopped)),
            (nameof(OrderStatus.Processing), nameof(OrderStatus.Cooked)),
            (nameof(OrderStatus.Stopped), nameof(OrderStatus.Processing)),
            (nameof(OrderStatus.Cooked), nameof(OrderStatus.Closed)),
        };
    
        
        public override async Task HandleAsync(ChangeOrderStatusRequest req, CancellationToken ct)
        {
            // Get all orders which connects with the user.
          
            var specOrder = await _dbContext.Orders.Where(x => x.Id == req.OrderId).FirstOrDefaultAsync(ct);
            var workerExists = await _dbContext.Workers.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (!workerExists)
            {
                AddError("TelegramId", "Работник с таким Telegram ID не найден.");
                await Send.ErrorsAsync(400, ct);
                return;
            }
            if (specOrder is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            // doing some fsm magic on my kneel.

            var isValid = AllowedTransitions.Contains((specOrder.Status.ToString(), req.Status.ToString())); // shit,
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

