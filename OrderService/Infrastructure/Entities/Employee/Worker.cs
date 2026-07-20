using OrderService.Infrastructure.Entities.Deal;


namespace OrderService.Infrastructure.Entities.Employee;

public class Worker
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public long TgId { get; set; }
    public WorkerProfile? Profile { get; set; }
    public List<Order> Orders { get; } = new List<Order>();



}
