using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;


namespace OrderService.src.Catalog.Admin { 
    public class AddAnItemToCatalog(ApplicationDbContext dbContext) : Endpoint<AddAnItemToCatalogRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/catalog/item");
            AllowAnonymous();
            Validator<AddAnItemToCatalogValidator>();

        }
     

        public override async Task HandleAsync(AddAnItemToCatalogRequest req, CancellationToken ct)
        {
            var adminId = await _dbContext.Admins
                .Where(x => x.TgId == req.TelegramId)
                .Select(x => x.Id).FirstAsync(ct);

            
                            // mapping 
            var catalogItem = new CatalogItem
            {   
                Id = Guid.CreateVersion7(),
                // client send
                AdminId = adminId,
                ProductName = req.ProductName,
                CreatorName = req.CreatorName,
                CreatorSurname = req.CreatorSurname,
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

            var response = new AddAnItemResponse
            {
                Id = catalogItem.Id,
            };

            _dbContext.Products.Add(catalogItem);
            await _dbContext.SaveChangesAsync(ct);

            await Send.OkAsync(response);


        }
    }   
    public class AddAnItemToCatalogValidator : Validator<AddAnItemToCatalogRequest>
    {
            public AddAnItemToCatalogValidator()
            {
                RuleFor(x => x.TelegramId).NotEmpty().WithMessage("Telegram ID is required.");
                RuleFor(x => x.ProductName).NotEmpty().WithMessage("Product name must not be empty.");
                RuleFor(x => x.Price).NotEmpty().WithMessage("Price is required.");
                RuleFor(x => x.ProductType).IsInEnum().WithMessage("Invalid product type.");
                RuleFor(x => x.AvailabilityStatus).IsInEnum().WithMessage("Invalid availability status.");
                RuleFor(x => x.Ingredients).Must(ing => ing != null && ing.Count > 0).WithMessage("Ingredients are required.");
                RuleFor(x => x.CreatorName).NotEmpty().WithMessage("Creator name is required.");
                RuleFor(x => x.CreatorSurname).NotEmpty().WithMessage("Creator surname is required.");
                

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

    public sealed record AddAnItemResponse
    {
        public Guid Id { get; init; }
    }
    public sealed record AddAnItemToCatalogRequest

    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.
        
        public long TelegramId { get; init; }
        public required string ProductName { get; init; }
        public required string CreatorName { get; init; }
        public required string CreatorSurname { get; init; }
        public decimal Price { get; init; }

        public AddProductType ProductType { get; init; }

        public AddProductAvailabilityStatus AvailabilityStatus { get; init; }
        // this is from JsonB part
        public List<string> Ingredients { get; set; } = [];
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; }

    }



}

