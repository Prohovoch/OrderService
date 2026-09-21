using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;




namespace OrderService.src.Admin
{
    public class DeleteWorker(ApplicationDbContext dbContext) : Endpoint<DeleteWorkerRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Delete("api/admin/workers/{workerId}");
            AllowAnonymous();
            Validator<DeleteWorkerValidator>();
        }

        public override async Task HandleAsync(DeleteWorkerRequest req, CancellationToken ct)
        {

            bool isAdminExists = await _dbContext.Admins.AnyAsync(x => x.TgId == req.TelegramId, ct);

            if (!isAdminExists)
            {
                await Send.ForbiddenAsync();
                return;
            }

            var affectedRows = await _dbContext.Workers
                .Where(x => x.Id == req.WorkerId)
                .ExecuteDeleteAsync(ct);
            if (affectedRows == 0)
            {
                await Send.NotFoundAsync(); // change status for something else idk.
                return;
            }
            await Send.NoContentAsync();


        }
    }

    public class DeleteWorkerValidator : Validator<DeleteWorkerRequest>
    {
        public DeleteWorkerValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required!");
            RuleFor(x => x.WorkerId).NotEmpty().WithMessage("WorkerId is required!");
        }
    }
    public sealed record DeleteWorkerRequest
    {
        [FromHeader("Admin-Telegram-Id")]
        public long TelegramId { get; init; }
        [BindFrom("workerId")]
        public Guid WorkerId { get; init; }

    }
}