
namespace OrderService.Infrastructure.Entities.Order
{
    // One customer can create many orders, but one order can only belong to one customer.
    // One worker can handle many orders, but one order can only be handled by one worker. 
    // One order can have many products from catalog.
    public enum OrderStatus
    {
        Created, // user created order, but not yet picked up by worker
        Processing, // worker picked up order and is processing it
        Completed, // worker completed order
        Cancelled // worker or user cancelled order //???
    }

    public class Order
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid? CustomerId { get; set; }
        public Guid? WorkerId { get; set; }
        public Customer.Customer? Customer { get; set; }
        public Worker.Worker? Worker { get; set; }
        public List<OrderItem> Items { get; } = new List<OrderItem>();

        public OrderStatus Status { get; set; }
        

        
    }
}
