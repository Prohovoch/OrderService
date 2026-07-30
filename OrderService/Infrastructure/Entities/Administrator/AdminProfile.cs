
namespace OrderService.Infrastructure.Entities.Administrator
{
    public enum AdminGender
    {
        Male, Female
    }
    public class AdminProfile
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid AdminId { get; set; }
        public int Age { get; set; }
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public AdminGender? Gender { get; set; } // We dont know exactly what is it gonna be... 
        public Admin Admin { get; set; } = null!;
    }
}
