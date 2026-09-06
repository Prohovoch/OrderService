using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Administrator;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Admin.Profile
{
    public class UpdateProfile(ApplicationDbContext dbContext) : Endpoint<UpdateProfileRequest>
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Patch("api/administrator/profile/me");
            Roles("admin");
            Validator<UpdateProfileValidator>();
        }
        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {
            var entityId = await _dbContext.Admins.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => x.Id).FirstAsync(ct);
            var profile = await _dbContext.AdminProfiles.FirstOrDefaultAsync(p => p.AdminId == entityId, ct);

            if (profile is null)
            {
                AddError("profileID","No object was found.");
                await Send.ErrorsAsync();
                return;
            }

            profile.Name = req.Name ?? profile.Name;
            profile.Surname = req.Surname ?? profile.Surname;
            profile.Age = req.Age ?? profile.Age;
            profile.Gender = req.Gender.HasValue ? (AdminGender)req.Gender.Value : profile.Gender ;

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
    }
}

public enum UpdateGender { Male, Female }
public sealed record UpdateProfileRequest
{
    

    public long TelegramId { get; init; }
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public int? Age { get; init; }
    public UpdateGender? Gender { get; init; }
}

}
    
   




