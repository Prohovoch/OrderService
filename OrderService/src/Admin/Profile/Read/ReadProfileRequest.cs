using FastEndpoints;
using System.Security.Cryptography.X509Certificates;

namespace OrderService.src.Admin.Profile.Read
{

    public sealed record ReadProfileRequest
    {
        [FromClaim]
        public Guid UserId { get; init; }
    }
}
