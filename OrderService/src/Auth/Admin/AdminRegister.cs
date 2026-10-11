using FastEndpoints;
using FluentValidation;
using OrderService.Infrastructure.Persistence;
using DotNetEnv;
using OrderService.Infrastructure.Entities.Administrator;

// hack
using Supervisor = OrderService.Infrastructure.Entities.Administrator.Admin;
using Microsoft.EntityFrameworkCore;
namespace OrderService.src.Auth.Admin
{
    public class AdminRegister (ApplicationDbContext dbContext) : Endpoint<AddAdminRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/admin/init");
            AllowAnonymous();
            Validator<AddAdminValidator>();
        }

        public override async Task HandleAsync(AddAdminRequest req, CancellationToken ct)
        {
            bool isAdminExists = await _dbContext.Admins.AnyAsync(w => w.TgId == req.TelegramId);
            if (isAdminExists)
            {
                AddError("Admin already exists.");
                await Send.ErrorsAsync();
                return;
            }
            var adminTgId = DotNetEnv.Env.GetString("TGADMIN_ID");
            if (!long.TryParse(adminTgId, out long tgAdminId) || tgAdminId <= 0)
            {
                AddError("Invalid TGADMIN_ID in environment variables.");
                await Send.ErrorsAsync();
                return;
            }
           
            if (req.TelegramId != tgAdminId)
            {
                await Send.ForbiddenAsync();
                return;
            }
            var admin = new Supervisor
            {
                Id = Guid.CreateVersion7(),
                TgId = req.TelegramId,
            };
            _dbContext.Admins.Add(admin);
            await _dbContext.SaveChangesAsync(ct);
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
 