using FastEndpoints;
using System.Security.Cryptography.X509Certificates;

namespace OrderService.src.Customer.Profile.Create
{
    public enum Gender { Male, Female }
    public class CreateProfileRequest
    {
        [FromClaim]
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public int Age { get; set; }
        public Gender? Gender { get; set; }
    }
}
