using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Employee;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Customer.Profile;


namespace OrderService.src.Worker.Profile
{
    // REPR endpoint
    public class CreateProfile(ApplicationDbContext dbContext) : Endpoint<CreateWorkerProfileRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/employee/profile");
            AllowAnonymous();
            Validator<CreateProfileValidator>();

        }


        public override async Task HandleAsync(CreateWorkerProfileRequest req, CancellationToken ct)
        {


            var mainId = await _dbContext.Workers.Where(x => x.TgId == req.TelegramId).Select(x => x.Id).FirstAsync(ct);


            var workerProfile = new WorkerProfile
            {
                WorkerId = mainId,
                Name = req.Name,
                Surname = req.Surname,
                Age = req.Age,

                PhoneNumber = req.PhoneNumber,

                Gender = req.Gender switch
                {
                    CreateReqGender.Male => WorkerGender.Male,
                    CreateReqGender.Female => WorkerGender.Female,

                    _ => null,
                },


            };
            _dbContext.Add(workerProfile);
            await _dbContext.SaveChangesAsync(ct);
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
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone must not be empty!");
        }
    }
   


    public enum CreateReqGender { Male, Female }
    public sealed record CreateWorkerProfileRequest
    {

        public long TelegramId { get; init; }
        public required string Name { get; init; } 
        public required string Surname { get; init; } 
        public required string PhoneNumber { get; init; }
        public int Age { get; init; }
        public CreateReqGender? Gender { get; init; }
    }

