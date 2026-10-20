using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Deal.Worker;
using OrderService.src.Security.Helpers;


namespace OrderService.src.Auth.Employee
{
    public class WorkerOtpGeneration(ApplicationDbContext dbContext, OtpGenerate otpService) : Endpoint<WorkerOtpGenerationRequest, WorkerOtpGenerationResponse>
    {
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
            var isWorkerExists = await dbContext.Customers.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (isWorkerExists)
            {
                Logger.LogWarning("Canceled. User is already registered in a system");
                await Send.ErrorsAsync();
                return;
            }


            var code = _otpService.GenerateOtp(req.TelegramId);
            var response = new WorkerOtpGenerationResponse
            {
                GeneratedCode = code
            };


            await Send.OkAsync(response);







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
        [FromHeader("Worker-Telegram-Id")]
        public long TelegramId { get; init; }

    }

    public sealed record WorkerOtpGenerationResponse
    {
        public required string GeneratedCode { get; init; }
    }



}

