using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;


namespace OrderService.src.CustomerOnBoarding
{
    // REPR endpoint
    public class ReadProfileEndpoint(ApplicationDbContext dbContext) : Endpoint<ReadProfileRequest, ReadProfileResponse, GetRequestProfileMapper>
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

    public class ReadProfileValidator : Validator<ReadProfileRequest>
    {
        public ReadProfileValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required.");
        }
    }
    public class GetRequestProfileMapper : ResponseMapper<ReadProfileResponse, CustomerProfile>
    {
        public override ReadProfileResponse FromEntity(CustomerProfile e) => new()
        {
            Name = e.Name,
            Surname = e.Surname,
            Age = e.Age,
            Gender = e.Gender switch
            {
                BuyerGender.Male => GetRequestGender.Male,
                BuyerGender.Female => GetRequestGender.Female,
                _ => null
            }
        };

    }

    public sealed record ReadProfileRequest
    {
        [FromClaim]
        public Guid UserId { get; init; }
    }

    public enum GetRequestGender { Male, Female }
    public sealed record ReadProfileResponse
    {
        public string Name { get; init; } = null!;
        public string Surname { get; init; } = null!;
        public int Age { get; init; }
        public GetRequestGender? Gender { get; init; }
    }

}
