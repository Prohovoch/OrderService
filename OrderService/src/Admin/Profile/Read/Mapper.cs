using FastEndpoints;
using OrderService.Infrastructure.Entities.Administrator;

namespace OrderService.src.Admin.Profile.Read
{
    // alias
    using AdminGender = Infrastructure.Entities.Administrator.Gender;

    public class ProfileMapper : ResponseMapper<ReadProfileResponse, AdminProfile>
    {
        public override ReadProfileResponse FromEntity(AdminProfile e) => new()
        {
            Name = e.Name,
            Surname = e.Surname,
            Age = e.Age,
            Gender = e.Gender switch
            {
                AdminGender.Male => Gender.Male,
                AdminGender.Female => Gender.Female,
                _ => null
            }
        };

    }
}