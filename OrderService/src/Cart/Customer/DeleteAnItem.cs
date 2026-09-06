using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;




namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class DeleteAnItem(ApplicationDbContext dbContext) : Endpoint<DeleteItemRequest, DeleteItemResponse> // this is bad.
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Delete("api/customer/cart/items{bucketItemId}");
            AllowAnonymous();
            Validator<DeleteAnItemValidator>();
        }


        public override async Task HandleAsync(DeleteItemRequest req, CancellationToken ct)
        {

            var entityId = await _dbContext.Customers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => x.Id).FirstAsync();
            // check if user has it
            bool isOwned = await _dbContext.Carts.AnyAsync(c => c.CustomerId == entityId, ct);

            if (!isOwned)
            {
                AddError("Some of credentials are invalid! ");
                await Send.ErrorsAsync();
            }

            var affectedRows = await _dbContext.CartItems
                .Where(bi => bi.Id == req.BucketItemId && bi.Bucket!.CustomerId == entityId) //  hack. mocking warnings. we already have created cart at this point.
                .ExecuteDeleteAsync(ct);
            
            // if wifi is baddie :(
            if (affectedRows == 0)
            {
                AddError("Item not found.");
                await Send.ErrorsAsync();
                return;
            }
            await Send.StringAsync(new DeleteItemResponse { message = "Item deleted successfully." }.ToString(), 204); // idk what a fuck did i do here, but i guess it could work.

        }
    }

    public class DeleteAnItemValidator : Validator<DeleteItemRequest>
    {
        public DeleteAnItemValidator()
            
        {
            RuleFor(x => x.TelegramId).NotNull().WithMessage("UserId is required.");
            RuleFor(x => x.BucketItemId).NotNull().WithMessage("BucketItemId is required.");
          
        }
    }

    public sealed record DeleteItemRequest
    {
 
        public long TelegramId{ get; init; }

        [BindFrom("bucketItemId")]
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
    
