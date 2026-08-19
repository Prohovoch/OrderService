using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Administrator;
using OrderService.Infrastructure.Persistence;


namespace OrderService.src.Admin.Profile


{
    // REPR endpoint
    public class ReadProfileEndpoint(ApplicationDbContext dbContext) : Endpoint<ReadProfileRequest, ReadProfileResponse, ReadProfileMapper>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/admin/profile");
            Roles("admin");
            Validator<ReadProfileValidator>();

        }
        public override async Task HandleAsync(ReadProfileRequest req, CancellationToken ct)
        {
            var adminProfileEntity = await _dbContext.AdminProfiles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.AdminId == req.UserId, ct);

            if (adminProfileEntity is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            var resp = Map.FromEntity(adminProfileEntity);
            await Send.OkAsync(resp);
        }
    }

    public class ReadProfileMapper : ResponseMapper<ReadProfileResponse, AdminProfile>
    {
    
    public override ReadProfileResponse FromEntity(AdminProfile e) => new()
    {
        
        Name = e.Name,
        Surname = e.Surname,
        Age = e.Age,
        Gender = e.Gender switch
        {
            AdminGender.Male => ReadReqGender.Male,
            AdminGender.Female => ReadReqGender.Female,
            _ => null
        }
    };
}
    public class ReadProfileValidator : Validator<ReadProfileRequest>
    {
    public ReadProfileValidator()
    {
        RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required.");
    }
}
    public sealed record ReadProfileRequest
    {
         [FromClaim]
         public Guid UserId { get; init; }
    }
    public enum ReadReqGender { Male, Female }
    public sealed record ReadProfileResponse
    {
         public string Name { get; init; } = null!;
         public string Surname { get; init; } = null!;
         public int Age { get; init; }
         public ReadReqGender? Gender { get; init; }
    }
}