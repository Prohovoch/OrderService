using OrderService.Infrastructure.Entities.Catalog;

namespace OrderService.Infrastructure.Entities.Cart;


// Bucket item has a many-to-one relationship with Bucket and Product.
// It represents a single item in a shopping cart, including the product and quantity.
// ProductId for dynamic data update;
public class BucketItem
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid BucketId { get; set; }
    public Guid ProductId { get; set; }
    public int BucketItemQuantity { get; set; } // make sure that we can get it from catalog.
    public Bucket? Bucket { get; set; }  //  we dont exactly when it will be created. it could potentially be in catalog.
    public CatalogItem Product { get; set; } = null!;

    // Bucket item ->  get some items -> check quantity on product ->
    // if quantity is enough -> add to order -> wipe quantity from product, check if 0 then close from
    // customers ability to get it.
    // -> remove from bucket???.
}