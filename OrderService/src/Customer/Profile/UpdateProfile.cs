using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Administrator;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Customer.Profile
{
    public class UpdateProfile(ApplicationDbContext dbContext) : Endpoint<UpdateProfileRequest>
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/customer/profile/me");
            Roles("customer");
            Validator<UpdateProfileValidator>();
        }
        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {
            var profile = await _dbContext.CustomerProfiles.FirstOrDefaultAsync(p => p.CustomerId == req.UserId, ct);
            if (profile == null)
            {
                AddError("ProfileId:", "Profile object not found");
                await Send.ErrorsAsync();
                return;
            }

            profile.Name = req.Name ?? profile.Name;
            profile.Surname = req.Surname ?? profile.Surname;
            profile.Age = req.Age ?? profile.Age;
            profile.Gender = req.Gender.HasValue ? (BuyerGender)req.Gender.Value : profile.Gender;
            await Send.NoContentAsync();
        }

    }
        
  

    public class UpdateProfileValidator : Validator<UpdateProfileRequest>
    {
        public UpdateProfileValidator()
        {

            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId required");
            RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.").When(x => x.Surname != null);
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.").When(x => x.Age != null);
            RuleFor(x => x.Gender).IsInEnum().When(x => x.Gender != null);
        }
    }
   
    public enum UpdateRequestGender { Male, Female }
    public sealed record UpdateProfileRequest
    {
        [FromClaim] 
        public Guid UserId { get; init; }
       

        public string? Name { get; init; }
        public string? Surname { get; init; } 
        public int? Age { get; init; }
        public UpdateRequestGender? Gender { get; init; }
    }

    
  }




