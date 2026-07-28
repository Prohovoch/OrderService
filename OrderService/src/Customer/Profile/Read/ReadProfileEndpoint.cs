using FastEndpoints;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Buyer;
using Microsoft.EntityFrameworkCore;
namespace OrderService.src.Customer.Profile.Read
{
    // REPR endpoint
    public class ReadProfileEndpoint(ApplicationDbContext dbContext) : Endpoint<ReadProfileRequest, ReadProfileResponse, ProfileMapper>
    {

        public readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/customer/profile");
            Roles("customer");
            Validator<ReadProfileValidator>();
            
        }


        public override async Task HandleAsync(ReadProfileRequest req, CancellationToken ct)
        {
            var customerProfileEntity = await _dbContext.CustomerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.CustomerId == req.UserId, ct);

            if (customerProfileEntity is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            var resp = Map.FromEntity(customerProfileEntity);
            await Send.OkAsync(resp);
        }
    }
}
