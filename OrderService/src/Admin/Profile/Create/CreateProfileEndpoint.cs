using FastEndpoints;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Administrator;

namespace OrderService.src.Admin.Profile.Create
{
    // REPR endpoint
    public class CreateProfileEndpoint(ApplicationDbContext dbContext) : EndpointWithMapper<CreateProfileRequest, ProfileMapper>
    {

        public readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/admin/profile");
            Roles("admin");
            Validator<CreateProfileValidator>();
            
        }


        public override async Task HandleAsync(CreateProfileRequest req, CancellationToken ct)
        {
            AdminProfile entity = Map.ToEntity(req);

            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();
            await Send.OkAsync();
        }
    }
}
