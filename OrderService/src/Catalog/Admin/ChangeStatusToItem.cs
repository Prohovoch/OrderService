using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;
using System.Data;

namespace OrderService.src.Catalog.Admin
{
    public class ChangeStatusToItem(ApplicationDbContext dbContext) : Endpoint<ChangeStatsCatalogRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/catalog/item/{ProductId}");
            Roles("admin");
            Validator<ChangeStatsCatalogValidator>();

        }


        public override async Task HandleAsync(ChangeStatsCatalogRequest req, CancellationToken ct)
        {
            // mapping 
            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == req.ProductId && x.AdminId == req.AdminId, ct); // protection from concurrent requests, shit -  no solution.
            if (product is null)
            {
                AddError("ProductID:", "Product not found.");
                await Send.ErrorsAsync();
                return;
            } 

            product.ProductName = req.ProductName ?? product.ProductName;
            product.Price = req.Price ?? product.Price;
            product.AvailabilityStatus = req.AvailabilityStatus.HasValue ? (ProductAvailabilityStatus)req.AvailabilityStatus.Value : product.AvailabilityStatus;
            product.Details = new ProductDetails
            {
                Ingredients = req.Ingredients ?? product.Details.Ingredients,
                Volume = req.Volume ?? product.Details.Volume,
                Weight = req.Weight ?? product.Details.Weight,

            };
           


            await _dbContext.SaveChangesAsync(ct);
  }

        }
   
    public class ChangeStatsCatalogValidator : Validator<ChangeStatsCatalogRequest>
    {
        public ChangeStatsCatalogValidator()
        {
            RuleFor(x => x.ProductName).NotEmpty().MaximumLength(100).When(x => x.ProductName != null).WithMessage("Product name must not be empty.");
            RuleFor(x => x.Price).NotEmpty().When(x => x.Price != null).WithMessage("Price is required.");
            RuleFor(x => x.AvailabilityStatus).IsInEnum().WithMessage("Invalid availability status.");
            RuleFor(x => x.Ingredients).Must(ing => ing != null && ing.Count > 0).When(x => x.Ingredients != null).WithMessage("Ingredients are required.");
            RuleFor(x => x.Volume).GreaterThan(0).When(x => x.Volume != null).WithMessage("Volume must be greater than 0.");
            RuleFor(x => x.Weight).GreaterThan(0).When(x => x.Weight != null).WithMessage("Weight must be greater than 0.");
        }
    }

    
    public enum ChangeProductAvStatus
    {
        Available, OutOfStock
    }
    public sealed record ChangeStatsCatalogRequest

    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.
        [FromClaim]
        public Guid AdminId { get; init; }
        public Guid ProductId { get; init; }
        public string? ProductName { get; init; }
        public decimal? Price { get; init; }


        public ChangeProductAvStatus? AvailabilityStatus { get; init; }
        // this is from JsonB part 
        public List<string>? Ingredients { get; set; }
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; }

    }
}
    

