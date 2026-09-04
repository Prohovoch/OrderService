using FastEndpoints;
using FluentValidation;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Entities.Buyer;
using Microsoft.EntityFrameworkCore;


namespace OrderService.src.Customer.Profile
{
    // REPR endpoint
    public class CreateProfile(ApplicationDbContext dbContext) : Endpoint<CreateCustomerProfileRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/customer/profile");
            AllowAnonymous();
            Validator<CreateProfileValidator>();
            
        }


        public override async Task HandleAsync(CreateCustomerProfileRequest req, CancellationToken ct)
        {

            var mainId = await _dbContext.Customers.Where(x => x.TgId == req.TelegramId).Select(x => x.Id).FirstOrDefaultAsync(ct);

            if (mainId == Guid.Empty)
            {
                AddError("Id", "Customer with this Id not found.");
                await Send.ErrorsAsync();
                return;
            }

            var customerProfile = new CustomerProfile
            {
                CustomerId = mainId,
                Name = req.Name,
                Surname = req.Surname,
                Age = req.Age,

                PhoneNumber = req.PhoneNumber,

                Gender = req.Gender switch
                {
                    CreateRequestGender.Male => BuyerGender.Male,
                    CreateRequestGender.Female => BuyerGender.Female,
                   
                    _ => BuyerGender.Unknown,
                },


            };
            _dbContext.Add(customerProfile);
            await _dbContext.SaveChangesAsync(ct);
            await Send.OkAsync();
        }

                     
        
    }
    public class CreateProfileValidator : Validator<CreateCustomerProfileRequest>
    {
        public CreateProfileValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required.");
            RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
                .NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.")
                .NotEmpty().WithMessage("Surname is required.");
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.");
            RuleFor(x => x.PhoneNumber).Matches(@"^(\+?7|8)\d{10}$").NotEmpty().WithMessage("Phone number from Russian Federation");
            RuleFor(x => x.Gender).IsInEnum();
        }
    }


   

    public enum CreateRequestGender { Male, Female }
    public sealed record CreateCustomerProfileRequest
    {
        
        public long TelegramId { get; init; }
        public required string Name { get; init; } 
        public required string Surname { get; init; } 
        public required string PhoneNumber { get; init; }
        public int Age { get; set; }
        public CreateRequestGender Gender { get; init; }
    }

}
