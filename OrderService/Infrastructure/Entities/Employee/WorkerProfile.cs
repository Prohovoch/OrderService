

namespace OrderService.Infrastructure.Entities.Employee;

public enum WorkerGender
{
    Male,
    Female,

    Unknown,
}
public class WorkerProfile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid WorkerId { get; set; }
    public int Age { get; set; }
    public required string Name { get; set; } 
    public required string Surname { get; set; } 
    public required string PhoneNumber { get; set; }
    public WorkerGender Gender { get; set; } // We dont know exactly what is it gonna be... 
    public Worker Worker { get; set; } = null!;
}
