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
            decimal? productPrice = await _dbContext.Products.Where(p => p.Id == req.ProductId).Select(p => (decimal?)p.Price).FirstOrDefaultAsync(ct);

            if (productPrice == null)
            {
                AddError("ProductId", "Product does not exist.");
                await Send.ErrorsAsync();
                return;
            }

            // checks if the cart item exists in the database and updates the quantity
            var affectedRows = await _dbContext.CartItems
                .Where(ci => ci.BucketId == req.BucketId && ci.ProductId == req.ProductId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(ci => ci.BucketItemQuantity, ci => req.Quantity), ct);
            // if item is deleted by foreign key where it is null? when what we should do?
            if (affectedRows == 0)
            {
                AddError("ProductID", "Product has been deleted");
                await Send.NotFoundAsync();
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
            RuleFor(x => x.ProductId).NotNull().WithMessage("ProductId is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be a positive number.");

        }
    }



    public sealed record UpdateItemQuantityRequest
    {

        public Guid BucketId { get; init; }
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
    }


    public sealed record UpdateItemQuantityResponse
    {
        // should we use quantity?
        public decimal CalcPrice { get; init; }

    }



}
