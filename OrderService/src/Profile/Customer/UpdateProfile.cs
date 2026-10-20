using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Profile.Customer
{
    public class UpdateProfile(ApplicationDbContext dbContext) : Endpoint<UpdateProfileRequest>
    {
       

        public override void Configure()
        {
            Patch("api/customer/profile/me");
            AllowAnonymous();
            Validator<UpdateProfileValidator>();
        }
        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {


            var entityId = await dbContext.Customers
                .Where(p => p.TgId == req.TelegramId).AsNoTracking()
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(ct);
           
            if (entityId is null)
            {
                Logger.LogWarning("Customer with TelegramId {TelegramId} not found.", req.TelegramId);
                await Send.ForbiddenAsync();
                return;
            
            }
            var profile = await dbContext.CustomerProfiles.FirstOrDefaultAsync(p => p.CustomerId == entityId.Value, ct);
            if (profile is null)
            {
                Logger.LogWarning("Profile for Customer with TelegramId {TelegramId} not found.", req.TelegramId);
                await Send.NotFoundAsync();
                return;
            }

            profile.Name = req.Name ?? profile.Name;
            profile.Surname = req.Surname ?? profile.Surname;
            profile.Age = req.Age ?? profile.Age;
            profile.PhoneNumber = req.PhoneNumber ?? profile.PhoneNumber;
            if (req.Gender.HasValue)
            {
                profile.Gender = req.Gender.Value switch
                {
                    UpdateRequestGender.Male => BuyerGender.Male,
                    UpdateRequestGender.Female => BuyerGender.Female,
                    _ => throw new ArgumentOutOfRangeException(nameof(req), $"out of range {req.Gender}"),
                };
            }

            Logger.LogInformation("Updating profile for Customer with TelegramId {TelegramId}.", req.TelegramId);
            
            await dbContext.SaveChangesAsync(ct);
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
            RuleFor(x => x.PhoneNumber).Matches(@"^(\+?7|8)\d{10}$").NotEmpty().When(x => x.PhoneNumber != null).WithMessage("PhoneNumber cannot be null!");
        }
    }
   
    public enum UpdateRequestGender { Male, Female }
    public sealed record UpdateProfileRequest
    {
        [FromHeader("Customer-Telegram-Id")]
        public long TelegramId { get; init; }
       

        public string? Name { get; init; }
        public string? Surname { get; init; } 
        public int? Age { get; init; }

        public string? Address { get; init; }
        public string? PhoneNumber { get; init; }
        public UpdateRequestGender? Gender { get; init; }
    }

    
  }




