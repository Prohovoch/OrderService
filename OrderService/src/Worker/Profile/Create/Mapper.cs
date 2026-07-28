using FastEndpoints;
using OrderService.Infrastructure.Entities.Employee;
namespace OrderService.src.Worker.Profile.Create
{
    // alias
    using WorkerGender = Infrastructure.Entities.Employee.Gender;

    public class ProfileMapper : RequestMapper<CreateProfileRequest, WorkerProfile>
    {
        public override WorkerProfile ToEntity(CreateProfileRequest r) => new()
        {
            WorkerId = r.UserId,
            Name = r.Name,
            Surname = r.Surname,
            Age = r.Age,
            Gender = r.Gender switch
            {
                Gender.Male => WorkerGender.Male,
                Gender.Female => WorkerGender.Female,
                _ => null
            }
        };
    }
}
