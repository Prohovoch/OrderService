using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;
using System.Runtime.InteropServices;

namespace OrderService.src.Auth.Buyer
{
    public class RegisterCustomer(ApplicationDbContext dbContext) : Endpoint<RegisterCustomerRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/register/cutomer");
            AllowAnonymous();
            Validator<RegisterCustomerValidator>();

        }


        public override async Task HandleAsync(RegisterCustomerRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database on a CATALOG page
            var isCustomerExists = await _dbContext.Customers.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (isCustomerExists is true)
            {
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
            await Send.OkAsync(new { Message = "Customer registered successfully." });







        }
    }

    public class RegisterCustomerValidator : Validator<RegisterCustomerRequest>
    {
        public RegisterCustomerValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("UserId is required.");
            
        }
    }



    public sealed record RegisterCustomerRequest
    {
        [FromHeader("Customer-Telegram-Id")]
        public long TelegramId { get; init; }

    }





}

