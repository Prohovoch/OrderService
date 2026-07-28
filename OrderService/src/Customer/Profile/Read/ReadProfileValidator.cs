using FastEndpoints;
using FluentValidation;

namespace OrderService.src.Customer.Profile.Read
{
    public class ReadProfileValidator : Validator<ReadProfileRequest>
    {
        public ReadProfileValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required.");
        }
    }
}
