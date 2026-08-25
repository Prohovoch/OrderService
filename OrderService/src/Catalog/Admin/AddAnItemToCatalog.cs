using FastEndpoints;
using FluentValidation;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Cart.Customer;

namespace OrderService.src.Catalog.Admin { 
    public class AddAnItemToCatalog(ApplicationDbContext dbContext) : Endpoint<AddAnItemToCatalogRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/catalog");
            Roles("admin");
            Validator<AddAnItemToCatalogValidator>();

        }
     

        public override async Task HandleAsync(AddAnItemToCatalogRequest req, CancellationToken ct)
        {
            // mapping 
            var catalogItem = new CatalogItem
            {
                ProductName = req.ProductName,
                Price = req.Price,
                Type = req.ProductType switch
                {
                    AddProductType.Pizza => ProductType.Pizza,
                    AddProductType.Burger => ProductType.Burger,
                    AddProductType.Soup => ProductType.Soup,
                    AddProductType.Salad => ProductType.Salad,
                    AddProductType.Sushi => ProductType.Sushi,
                    AddProductType.Drinks => ProductType.Drinks,
                    _ => throw new ArgumentOutOfRangeException()
                },
                AvailabilityStatus = req.AvailabilityStatus switch
                {
                    AddProductAvailabilityStatus.Available => ProductAvailabilityStatus.Available,
                    AddProductAvailabilityStatus.OutOfStock => ProductAvailabilityStatus.OutOfStock,
                    _ => throw new ArgumentOutOfRangeException()
                  
                },
                Details = new ProductDetails
                {
                    Ingredients = req.Ingredients,
                    Volume = req.Volume,
                    Weight = req.Weight
                }
                
            };
            _dbContext.Products.Add(catalogItem);
            await _dbContext.SaveChangesAsync();


        }
    }   
    public class AddAnItemToCatalogValidator : Validator<AddAnItemToCatalogRequest>
    {
            public AddAnItemToCatalogValidator()
            {
                RuleFor(x => x.ProductName).NotEmpty().WithMessage("Product name must not be empty.");
                RuleFor(x => x.Price).NotEmpty().WithMessage("Price is required.");
                RuleFor(x => x.ProductType).IsInEnum().WithMessage("Invalid product type.");
                RuleFor(x => x.AvailabilityStatus).IsInEnum().WithMessage("Invalid availability status.");

            }
        }

    public enum AddProductType
    {
        Pizza, Burger, Soup, Salad, Sushi, Drinks
    }
    public enum AddProductAvailabilityStatus
    {
        Available, OutOfStock
    }
    public sealed record AddAnItemToCatalogRequest

    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.

        public string ProductName { get; init; } = string.Empty;
        public decimal Price { get; init; }

        public AddProductType ProductType { get; init; }

        public AddProductAvailabilityStatus AvailabilityStatus { get; init; }
        // this is from JsonB part
        public List<string> Ingredients { get; set; } = [];
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; }

    }



}

