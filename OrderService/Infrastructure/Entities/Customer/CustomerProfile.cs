using OrderService.Infrastructure.Entities.Order;

namespace OrderService.Infrastructure.Entities.Customer;

public class CustomerProfile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CustomerId { get; set; }


    public int Age { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Gender { get; set; } = string.Empty; // We dont know exactly what is it gonna be... 
    public Customer Customer { get; set; } = null!;

}