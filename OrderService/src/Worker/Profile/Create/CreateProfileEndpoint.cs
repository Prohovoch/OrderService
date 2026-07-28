using FastEndpoints;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Employee;

namespace OrderService.src.Worker.Profile.Create
{
    // REPR endpoint
    public class CreateProfileEndpoint(ApplicationDbContext dbContext) : EndpointWithMapper<CreateProfileRequest, ProfileMapper>
    {

        public readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/employee/profile");
            Roles("worker");
            Validator<CreateProfileValidator>();

        }


        public override async Task HandleAsync(CreateProfileRequest req, CancellationToken ct)
        {
            WorkerProfile entity = Map.ToEntity(req);

            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();
            await Send.OkAsync();
        }
    }
}
