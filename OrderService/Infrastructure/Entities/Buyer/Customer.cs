using OrderService.Infrastructure.Entities.Draft;
using OrderService.Infrastructure.Entities.Deal;

namespace OrderService.Infrastructure.Entities.Buyer;

public class Customer
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public long TgId { get; set; }
    public CustomerProfile? Profile { get; set; }
    public Bucket? Draft { get; set; }
    public List<Order> Orders { get; } = [];




    // an another idea for mb future projects, just an example.
    // public DateTime DeletedAt { get; set; } = DateTime.Utc.Now;
}