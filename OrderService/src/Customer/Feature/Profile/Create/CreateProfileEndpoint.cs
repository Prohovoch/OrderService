using FastEndpoints;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Buyer;
namespace OrderService.src.Customer.Feature.Profile.Create
{
    // REPR endpoint
    public class CreateProfileEndpoint(ApplicationDbContext dbContext) : EndpointWithMapper<CreateProfileRequest, ProfileMapper>
    {

        public readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/customer/profile");
            Roles("customer");
            Validator<ValidatorClass>();
            
        }


        public override async Task HandleAsync(CreateProfileRequest req, CancellationToken ct)
        {
            CustomerProfile entity = Map.ToEntity(req);

            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();
            await Send.OkAsync();
        }
    }
}
