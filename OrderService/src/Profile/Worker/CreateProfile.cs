using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Employee;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Profile.Worker
{
    // REPR endpoint
    public class CreateProfile(ApplicationDbContext dbContext) : Endpoint<CreateWorkerProfileRequest>
    {

       

        public override void Configure()
        {
            Post("api/employee/profile");
            AllowAnonymous();
            Validator<CreateProfileValidator>();
            // post mvp stage -> docs.
        }


        public override async Task HandleAsync(CreateWorkerProfileRequest req, CancellationToken ct)
        {


            var entityId = await dbContext.Workers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            if (entityId is null)
            {
                Logger.LogWarning("Worker with TelegramId {TelegramId} not found.", req.TelegramId);
                await Send.NotFoundAsync();
                return;
            }

            var workerProfile = new WorkerProfile
            {
                WorkerId = entityId.Value,
                Name = req.Name,
                Surname = req.Surname,
                Age = req.Age,

                PhoneNumber = req.PhoneNumber,

                Gender = req.Gender switch
                {
                    CreateReqGender.Male => WorkerGender.Male,
                    CreateReqGender.Female => WorkerGender.Female,

                    _ => throw new ArgumentOutOfRangeException(nameof(req), "out of range ex"),
                },


            };
            
            Logger.LogInformation("Creating profile for WorkerId {WorkerId} with Name {Name} and Surname {Surname}.", workerProfile.WorkerId, workerProfile.Name, workerProfile.Surname);
            dbContext.Add(workerProfile);
            await dbContext.SaveChangesAsync(ct);
            await Send.OkAsync();
        }

    }
}
    public class CreateProfileValidator : Validator<CreateWorkerProfileRequest>
    {
        public CreateProfileValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required.");
            RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
                .NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.")
                .NotEmpty().WithMessage("Surname is required.");
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.");
            RuleFor(x => x.Gender).IsInEnum();
            RuleFor(x => x.PhoneNumber).Matches(@"^(\+?7|8)\d{10}$").NotEmpty().WithMessage("Phone must not be empty!");
        }
    }
   


    public enum CreateReqGender { Male, Female }
    public sealed record CreateWorkerProfileRequest
    {
        [FromHeader("Worker-Telegram-Id")]
        public long TelegramId { get; init; }
        public required string Name { get; init; } 
        public required string Surname { get; init; } 
        public required string PhoneNumber { get; init; }
        public int Age { get; init; }
        public CreateReqGender Gender { get; init; }
    }

