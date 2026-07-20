

namespace OrderService.Infrastructure.Entities.Employee;

public class WorkerProfile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid WorkerId { get; set; }
    public int Age { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Gender { get; set; } = string.Empty; // We dont know exactly what is it gonna be... 
    public Worker Worker { get; set; } = null!;
}
