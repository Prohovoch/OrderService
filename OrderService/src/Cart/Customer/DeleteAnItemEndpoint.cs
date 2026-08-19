using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;




namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class DeleteAnItemEndpoint(ApplicationDbContext dbContext) : Endpoint<DeleteItemRequest, DeleteItemResponse> // this is bad.
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Delete("api/customer/cart/items");
            Roles("customer");
            Validator<DeleteAnItemValidator>();
        }


        public override async Task HandleAsync(DeleteItemRequest req, CancellationToken ct)
        {
            var affectedRows = await _dbContext.CartItems
                .Where(bi => bi.Id == req.BucketItemId && bi.BucketId == req.BucketId && bi.Bucket.CustomerId == req.UserId)
                .ExecuteDeleteAsync(ct);

            if (affectedRows == 0)
            {
                AddError("Item not found.");
                await Send.ErrorsAsync();
                return;
            }
            await Send.StringAsync(new DeleteItemResponse { message = "Item deleted successfully." }.ToString(), 200); // idk what a fuck did i do here, but i guess it could work.

        }
    }

    public class DeleteAnItemValidator : Validator<DeleteItemRequest>
    {
        public DeleteAnItemValidator()
        {
            RuleFor(x => x.BucketItemId).NotNull().WithMessage("BucketItemId is required.");
            RuleFor(x => x.BucketId).NotNull().WithMessage("BucketId is required.");
        }
    }

    public sealed record DeleteItemRequest
    {
        [FromClaim]
        public Guid UserId { get; init; }
        public Guid BucketId { get; init; }
        public Guid BucketItemId { get; init; }

    }

    public sealed record DeleteItemResponse // i fought a framework and i won.
    {
        public string message
        {
            get; init;
        } = null!;


    }
}
    
