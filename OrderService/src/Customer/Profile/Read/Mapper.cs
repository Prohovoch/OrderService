using FastEndpoints;
using OrderService.Infrastructure.Entities.Buyer;

namespace OrderService.src.Customer.Profile.Read
{
    // alias
    using BuyerGender = Infrastructure.Entities.Buyer.Gender;

    public class ProfileMapper : ResponseMapper<ReadProfileResponse, CustomerProfile>
    {
        public override ReadProfileResponse FromEntity(CustomerProfile e) => new()
        {
            Name = e.Name,
            Surname = e.Surname,
            Age = e.Age,
            Gender = e.Gender switch
            {
               BuyerGender.Male => Gender.Male,
               BuyerGender.Female => Gender.Female,
               _ => null
            }
        };

    }
}