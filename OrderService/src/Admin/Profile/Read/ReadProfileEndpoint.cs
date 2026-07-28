using FastEndpoints;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Administrator;
using Microsoft.EntityFrameworkCore;
namespace OrderService.src.Admin.Profile.Read
{
    // REPR endpoint
    public class ReadProfileEndpoint(ApplicationDbContext dbContext) : Endpoint<ReadProfileRequest, ReadProfileResponse, ProfileMapper>
    {

        public readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/admin/profile");
            Roles("administrator");
            Validator<ReadProfileValidator>();

        }


        public override async Task HandleAsync(ReadProfileRequest req, CancellationToken ct)
        {
            var adminProfileEntity = await _dbContext.AdminProfiles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.AdminId == req.UserId, ct);

            if (adminProfileEntity is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            var resp = Map.FromEntity(adminProfileEntity);
            await Send.OkAsync(resp);
        }
    }
}
