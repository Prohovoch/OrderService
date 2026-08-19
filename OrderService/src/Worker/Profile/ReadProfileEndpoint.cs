using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Employee;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Worker.Profile
{
    // REPR endpoint
    public class ReadProfileEndpoint(ApplicationDbContext dbContext) : Endpoint<ReadProfileRequest, ReadProfileResponse, ReadMapper>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/employee/profile");
            Roles("employee");
            Validator<ReadProfileValidator>();

        }
        public override async Task HandleAsync(ReadProfileRequest req, CancellationToken ct)
        {
            var adminProfileEntity = await _dbContext.WorkerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.WorkerId == req.UserId, ct);

            if (adminProfileEntity is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            var resp = Map.FromEntity(adminProfileEntity);
            await Send.OkAsync(resp);
        }
    }

public class ReadMapper : ResponseMapper<ReadProfileResponse, WorkerProfile>
{

    public override ReadProfileResponse FromEntity(WorkerProfile e) => new()
    {

        Name = e.Name,
        Surname = e.Surname,
        Age = e.Age,
        Gender = e.Gender switch
        {
            WorkerGender.Male => ReadRequestGender.Male,
            WorkerGender.Female => ReadRequestGender.Female,
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
public enum ReadRequestGender { Male, Female }
public sealed record ReadProfileResponse
{
    public string Name { get; init; } = null!;
    public string Surname { get; init; } = null!;
    public int Age { get; init; }
    public ReadRequestGender? Gender { get; init; }
}
}