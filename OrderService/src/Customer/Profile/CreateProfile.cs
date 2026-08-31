using FastEndpoints;
using FluentValidation;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Buyer;

namespace OrderService.src.Customer.Profile
{
    // REPR endpoint
    public class CreateProfile(ApplicationDbContext dbContext) : EndpointWithMapper<CreateProfileRequest, CreateRequestProfileMapper>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/customer/profile");
            Roles("customer");
            Validator<CreateProfileValidator>();
            
        }


        public override async Task HandleAsync(CreateProfileRequest req, CancellationToken ct)
        {
            CustomerProfile entity = Map.ToEntity(req);

            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync(ct);
            await Send.OkAsync();
        }
    }
    public class CreateProfileValidator : Validator<CreateProfileRequest>
    {
        public CreateProfileValidator()
        {
            RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
                .NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.")
                .NotEmpty().WithMessage("Surname is required.");
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.");
            RuleFor(x => x.PhoneNumber).Matches(@"^(\+?7|8)\d{10}$").NotEmpty().WithMessage("Phone number from Russian Federation");
            RuleFor(x => x.Gender).IsInEnum();
        }
    }


    public class CreateRequestProfileMapper : RequestMapper<CreateProfileRequest, CustomerProfile>
    {
        public override CustomerProfile ToEntity(CreateProfileRequest r) => new()
        {
            CustomerId = r.UserId,
            Name = r.Name,
            Surname = r.Surname,
            Age = r.Age,
  
            PhoneNumber = r.PhoneNumber,

            Gender = r.Gender switch
            {
                CreateRequestGender.Male => BuyerGender.Male,
                CreateRequestGender.Female => BuyerGender.Female,
                _ => null
            }
        };
    }

    public enum CreateRequestGender { Male, Female }
    public sealed record CreateProfileRequest
    {
        [FromClaim]
        public Guid UserId { get; init; }
        public required string Name { get; init; } 
        public required string Surname { get; init; } 
        public required string PhoneNumber { get; init; }
        public int Age { get; set; }
        public CreateRequestGender? Gender { get; init; }
    }

}
