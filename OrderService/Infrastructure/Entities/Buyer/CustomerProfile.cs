

namespace OrderService.Infrastructure.Entities.Buyer;


public enum BuyerGender
{
    Male,
    Female
}
public class CustomerProfile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CustomerId { get; set; }


    public int Age { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public BuyerGender? Gender { get; set; } // We dont know exactly what is it gonna be... 
    public Customer Customer { get; set; } = null!;

}