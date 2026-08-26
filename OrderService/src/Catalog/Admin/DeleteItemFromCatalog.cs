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
            Delete("api/catalog/item/{ProductId}");
            Roles("admin");
            Validator<DeleteItemValidator>();

        }


        public override async Task HandleAsync(DeleteItemRequest req, CancellationToken ct)
        {
            bool isOwned = await _dbContext.Products.AnyAsync(p => p.Id == req.ProductId && p.AdminId == req.AdminId, ct);
            if (!isOwned)
            {
                AddError("AdminId: ", "Invalid id ");
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



            
        }

    }

    public class DeleteItemValidator : Validator<ChangeStatsCatalogRequest>
    {
        public DeleteItemValidator()
        {
            RuleFor(x => x.AdminId).NotEmpty().WithMessage(" AdminId is required.");
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId required.");
        }
    }


   
    public sealed record DeleteItemRequest

    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.
        [FromClaim]
        public Guid AdminId { get; init; }
        public Guid ProductId { get; init; }
    

    }
}
