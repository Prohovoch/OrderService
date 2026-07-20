
namespace OrderService.Infrastructure.Entities.Order
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid ProductId { get; set; } // snapshot.
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderItemDetails Details { get; set; } = new OrderItemDetails();
        // No details here, only happy path for now.
    }
    // JsonB column;
    public class OrderItemDetails
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; } //?
        // price at purchase.
        public decimal Price { get; set; }
        
    }
}
