using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Employee;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Profile.Worker
{
    public class UpdateProfile(ApplicationDbContext dbContext) : Endpoint<UpdateProfileRequest>
    {
       

        public override void Configure()
        {
            Patch("api/employee/profile/me");
            AllowAnonymous();
            Validator<UpdateProfileValidator>();
        }
        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {
            var entityId = await dbContext.Workers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
         
            if (entityId is null)
            {
                Logger.LogWarning("No entity were found for Worker with TelegramId {TelegramId}.", req.TelegramId);
                await Send.NotFoundAsync();
                return;
            }
            

            var profile = await dbContext.WorkerProfiles.FirstOrDefaultAsync(p => p.WorkerId == entityId.Value, ct);
            if (profile is null)
            {
                Logger.LogWarning("Profile for Worker with WorkerId {WorkerId} not found.", entityId.Value);
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
                    UpdateGender.Male => WorkerGender.Male,
                    UpdateGender.Female => WorkerGender.Female,
                    _ => throw new ArgumentOutOfRangeException(nameof(req), $"out of range {req.Gender}"),
                };
            }

            
            await dbContext.SaveChangesAsync(ct);
            await Send.NoContentAsync();
            Logger.LogInformation("Updating profile for Worker");
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
            RuleFor(x => x.PhoneNumber).Matches(@"^(\+?7|8)\d{10}$").NotEmpty().When(x => x.PhoneNumber != null).WithMessage("Phone number must not be null!");
        }
    }

    public enum UpdateGender { Male, Female }
    public sealed record UpdateProfileRequest
    {
        [FromHeader("Worker-Telegram-Id")]
        public long TelegramId { get; init; }
        public Guid ProfileId { get; init; }
        public string? Name { get; init; } 
        public string? Surname { get; init; } 
        public string? PhoneNumber { get; init; }
        public int? Age { get; init; }
        public UpdateGender? Gender { get; init; }
    }

}






