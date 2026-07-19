using OrderService.Infrastructure.Entities.Worker;
using OrderService.Infrastructure.Entities.Customer;
namespace OrderService.Infrastructure.Entities.Order
{
    // One customer can create many orders, but one order can only belong to one customer.
    // One worker can handle many orders, but one order can only be handled by one worker. 
    // One order can have many products from catalog, but one product can only belong to one order.
    public enum OrderStatus
    {
        Created, // user created order, but not yet picked up by worker
        Processing, // worker picked up order and is processing it
        Completed, // worker completed order
        Cancelled // worker or user cancelled order //???
    }

    public class OrderModel
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid? CustomerId { get; set; }
        public Guid? WorkerId { get; set; }
        public CustomerModel? Customer { get; set; } = null!;
        public WorkerModel? Worker { get; set; } = null!;
        public List<OrderItemModel> Items { get; } = new List<OrderItemModel>();

        public OrderStatus Status { get; set; }
        

        
    }
}
