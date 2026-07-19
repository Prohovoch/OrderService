using OrderService.Infrastructure.Entities.Order;


namespace OrderService.Infrastructure.Entities.Worker
{
    public class WorkerModel
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public long TgId { get; set; }
        public WorkerProfileModel? Profile { get; set; }
        public List<OrderModel> Orders { get; } = new List<OrderModel>();



    }
}
