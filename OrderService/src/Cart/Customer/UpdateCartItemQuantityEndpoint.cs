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
            AllowAnonymous();
            Validator<UpdateCartItemQuantityValidator>();

        }


        public override async Task HandleAsync(UpdateItemQuantityRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database
            decimal? productPrice = await _dbContext.CartItems.Where(p => p.Id == req.BucketItemId).Select(p => (decimal?)p.Product.Price).FirstOrDefaultAsync(ct);

            if (productPrice == null)
            {
                AddError("ProductId" , "Product does not exist.");
                await Send.ErrorsAsync();
                return;
            }

            // checks if the cart item exists in the database and updates the quantity
            var affectedRows = await _dbContext.CartItems
                .Where(ci => ci.Id == req.BucketItemId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(ci => ci.BucketItemQuantity, ci => req.Quantity), ct);
            // if item is deleted by foreign key where it is null? when what we should do?
            if (affectedRows == 0)
            {
                AddError("ProductID", "Product has been deleted");
                
                await Send.ErrorsAsync();
                return;
            }

            await Send.OkAsync(new UpdateItemQuantityResponse
            {
                CalcPrice = productPrice.Value * req.Quantity
            });





        }
    }

    public class UpdateCartItemQuantityValidator : Validator<UpdateItemQuantityRequest>
    {
        public UpdateCartItemQuantityValidator()
        {
            RuleFor(x => x.BucketItemId).NotNull().WithMessage("BucketItemId is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be a positive number.");

        }
    }



    public sealed record UpdateItemQuantityRequest
    {

        public Guid BucketItemId { get; init; }
     
        public int Quantity { get; init; }
    }


    public sealed record UpdateItemQuantityResponse
    {
        // should we use quantity?
        public decimal CalcPrice { get; init; }

    }



}
