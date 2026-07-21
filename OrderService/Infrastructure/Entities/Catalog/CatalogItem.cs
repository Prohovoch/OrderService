using OrderService.Infrastructure.Entities.Administrator;
using OrderService.Infrastructure.Entities.Draft;
namespace OrderService.Infrastructure.Entities.Catalog
{
    // Instead of TPH or TPT, i choose my own great combo.
    // easy to make more, no diff left joins, es to maintain, prob.... x.x
    public enum ProductType
    {
        Pizza, Burger, Soup, Salad, Sushi, Drinks
    }

    public enum ProductAvailabilityStatus
    {
        Available, OutOfStock, Discounted // for mvp is ok.
    }
    // our main model.
    public class CatalogItem
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid? AdminId { get; set;  }
        public Admin? Admin { get; set; }  
        public decimal Price { get; set; }
        public ProductType Type { get; set; }
        public ProductAvailabilityStatus AvailabilityStatus { get; set; }
        public int Quantity { get; set; } // not for dishes
        public string Description { get; set; } = string.Empty;
        public List<BucketItem> BucketItems { get; } = [];
        public ProductDetails Details { get; set; } = new ();
    }
    // JSONB column, idk what to add lol.
    public class ProductDetails
    {
        public List<string> Ingredients { get; set; } = [];
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; } // full dish weight without any toppings. too much for a mvp :/


    }
}