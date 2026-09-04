

namespace OrderService.Infrastructure.Entities.Buyer;


public enum BuyerGender
{
    Male,
    Female,

    Unknown,
}
public class CustomerProfile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid? CustomerId { get; set; }


    public int Age { get; set; }
    public required string Name { get; set; } 
    public required string Surname { get; set; }
    public required string PhoneNumber { get; set; }
    public BuyerGender Gender { get; set; } // We dont know exactly what is it gonna be... 
    public Customer Customer { get; set; } = null!;

}