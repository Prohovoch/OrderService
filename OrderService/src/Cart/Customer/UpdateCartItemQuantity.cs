using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;





namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class UpdateCartItemQuantity(ApplicationDbContext dbContext) : Endpoint<UpdateItemQuantityRequest, UpdateItemQuantityResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/customer/cart/{bucketId}/items/{bucketItemId}");
            AllowAnonymous();
            Validator<UpdateCartItemQuantityValidator>();

        }


        public override async Task HandleAsync(UpdateItemQuantityRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database

            var entityId = await _dbContext.Customers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => x.Id).FirstAsync(ct);
            var productInfo = await _dbContext.CartItems.Where(p => p.Id == req.BucketItemId && p.BucketId == req.BucketId && p.Bucket!.CustomerId == entityId).AsNoTracking().Select(p =>  new { p.Product.Price}).FirstOrDefaultAsync(ct);

            if (productInfo is null) //  guarantees not existing
            {
              
                await Send.NotFoundAsync(); // or 404?
                return;
            }

          
       
            var affectedRows = await _dbContext.CartItems.Where(p => p.Id == req.BucketItemId && p.BucketId == req.BucketId && p.Bucket!.CustomerId == entityId).ExecuteUpdateAsync(p => p.SetProperty(x => x.BucketItemQuantity, req.Quantity), ct);
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
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("UserId is required");
            RuleFor(x => x.BucketItemId).NotNull().WithMessage("BucketItemId is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be a positive number.");
            RuleFor(x => x.BucketId).NotEmpty().WithMessage("BucketId is required");

        }
    }



    public sealed record UpdateItemQuantityRequest
    {
        
        public long TelegramId { get; init; }

        [BindFrom("bucketId")]
        public Guid BucketId { get; init; }
        [BindFrom("bucketItemId")]
        public Guid BucketItemId { get; init; }
     
        public int Quantity { get; init; }
    }


    public sealed record UpdateItemQuantityResponse
    {
        // should we use quantity?
        public decimal CalcPrice { get; init; }

    }



}
