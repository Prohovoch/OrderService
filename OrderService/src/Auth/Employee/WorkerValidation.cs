using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Security.Helpers;

namespace OrderService.src.Auth.Employee
{
    public class WorkerValidation(ApplicationDbContext dbContext, OtpGenerate otpService) : Endpoint<WorkerOtpValidationRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly OtpGenerate _otpService = otpService;

        public override void Configure()
        {
            Post("api/worker/otp/validate");
            AllowAnonymous();
            Validator<WorkerOtpValidationValidator>();

        }


        public override async Task HandleAsync(WorkerOtpValidationRequest req, CancellationToken ct)
        {



            var isAlreadRegistered = await _dbContext.Workers.AnyAsync(w => w.TgId == req.TelegramId, ct);
            if (isAlreadRegistered)
            {
                AddError($"Worker with TelegramId { req.TelegramId} is already registered.");
                await Send.ErrorsAsync();
                return;
            }
            var isValid = _otpService.ValidateOtp(req.TelegramId, req.OtpCode);
            
            if (!isValid)
            {
                AddError($"Invalid OTP code for TelegramId {req.TelegramId}.");
                await Send.ErrorsAsync();
                return;
            }

            var worker = new OrderService.Infrastructure.Entities.Employee.Worker
            {
                TgId = req.TelegramId,
                Id = Guid.CreateVersion7(),

            };

            _dbContext.Workers.Add(worker);
            await _dbContext.SaveChangesAsync(ct);


            await Send.OkAsync(new { Message = "Worker registered successfully." });







        }
    }

    public class WorkerOtpValidationValidator : Validator<WorkerOtpValidationRequest>
    {
        public WorkerOtpValidationValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.OtpCode).NotEmpty().WithMessage("OtpCode is required.");
        }
    }



    public sealed record WorkerOtpValidationRequest
    {
        [FromHeader("Customer-Telegram-Id")]
        public long TelegramId { get; init; }
        public required string OtpCode
        {
            get; init;
        }

    }
}



 
