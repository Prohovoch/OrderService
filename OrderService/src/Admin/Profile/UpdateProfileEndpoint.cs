using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Administrator;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Admin.Profile
{
    public class UpdateProfileEndpoint(ApplicationDbContext dbContext) : EndpointWithMapper<UpdateProfileRequest, UpdateProfileMapper>
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Put("api/administrator/profile");
            Roles("admin");
            Validator<UpdateProfileValidator>();
        }
        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {
            var adminProfileEntity = Map.ToEntity(req);

            var affectedRows = await _dbContext.AdminProfiles.Where(c => c.Id == adminProfileEntity.Id).ExecuteUpdateAsync(c => c.SetProperty(c => c.Name, c => adminProfileEntity.Name)
            .SetProperty(c => c.Surname, c => adminProfileEntity.Surname)
            .SetProperty(c => c.Age, c => adminProfileEntity.Age)
            .SetProperty(c => c.Gender, adminProfileEntity.Gender), ct);
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
public class UpdateProfileMapper : RequestMapper<UpdateProfileRequest, AdminProfile>
{
    public override AdminProfile ToEntity(UpdateProfileRequest r) => new()
    {
        AdminId = r.UserId,
        Name = r.Name,
        Surname = r.Surname,
        Age = r.Age,
        Gender = r.Gender switch
        {
            UpdateGender.Male => AdminGender.Male,
            UpdateGender.Female => AdminGender.Female,
            _ => null
        }
    };

}
public enum UpdateGender { Male, Female }
public sealed record UpdateProfileRequest
{
    [FromClaim]
    public Guid UserId { get; init; }
    public string Name { get; init; } = null!;
    public string Surname { get; init; } = null!;
    public int Age { get; init; }
    public UpdateGender? Gender { get; init; }
}

}
    
   




