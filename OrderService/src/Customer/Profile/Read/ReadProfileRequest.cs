using FastEndpoints;
using System.Security.Cryptography.X509Certificates;

namespace OrderService.src.Customer.Profile.Read
{

    public sealed record ReadProfileRequest
    {   [FromClaim]
        public Guid UserId { get; init; }
    }
}
