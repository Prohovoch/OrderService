using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Profile.Customer
{
    public class UpdateProfile(ApplicationDbContext dbContext) : Endpoint<UpdateProfileRequest>
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/customer/profile/me");
            AllowAnonymous();
            Validator<UpdateProfileValidator>();
        }
        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {


            var entityId = await _dbContext.Customers
                .Where(p => p.TgId == req.TelegramId).AsNoTracking()
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(ct);
           
            if (entityId is null)
            {
                await Send.ForbiddenAsync();
                return;
            
            }
            var profile = await _dbContext.CustomerProfiles.FirstOrDefaultAsync(p => p.CustomerId == entityId.Value, ct);
            if (profile is null)
            {
               
                await Send.NotFoundAsync();
                return;
            }

            profile.Name = req.Name ?? profile.Name;
            profile.Surname = req.Surname ?? profile.Surname;
            profile.Age = req.Age ?? profile.Age;
            profile.Gender = req.Gender.HasValue ? (BuyerGender)req.Gender.Value : profile.Gender;
            profile.PhoneNumber = req.PhoneNumber ?? profile.PhoneNumber;
            await Send.NoContentAsync();
        }

    }
        
  

    public class UpdateProfileValidator : Validator<UpdateProfileRequest>
    {
        public UpdateProfileValidator()
        {

            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId required");
            RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.").When(x => x.Surname != null);
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.").When(x => x.Age != null);
            RuleFor(x => x.Gender).IsInEnum().When(x => x.Gender != null);
            RuleFor(x => x.PhoneNumber).NotEmpty().When(x => x.PhoneNumber != null).WithMessage("PhoneNumber cannot be null!");
        }
    }
   
    public enum UpdateRequestGender { Male, Female, Unknown }
    public sealed record UpdateProfileRequest
    {
       
        public long TelegramId { get; init; }
       

        public string? Name { get; init; }
        public string? Surname { get; init; } 
        public int? Age { get; init; }

        public string? Address { get; init; }
        public string? PhoneNumber { get; init; }
        public UpdateRequestGender? Gender { get; init; }
    }

    
  }




