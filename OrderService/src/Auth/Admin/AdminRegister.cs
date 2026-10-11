using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;

namespace OrderService.src.Auth.Admin
{
    public class AdminRegister (ApplicationDbContext dbContext) : Endpoint<AddAdminRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/admin/register");
            AllowAnonymous();
            Validator<AddAdminValidator>();
        }

        public override async Task HandleAsync(AddAdminRequest req, CancellationToken ct)
        {

            bool isAdminExists = await _dbContext.Admins.AnyAsync(x => x.TgId == req.TelegramId, ct);

            if (!isAdminExists)
            {
                await Send.ForbiddenAsync();
                return;
            }

           
            await Send.OkAsync();


        }
    }

    public class AddAdminValidator : Validator<AddAdminRequest>
    {
        public AddAdminValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required!");
        }
    }
    public sealed record AddAdminRequest
    {
        [FromHeader("Admin-Telegram-Id")]
        public long TelegramId { get; init; }
       
    }
}
 