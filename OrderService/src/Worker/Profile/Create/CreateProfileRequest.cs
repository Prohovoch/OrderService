using FastEndpoints;
using System.Security.Cryptography.X509Certificates;

namespace OrderService.src.Worker.Profile.Create
{
    public enum Gender { Male, Female }
    public sealed record CreateProfileRequest
    {   
        
        [FromClaim]
        public Guid UserId {  get; init; }
        public string Name { get; init; } = null!;
        public string Surname { get; init; } = null!;
        public int Age { get; init; }
        public Gender? Gender { get; init; }
    }
}
