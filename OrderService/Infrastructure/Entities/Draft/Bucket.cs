using OrderService.Infrastructure.Entities.Buyer;


namespace OrderService.Infrastructure.Entities.Draft;

public class Bucket
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<BucketItem> Items { get; } = [];
}