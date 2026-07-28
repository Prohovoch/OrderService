using FastEndpoints;
using OrderService.Infrastructure.Entities.Buyer;
namespace OrderService.src.Customer.Profile.Create
{
    // alias
    using BuyerGender = Infrastructure.Entities.Buyer.Gender;

    public class ProfileMapper : RequestMapper<CreateProfileRequest, CustomerProfile>
    {
        public override CustomerProfile ToEntity(CreateProfileRequest r) => new()
        {
            CustomerId = r.UserId,
            Name = r.Name,
            Surname = r.Surname,
            Age = r.Age,
            Gender = r.Gender switch
            {
                Gender.Male => BuyerGender.Male,
                Gender.Female => BuyerGender.Female,
                _ => null
            }
        };
    }
}
