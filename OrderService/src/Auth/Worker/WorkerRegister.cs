using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;


namespace OrderService.src.Auth.Worker
{
    public class WorkerRegister(ApplicationDbContext dbContext) : Endpoint<WorkerRegisterRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/register/worker");
            AllowAnonymous();
            Validator<RegisterCustomerValidator>();

        }


        public override async Task HandleAsync(WorkerRegisterRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database on a CATALOG page
            var isCustomerExists = await _dbContext.Customers.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (isCustomerExists is true)
            {
                Logger.LogWarning("Canceled. User is already registered in a system");
                await Send.ErrorsAsync();
                return;
            }

            var customer = new Customer
            {
                Id = Guid.CreateVersion7(),
                TgId = req.TelegramId
            };
            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync(ct);
            Logger.LogInformation("User is succesfully registered");
            await Send.OkAsync(new { Message = "Customer registered successfully." });







        }
    }

    public class RegisterCustomerValidator : Validator<WorkerRegisterRequest>
    {
        public RegisterCustomerValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("UserId is required.");

        }
    }



    public sealed record WorkerRegisterRequest
    {
        [FromHeader("Customer-Telegram-Id")]
        public long TelegramId { get; init; }

    }





}

