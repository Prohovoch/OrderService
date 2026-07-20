using OrderService.Infrastructure.Entities.Admin;
namespace OrderService.Infrastructure.Entities.Catalog
{
    // Instead of TPH or TPT, i choose my own great combo.
    // easy to make more, no diff left joins, es to maintain, prob.... x.x
    public enum ProductType
    {
        Pizza, Burger, Salad, Sushi, Drinks
    }
    // our main model.
    public class CatalogItem
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid? AdminId { get; set;  }
        public Admin.Admin? Admin { get; set; }  
        public decimal Price { get; set; }
        public ProductType Type { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true; // some kind of soft delete, but not really, just for the admin to hide some products.


        public ProductDetails Details { get; set; } = new ProductDetails();
    }
    // JSONB column, idk what to add lol.
    public class ProductDetails
    {
        public List<string> Ingredients { get; set; } = new List<string>();
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; } // full dish weight without any toppings. too much for a mvp :/


    }
}