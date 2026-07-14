using OrderService.Models;
namespace OrderService.Models.Draft;

public class DraftModel
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CustomerId { get; set; }
    public CustomerModel Customer { get; set; } = null!;
    public List<DraftItemModel> Items { get; } = new List<DraftItemModel>();
}