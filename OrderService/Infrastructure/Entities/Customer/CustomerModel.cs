using OrderService.Infrastructure.Entities.Draft;
using OrderService.Infrastructure.Entities.Order;

namespace OrderService.Infrastructure.Entities.Customer;

public class CustomerModel
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public long TgId { get; set; }
    public CustomerProfileModel? Profile { get; set; }
    public DraftModel? Draft { get; set; }
    public List<OrderModel> Orders { get; } = new List<OrderModel>();


}