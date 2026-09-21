using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Catalog.Admin
{
    public class GetCatalogAdInfo(ApplicationDbContext dbContext) : Endpoint<GetCatalogAdInfoRequest, List<CatalogResponse>>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/admin/catalog");
            AllowAnonymous();
            Validator<GetCatalogAdInfoValidator>();

        }


        public override async Task HandleAsync(GetCatalogAdInfoRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database on CATALOG page
            bool isAdminExists = await _dbContext.Admins.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (isAdminExists is false)
            {
                await Send.ForbiddenAsync();
                return;
            }
            var catalogResult = await _dbContext.Products.AsNoTracking().Select(r => new CatalogResponse
            {
                ItemId = r.Id,
                CreatorName = r.CreatorName,
                CreatorSurname = r.CreatorSurname,
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
                : GetProductType.Drinks, // i want to vommit lol
                Ingredients = r.Details.Ingredients,
                Volume = r.Details.Volume,
                Weight = r.Details.Weight



            }).ToListAsync(ct);

            await Send.OkAsync(catalogResult);
        }
    }
    public class GetCatalogAdInfoValidator : Validator<GetCatalogAdInfoRequest>
    {
        public GetCatalogAdInfoValidator()
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
                               // for mvp is ok.
    }
    public sealed record CatalogResponse

    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.

        public Guid ItemId { get; init; }
        public required string CreatorName { get; init; }
        public required string CreatorSurname { get; init; } 
        public required string ProductName { get; init; }
        public decimal Price { get; init; }

        public GetProductType ProductType { get; init; }

        public GetProductAvailabilityStatus AvailabilityStatus { get; init; }
        // this is from JsonB part
        public List<string> Ingredients { get; set; } = [];
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; }

    }

    public sealed record GetCatalogAdInfoRequest
    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.
        [FromHeader("Admin-Telegram-Id")]
        public long TelegramId { get; init; }
    }

}


