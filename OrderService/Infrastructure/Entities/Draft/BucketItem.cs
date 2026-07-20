using OrderService.Infrastructure.Entities.Catalog;

namespace OrderService.Infrastructure.Entities.Draft;


// Draft item has a many-to-one relationship with DraftModel and ProductModel.
// It represents a single item in a draft order, including the product and quantity.
// ProductId for dynamic data update;
public class BucketItem
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid BucketId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Bucket Bucket { get; set; } = null!;
    public CatalogItem Product { get; set; } = null!;

    // Bucket item ->  get some items -> check quantity on product ->
    // if quantity is enough -> add to order -> wipe quantity from product, check if 0 then close from
    // customers ability to get it.
    // -> remove from bucket???.
}