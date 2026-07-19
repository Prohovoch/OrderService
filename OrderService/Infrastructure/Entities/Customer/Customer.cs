using OrderService.Infrastructure.Entities.Draft;
using OrderService.Infrastructure.Entities.Order;

namespace OrderService.Infrastructure.Entities.Customer;

public class Customer
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public long TgId { get; set; }
    public CustomerProfile? Profile { get; set; }
    public Draft.Draft? Draft { get; set; }
    public List<Order.Order> Orders { get; } = new List<Order.Order>();


}