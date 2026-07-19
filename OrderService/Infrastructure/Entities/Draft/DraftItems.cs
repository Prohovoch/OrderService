using OrderService.Infrastructure.Entities.Catalog;

namespace OrderService.Infrastructure.Entities.Draft;


// Draft item has a many-to-one relationship with DraftModel and ProductModel.
// It represents a single item in a draft order, including the product and quantity.
// ProductId for dynamic data update;
public class DraftItemModel
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DraftId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DraftModel Draft { get; set; } = null!;
    public ProductModel Product { get; set; } = null!;

}