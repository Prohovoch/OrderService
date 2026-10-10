using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Deal.Worker;
using OrderService.src.Security.Helpers;


namespace OrderService.src.Auth.Worker
{
    public class WorkerOtpGeneration(ApplicationDbContext dbContext, OtpGenerate otpService) : Endpoint<WorkerOtpGenerationRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly OtpGenerate _otpService = otpService;

        public override void Configure()
        {
            Post("api/worker/otp/generate");
            AllowAnonymous();
            Validator<WorkerOtpGenerationValidator>();

        }


        public override async Task HandleAsync(WorkerOtpGenerationRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database on a CATALOG page
            var isWorkerExists = await _dbContext.Customers.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (isWorkerExists)
            {
                Logger.LogWarning("Canceled. User is already registered in a system");
                await Send.ErrorsAsync();
                return;
            }


            var code = _otpService.GenerateOtp(req.TelegramId);
            await PublishAsync(new CodeGeneratedEventObj
            {
                code = code,
                telegramId = req.TelegramId,
            }, Mode.WaitForNone); // we need to log this somehow and test it.

        

            await Send.OkAsync(new { Message = "Worker registered successfully." });







        }
    }

    public class WorkerOtpGenerationValidator : Validator<WorkerOtpGenerationRequest>
    {
        public WorkerOtpGenerationValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("UserId is required.");

        }
    }



    public sealed record WorkerOtpGenerationRequest
    {
        [FromHeader("Customer-Telegram-Id")]
        public long TelegramId { get; init; }

    }

    public sealed record CodeGeneratedEventObj
    {
        public string code { get; init; } = null!;
        public long telegramId { get; init; }

    }



}

