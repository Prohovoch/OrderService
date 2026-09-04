using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Deal.Worker
{
    public class PickUpOrder(ApplicationDbContext dbContext) : Endpoint<PickUpOrderRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/worker/order/{OrderId}");
            Roles("worker");
            Validator<PickUpOrderValidator>();

        }


        public override async Task HandleAsync(PickUpOrderRequest req, CancellationToken ct)
        {
            // Get all orders which connects with the user.
            var specOrder = await _dbContext.Orders.Where(x => x.Id == req.OrderId).FirstOrDefaultAsync(ct);

            if (specOrder is null)
            {
                AddError("OrderId", " Object specified is not found");
                await Send.ErrorsAsync();
                return;
            }


            specOrder.WorkerId = req.UserId;
            specOrder.Status = OrderStatus.Processing;

            await _dbContext.SaveChangesAsync(ct);
            await Send.OkAsync();


        }
    }

    public class PickUpOrderValidator : Validator<PickUpOrderRequest>
    {
        public PickUpOrderValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId required!");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId required!");
        }
    }



   


    public sealed record PickUpOrderRequest
    {
        [FromClaim]
        public Guid UserId { get; init; }
        public Guid OrderId { get; init; }
    }



}

