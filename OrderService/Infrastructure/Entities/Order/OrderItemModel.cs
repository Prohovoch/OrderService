
namespace OrderService.Infrastructure.Entities.Order
{
    public class OrderItemModel
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid ProductId { get; set; } // snapshot.
        public Guid OrderId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        // price at purchase.
        public decimal SPrice { get; set; }
        public OrderModel Order { get; set; } = null!;
        // No details here, only happy path for now.
    }
}
