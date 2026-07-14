using OrderService.Models.Draft;
namespace OrderService.Models.Customer;

public class CustomerModel
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public long TgId { get; set; }
    public CustomerProfileModel? Profile { get; set; }
    public DraftModel? Draft { get; set; }
}