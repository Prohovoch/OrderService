
using OrderService.Models.Draft;

namespace OrderService.Models.Draft;

public class DraftItemModel
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DraftId { get; set; }
    public int Quantity { get; set; }
    public decimal SPrice { get; set; }
    public string SName { get; set; } = null!;
    public DraftModel Draft { get; set; } = null!;

}