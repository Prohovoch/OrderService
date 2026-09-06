using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Employee;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Worker.Profile
{
    // REPR endpoint
    public class ReadProfile(ApplicationDbContext dbContext) : Endpoint<ReadProfileRequest, ReadProfileResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/employee/profile/me");
            AllowAnonymous();
            Validator<ReadProfileValidator>();

        }
        public override async Task HandleAsync(ReadProfileRequest req, CancellationToken ct)
        {
            var mainId = await _dbContext.Workers.AsNoTracking().Where(x => x.TgId == req.TelegramId).Select(x => (Guid?)x.Id).FirstAsync(ct);
            
            var workerProfileEntity = await _dbContext.WorkerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.WorkerId == mainId, ct);

            if (workerProfileEntity is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            var resp = new ReadProfileResponse
            {
                Name = workerProfileEntity.Name,
                Surname = workerProfileEntity.Surname,
                Age = workerProfileEntity.Age,   
                Gender = workerProfileEntity.Gender switch
                {
                    WorkerGender.Male => ReadRequestGender.Male,
                    WorkerGender.Female => ReadRequestGender.Female,
                    _ => null

                }
            };
            await Send.OkAsync(resp);
        }


    }




        public class ReadProfileValidator : Validator<ReadProfileRequest>
        {
            public ReadProfileValidator()
            {
                RuleFor(x => x.TelegramId).NotNull().WithMessage("TelegramId is required.");
            }
        }
        public sealed record ReadProfileRequest
        {

            public long TelegramId { get; init; }

        }
        public enum ReadRequestGender { Male, Female }
        public sealed record ReadProfileResponse
        {
            public required string Name { get; init; }
            public required string Surname { get; init; }
            public int Age { get; init; }
            public ReadRequestGender? Gender { get; init; }
        }
    }

