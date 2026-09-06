using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Catalog.Admin
{
    public class DeleteItemFromCatalog(ApplicationDbContext dbContext) : Endpoint<DeleteItemRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Delete("api/catalog/item/{productId}");
            AllowAnonymous();
            Validator<DeleteItemValidator>();

        }


        public override async Task HandleAsync(DeleteItemRequest req, CancellationToken ct)
        {

            var adminId = await _dbContext.Admins.Where(a => a.TgId == req.TelegramId).Select(a => a.Id).FirstAsync(ct); // XDDDDDDDDDDDDDDDDD
            
            bool isOwned = await _dbContext.Products.AnyAsync(p => p.Id == req.ProductId && p.AdminId == adminId, ct); // same as in patch thing.
            // i dont want to think about concurrency right now cause i guess there will be only 1 instance of app.
            if (!isOwned)
            {
                AddError("AdminId: ", "Invalid id");
                await Send.ErrorsAsync();
                return;
            }

            var affectedRows = await _dbContext.Products.Where(p => p.Id == req.ProductId).ExecuteDeleteAsync(ct);
            if (affectedRows == 0)
            {
                AddError("Something went wrong during execution. No object found.");
                await Send.ErrorsAsync();
                return;

            }


            await Send.NoContentAsync();
            
        }

    }

    public class DeleteItemValidator : Validator<DeleteItemRequest>
    {
        public DeleteItemValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required.");
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId required.");
        }
    }


   
    public sealed record DeleteItemRequest

    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.
        public long TelegramId { get; init; }

        [BindFrom("productId")]
        public Guid ProductId { get; init; }
    

    }
}
