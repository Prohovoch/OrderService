using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Catalog;

namespace OrderService.src.Catalog.Admin
{
    public class GetCatalogAdInfoEndpoint(ApplicationDbContext dbContext) : EndpointWithoutRequest
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/catalog");
            Roles("admin");


        }


        public override async Task HandleAsync(CancellationToken ct)
        {
            // checks if the product exists in the database on CATALOG page

            var catalogResult = _dbContext.Products.Select(r => new CatalogResponse
            {
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
                Ingredients = r.Details.Ingredients.ToList(),
                Volume = r.Details.Volume,
                Weight = r.Details.Weight



            }).ToListAsync(ct);

            await Send.OkAsync(catalogResult);
        }
    }

    public enum GetProductType
    {
        Pizza, Burger, Soup, Salad, Sushi, Drinks
    }
    public enum GetProductAvailabilityStatus
    {
        Available, OutOfStock, // Discounted only for soft delete. too bad idc abt this type of shit for now. We dont analize what they bout as for now
                               // for mvp is ok.
    }
    public sealed record CatalogResponse

    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.

        public string ProductName { get; init; } = string.Empty;
        public decimal Price { get; init; }

        public GetProductType ProductType { get; init; }

        public GetProductAvailabilityStatus AvailabilityStatus { get; init; }
        // this is from JsonB part
        public List<string> Ingredients { get; set; } = [];
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; }

    }



}


