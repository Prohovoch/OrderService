using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Customer.Profile
{
    public class UpdateCustomerProfileEndpoint(ApplicationDbContext dbContext) : EndpointWithMapper<UpdateProfileRequest,  UpdateProfileMapper>
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Put("api/customer/profile");
            AllowAnonymous();
            Validator<UpdateProfileValidator>();
        }
        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {
            var customerProfileEntity = Map.ToEntity(req);

            var affectedRows = await _dbContext.CustomerProfiles.Where(c => c.Id == customerProfileEntity.Id).ExecuteUpdateAsync(c => c.SetProperty(c => c.Name, c => customerProfileEntity.Name)
            .SetProperty(c => c.Surname, c => customerProfileEntity.Surname)
            .SetProperty(c => c.Age, c => customerProfileEntity.Age)
            .SetProperty(c => c.Gender, customerProfileEntity.Gender), ct);
            if (affectedRows == 0)
            {
                await Send.NotFoundAsync();
                return;
            }
            await Send.NoContentAsync();
        }


        }
  

    public class UpdateProfileValidator : Validator<UpdateProfileRequest>
    {
        public UpdateProfileValidator()
        {
      
            RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.")
                .NotEmpty().WithMessage("Surname is required.");
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.");
            RuleFor(x => x.Gender).IsInEnum();
        }
    }
    public class UpdateProfileMapper : RequestMapper<UpdateProfileRequest, CustomerProfile>
    {
        public override CustomerProfile ToEntity(UpdateProfileRequest r) => new()
        {
            CustomerId = r.UserId,
            Name = r.Name,
            Surname = r.Surname,
            Age = r.Age,
            Gender = r.Gender switch
            {
                UpdateRequestGender.Male => BuyerGender.Male,
                UpdateRequestGender.Female => BuyerGender.Female,
                _ => null
            }
        };

    }
    public enum UpdateRequestGender { Male, Female }
    public sealed record UpdateProfileRequest
    {

        public Guid UserId { get; init; }
        public string Name { get; init; } = null!;
        public string Surname { get; init; } = null!;
        public int Age { get; init; }
        public UpdateRequestGender? Gender { get; init; }
    }

    
  }




