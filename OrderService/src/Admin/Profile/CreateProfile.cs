using FastEndpoints;
using FluentValidation;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Administrator;
using Microsoft.EntityFrameworkCore;



namespace OrderService.src.Admin.Profile
{
    // REPR endpoint
    public class CreateProfile(ApplicationDbContext dbContext) : Endpoint<CreateProfileRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/admin/profile");
            AllowAnonymous();
            Validator<CreateProfileValidator>();
            
        }


        public override async Task HandleAsync(CreateProfileRequest req, CancellationToken ct)
        {

            var entityId = await _dbContext.Admins.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => x.Id).FirstAsync(ct);
            var profile = new AdminProfile
            {
                AdminId = entityId,
                Name = req.Name,
                Surname = req.Surname,
                Age = req.Age,
                PhoneNumber = req.PhoneNumber,
                Gender = req.Gender switch

                {
                    CreateReqGender.Male => AdminGender.Male,
                    CreateReqGender.Female => AdminGender.Female,
                    _ => null
                }
            };
            
            _dbContext.Add(profile);
            await _dbContext.SaveChangesAsync(ct);
            await Send.OkAsync();

                
                
        }
    }


// Usually validator is located in validator.cs file, but for KISS, it going to be here.
    public class CreateProfileValidator : Validator<CreateProfileRequest>
    {
    public CreateProfileValidator()
    {
        RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required");
        RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
            .NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.")
            .NotEmpty().WithMessage("Surname is required.");
        RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.");
        
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number must not be null!");
        
        }
}


    public enum CreateReqGender { Male, Female }

    
    public sealed record CreateProfileRequest
    {
    
         public long TelegramId { get; init; }
         public required string Name { get; init; } 
         public required string Surname { get; init; } 
         public required string PhoneNumber { get; init; } 
         public int Age { get; init; }
         public CreateReqGender? Gender { get; init; }
    }

}