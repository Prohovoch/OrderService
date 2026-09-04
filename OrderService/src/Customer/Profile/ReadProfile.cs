using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Persistence;


namespace OrderService.src.Customer.Profile
{
    // REPR endpoint
    public class ReadProfile(ApplicationDbContext dbContext) : Endpoint<ReadCustomerProfileRequest, ReadCustomerProfileResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/customer/profile/{TelegramId}");
            AllowAnonymous();

            Validator<ReadProfileValidator>();
            
        }


        public override async Task HandleAsync(ReadCustomerProfileRequest req, CancellationToken ct)
        {
            var mainId = await _dbContext.Customers.AsNoTracking()
                .Where(r => r.TgId == req.TelegramId)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(ct);
            if (mainId == Guid.Empty)
            {
                AddError("Id","Customer with this TelegramId not found.");
                await Send.ErrorsAsync();
                return;
            }

            var customerProfileEntity = await _dbContext.CustomerProfiles.AsNoTracking()
                .Where(r => r.CustomerId == mainId).FirstOrDefaultAsync(ct);
                
            if (customerProfileEntity is null)
            {
                await Send.NotFoundAsync();
                return;
            }



            var response = new ReadCustomerProfileResponse
            {
                Name = customerProfileEntity.Name,
                Surname = customerProfileEntity.Surname,
                PhoneNumber = customerProfileEntity.PhoneNumber,
                Age = customerProfileEntity.Age,

                // Маппинг Enum (если в БД null, то в ДТО запишется null, иначе проверит на Male)
                Gender = customerProfileEntity.Gender switch
                {
                    BuyerGender.Male => GetRequestGender.Male,
                    BuyerGender.Female => GetRequestGender.Female,
                    _ => GetRequestGender.Unknown,
                }

            };
            await Send.OkAsync(response);
            
        }
    }

    public class ReadProfileValidator : Validator<ReadCustomerProfileRequest>
    {
        public ReadProfileValidator()
        {
            RuleFor(x => x.TelegramId).NotNull().WithMessage("UserId is required.");
        }
    }
   
    

    public sealed record ReadCustomerProfileRequest
    {
        public long TelegramId { get; init; }
        
    }

    public enum GetRequestGender { Male, Female, Unknown }
    public sealed record ReadCustomerProfileResponse
    {
        public required string Name { get; init; }
        public required string Surname { get; init; }
        public required string PhoneNumber { get; init; }
        public int Age { get; init; }
        public GetRequestGender Gender { get; init; }
    }

}
