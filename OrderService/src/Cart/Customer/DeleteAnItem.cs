using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;
using System.Net;




namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class DeleteAnItem(ApplicationDbContext dbContext) : Endpoint<DeleteItemRequest> // this is bad.
    {

        private readonly ApplicationDbContext _dbContext = dbContext; 

        public override void Configure()
        {
            Delete("api/customer/cart/items/{bucketItemId}");
            AllowAnonymous();
            Validator<DeleteAnItemValidator>();
        }


        public override async Task HandleAsync(DeleteItemRequest req, CancellationToken ct)
        {

            var entityId = await _dbContext.Customers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            // check if user has it
            if (entityId is null)
            {
                Logger.LogWarning("Excpected user is not found in a system");
                await Send.NotFoundAsync();
                return;
            }
            bool isOwned = await _dbContext.Carts.AnyAsync(c => c.CustomerId == entityId.Value, ct);

            if (!isOwned)
            {
                Logger.LogWarning("{CustomerId} cart not found", entityId.Value);
                AddError("Some of credentials are invalid! ");
                await Send.ErrorsAsync();
            }

            var affectedRows = await _dbContext.CartItems
                .Where(bi => bi.Id == req.BucketItemId && bi.Bucket!.CustomerId == entityId.Value) //  hack. mocking warnings. we already have created cart at this point.
                .ExecuteDeleteAsync(ct);
            
            
            if (affectedRows == 0)
            {
                AddError("Item not found.");
                Logger.LogWarning("No {CustomerId} item were found", entityId.Value);
                await Send.ErrorsAsync();
                return;
            }
         
            await Send.NoContentAsync(); // idk what a fuck did i do here, but i guess it could work.
            Logger.LogInformation("Operation completed");

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
        [FromHeader("Customer-Telegram-Id")]
        public long TelegramId{ get; init; }

        [BindFrom("bucketItemId")]
        public Guid BucketItemId { get; init; }

    }



    }

    
