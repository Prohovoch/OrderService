using OrderService.Infrastructure.Entities.Order;


namespace OrderService.Infrastructure.Entities.Worker
{
    public class Worker
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public long TgId { get; set; }
        public WorkerProfile? Profile { get; set; }
        public List<Order.Order> Orders { get; } = new List<Order.Order>();



    }
}
