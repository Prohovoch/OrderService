using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Administrator;
using OrderService.Infrastructure.Persistence;


namespace OrderService.src.Admin.Profile


{
    // REPR endpoint
    public class ReadProfile(ApplicationDbContext dbContext) : Endpoint<ReadProfileRequest, ReadProfileResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/admin/profile/me");
            AllowAnonymous();
            Validator<ReadProfileValidator>();

        }
        public override async Task HandleAsync(ReadProfileRequest req, CancellationToken ct)
        {
            var entityId = await _dbContext.Admins.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => x.Id).FirstAsync(ct);
            var adminProfileEntity = await _dbContext.AdminProfiles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.AdminId == entityId, ct);

            if (adminProfileEntity is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            var response = new ReadProfileResponse
            {
                Name = adminProfileEntity.Name,
                Surname = adminProfileEntity.Surname,
                Age = adminProfileEntity.Age,
                Gender = adminProfileEntity.Gender switch
                {
                    AdminGender.Male => ReadReqGender.Male,
                    AdminGender.Female => ReadReqGender.Female,
                    _ => null
                }
            };

            await Send.OkAsync(response);
        }
    }

   
    public class ReadProfileValidator : Validator<ReadProfileRequest>
    {
    public ReadProfileValidator()
    {
        RuleFor(x => x.TelegramId).NotNull().WithMessage("UserId is required.");
    }
}
    public sealed record ReadProfileRequest
    {
        
         public long TelegramId{ get; init; }
    
    }
    public enum ReadReqGender { Male, Female }
    public sealed record ReadProfileResponse
    {
         public required string Name { get; init; } 
         public required string Surname { get; init; }
         public int Age { get; init; }
         public ReadReqGender? Gender { get; init; }
    }
}