using FastEndpoints;
using OrderService.Infrastructure.Entities.Administrator;
namespace OrderService.src.Admin.Feature.Profile.Create
{
    // alias
    using AdminGender = OrderService.Infrastructure.Entities.Administrator.Gender;

    public class ProfileMapper : RequestMapper<CreateProfileRequest, AdminProfile>
    {
        public override AdminProfile ToEntity(CreateProfileRequest r) => new()
        {
            AdminId = r.UserId,
            Name = r.Name,
            Surname = r.Surname,
            Age = r.Age,
            Gender = r.Gender switch
            {
                Gender.Male => AdminGender.Male,
                Gender.Female => AdminGender.Female,
                _ => null
            }
        };
    }
}
