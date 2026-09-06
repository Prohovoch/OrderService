using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Employee;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Worker.Profile
{
    public class UpdateProfile(ApplicationDbContext dbContext) : Endpoint<UpdateProfileRequest>
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/employee/profile/me");
            AllowAnonymous();
            Validator<UpdateProfileValidator>();
        }
        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {
            var entityId = await _dbContext.Workers.Where(x => x.TgId == req.TelegramId).Select(x => x.Id).FirstAsync(ct);

            
            var profile = await _dbContext.WorkerProfiles.FirstOrDefaultAsync(p => p.WorkerId == entityId, ct);
            if (profile is null)
            {
                AddError("ProfileId:", "Profile object not found");
                await Send.ErrorsAsync();
                return;
            }

            profile.Name = req.Name ?? profile.Name;
            profile.Surname = req.Surname ?? profile.Surname;
            profile.Age = req.Age ?? profile.Age;
            profile.Gender = req.Gender.HasValue ? (WorkerGender)req.Gender.Value : profile.Gender;
            profile.PhoneNumber = req.PhoneNumber ?? profile.PhoneNumber;
         
            await Send.NoContentAsync();
        }


    }


    public class UpdateProfileValidator : Validator<UpdateProfileRequest>
    {
        public UpdateProfileValidator()
        {

            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("UserId required");
            RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.").When(x => x.Surname != null);
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.").When(x => x.Age != null);
            RuleFor(x => x.Gender).IsInEnum().When(x => x.Gender != null);
            RuleFor(x => x.PhoneNumber).NotEmpty().When(x => x.PhoneNumber != null).WithMessage("Phone number must not be null!");
        }
    }

    public enum UpdateGender { Male, Female }
    public sealed record UpdateProfileRequest
    {
        
        public long TelegramId { get; init; }
        public Guid ProfileId { get; init; }
        public string? Name { get; init; } 
        public string? Surname { get; init; } 
        public string? PhoneNumber { get; init; }
        public int? Age { get; init; }
        public UpdateGender? Gender { get; init; }
    }

}






