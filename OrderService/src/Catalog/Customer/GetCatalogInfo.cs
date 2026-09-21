using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Catalog.Customer
{
    public class GetCatalogInfo(ApplicationDbContext dbContext) : Endpoint<GetCatalogCustomerInfoRequest, List<CatalogResponse>>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/customer/catalog");
            Roles("customer");
            Validator<GetCatalogCustomerInfoValidator>();

        }


        public override async Task HandleAsync(GetCatalogCustomerInfoRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database on CATALOG page
            bool isCustomerExists = await _dbContext.Customers.AnyAsync(c => c.TgId == req.TelegramId, ct);
            if(isCustomerExists is false)
            {
                await Send.ForbiddenAsync();
                return;
            }

            var catalogResult = await _dbContext.Products.Select(r => new CatalogResponse
            {
                ItemId = r.Id,
                ProductName = r.ProductName,
                Price = r.Price,
                AvailabilityStatus = r.AvailabilityStatus == ProductAvailabilityStatus.Available ? GetProductAvailabilityStatus.Available : GetProductAvailabilityStatus.OutOfStock,
                // This is unbearable to watch. but it is not a dynamic, it is a static. and i dont want to bother using this Expression stuff frow now.
                // and yeah. C# cannot translate switch expression so...
                ProductType = r.Type
                == ProductType.Pizza ? GetProductType.Pizza
                : r.Type == ProductType.Burger ? GetProductType.Burger
                : r.Type == ProductType.Soup ? GetProductType.Soup
                : r.Type == ProductType.Salad ? GetProductType.Salad
                : r.Type == ProductType.Sushi ? GetProductType.Sushi
                : GetProductType.Drinks,
                Ingredients = r.Details.Ingredients,
                Volume = r.Details.Volume,
                Weight = r.Details.Weight


                
            }).ToListAsync(ct);

            await Send.OkAsync(catalogResult);
        }
    }
    public class GetCatalogCustomerInfoValidator : Validator<GetCatalogCustomerInfoRequest>
    {
        public GetCatalogCustomerInfoValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("Telegram ID is required.");

        }
    }
    public enum GetProductType
    {
        Pizza, Burger, Soup, Salad, Sushi, Drinks
    }
    public enum GetProductAvailabilityStatus
    {
        Available, OutOfStock, // Discounted only for soft delete. too bad idc abt this type of shit for now. We dont analize what they bout as for now
                               // for mvp it is ok.
    }
    public sealed record CatalogResponse

    {
       // return a list of calatog items.
       // use a flattenned dto without heritance.

        public Guid ItemId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal Price { get; init; }

        public GetProductType ProductType { get; init; }

        public GetProductAvailabilityStatus AvailabilityStatus { get; init; }
       // this is from JsonB part
        public List<string> Ingredients { get; set; } = [];
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; }

    }

    public sealed record GetCatalogCustomerInfoRequest
    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.
        [FromHeader("Customer-Telegram-Id")]
        public long TelegramId { get; init; }
    }

}


