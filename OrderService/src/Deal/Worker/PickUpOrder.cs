using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Deal.Worker
{
    public class PickUpOrder(ApplicationDbContext dbContext) : Endpoint<PickUpOrderRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/worker/{telegramId}/order/{OrderId}");
            AllowAnonymous();
            Validator<PickUpOrderValidator>();

        }


        public override async Task HandleAsync(PickUpOrderRequest req, CancellationToken ct)
        {
            // Get all orders which connects with the user.
            var entityId = await _dbContext.Workers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => x.Id).FirstAsync(ct);
            var specOrder = await _dbContext.Orders.Where(x => x.Id == req.OrderId).FirstOrDefaultAsync(ct);

            if (specOrder is null)
            {
                await Send.NotFoundAsync();
                return;
            }


            specOrder.WorkerId = entityId;
            specOrder.Status = OrderStatus.Processing;

            await _dbContext.SaveChangesAsync(ct);
            await Send.OkAsync();


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
        [BindFrom("telegramId")]
        public long TelegramId { get; init; }
        [BindFrom("orderId")]
        public Guid OrderId { get; init; }
    }



}

