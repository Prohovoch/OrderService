using OrderService.Infrastructure.Entities.Customer;


namespace OrderService.Infrastructure.Entities.Draft;

public class Draft
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CustomerId { get; set; }
    public Customer.Customer Customer { get; set; } = null!;
    public List<DraftItem> Items { get; } = new List<DraftItem>();
}