using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;





namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class UpdateCartItemQuantityEndpoint(ApplicationDbContext dbContext) : Endpoint<UpdateItemQuantityRequest, UpdateItemQuantityResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/customer/cart/items");
            Roles("customer");
            Validator<UpdateCartItemQuantityValidator>();

        }


        public override async Task HandleAsync(UpdateItemQuantityRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database
            // OMG THIS SHIT JUST CAME OUT OF MY FKN BRAIN AS FKN STUOID EDGE CASE!!!! LMAO>_)

            var productInfo = await _dbContext.CartItems.Where(p => p.Id == req.BucketItemId && p.BucketId == req.BucketId).Select(p =>  new { p.Product.Price, p.Product.AvailabilityStatus }).FirstOrDefaultAsync(ct);

            if (productInfo == null) //  guarantees not existing
            {
                AddError("ProductId" , "Product does not exist.");
                await Send.ErrorsAsync(); // or 404?
                return;
            }

          
            
            // I KNOW THAT STUPID FSM EXISTS BUT I DONT CARE, CAUSE I HAVE 2 STATS TYPE SO ... YAGNI AND KISS
            if (productInfo.AvailabilityStatus == ProductAvailabilityStatus.OutOfStock)
            {
                await Send.ErrorsAsync();
                return;
            }
            var affectedRows = await _dbContext.CartItems.Where(p => p.Id == req.BucketItemId && p.BucketId == req.BucketId).ExecuteUpdateAsync(p => p.SetProperty(x => x.BucketItemQuantity, req.Quantity), ct);
            if (affectedRows == 0)
            {
                AddError("UpdateFailed", "Failed to update the item quantity.");
                await Send.ErrorsAsync();
                return;
            }
            await Send.OkAsync(new UpdateItemQuantityResponse
            {
                CalcPrice = productInfo.Price * req.Quantity
            });
           

          





        }
    }

    public class UpdateCartItemQuantityValidator : Validator<UpdateItemQuantityRequest>
    {
        public UpdateCartItemQuantityValidator()
        {
            RuleFor(x => x.BucketId).NotNull().WithMessage("BucketId is required.");
            RuleFor(x => x.BucketItemId).NotNull().WithMessage("BucketItemId is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be a positive number.");

        }
    }



    public sealed record UpdateItemQuantityRequest
    {
        public Guid BucketId { get; init; }
        public Guid BucketItemId { get; init; }
     
        public int Quantity { get; init; }
    }


    public sealed record UpdateItemQuantityResponse
    {
        // should we use quantity?
        public decimal CalcPrice { get; init; }

    }



}
