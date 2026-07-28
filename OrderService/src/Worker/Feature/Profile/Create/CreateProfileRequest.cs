using FastEndpoints;
using System.Security.Cryptography.X509Certificates;

namespace OrderService.src.Worker.Feature.Profile.Create
{
    public enum Gender { Male, Female }
    public sealed record CreateProfileRequest(
        [property: FromClaim]
        Guid UserId,
        string Name,
        string Surname,
        int Age,
        Gender? Gender
    );
}
