using FastEndpoints;
using FluentValidation;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Administrator;



namespace OrderService.src.Admin.Profile
{
    // REPR endpoint
    public class CreateProfileEndpoint(ApplicationDbContext dbContext) : EndpointWithMapper<CreateProfileRequest, CreateRequestProfileMapper>
    {

        public readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/admin/profile");
            Roles("admin");
            Validator<CreateProfileValidator>();
            
        }


        public override async Task HandleAsync(CreateProfileRequest req, CancellationToken ct)
        {
            AdminProfile entity = Map.ToEntity(req);

            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();
            await Send.OkAsync();
        }
    }

    public class CreateRequestProfileMapper : RequestMapper<CreateProfileRequest, AdminProfile>
    {
    public override AdminProfile ToEntity(CreateProfileRequest r) => new()
    {
        AdminId = r.UserId,
        Name = r.Name,
        Surname = r.Surname,
        Age = r.Age,
        Gender = r.Gender switch
        {
            CreateReqGender.Male => AdminGender.Male,
            CreateReqGender.Female => AdminGender.Female,
            _ => null
        }
    };
}

// Usually validator is located in validator.cs file, but for KISS, it going to be here.
    public class CreateProfileValidator : Validator<CreateProfileRequest>
    {
    public CreateProfileValidator()
    {
        RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
            .NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.")
            .NotEmpty().WithMessage("Surname is required.");
        RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.");
    }
}


    public enum CreateReqGender { Male, Female }
    public sealed record CreateProfileRequest
    {
         [FromClaim]
         public Guid UserId { get; init; }
         public string Name { get; init; } = null!;
         public string Surname { get; init; } = null!;
         public int Age { get; init; }
         public CreateReqGender? Gender { get; init; }
    }

}