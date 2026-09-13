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
           
         
            var workerExists = await _dbContext.Workers.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (!workerExists)
            {
                AddError("TelegramId", "No worker found with the given Telegram ID.");
                await Send.ErrorsAsync(400, ct);
                return;
            }

            var specOrder = await _dbContext.Orders.Where(x => x.Id == req.OrderId).FirstOrDefaultAsync(ct);
            if (specOrder is null)
            {
                await Send.NotFoundAsync();
                return;
            }
            if(!Enum.TryParse<OrderStatus>(req.Status.ToString(), out var newStatus))
            {
                AddError("Status", "Invalid status value.");
                await Send.ErrorsAsync(400, ct);
                return;
            }   
            // doing some fsm magic on my kneel.

            var isValid = AllowedTransitions.Contains((specOrder.Status, newStatus)); 
            if (!isValid)
            {
                AddError("Transition:", "Cannot convert transition");
                await Send.ErrorsAsync();
                return;
            }
            specOrder.Status = newStatus; 

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
        [BindFrom("telegramId")]
        public long TelegramId { get; init; }
        [BindFrom("orderId")]
        public Guid OrderId { get; init; }

        public StatusType Status { get; init; }
        
    }



}

