using FastEndpoints;
using FluentValidation;

namespace OrderService.src.Admin.Feature.Profile.Create
{
    public class ValidatorClass : Validator<CreateProfileRequest>
    {
        public ValidatorClass() { 
            RuleFor(x => x.Name).MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
                .NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Surname).MinimumLength(3).WithMessage("Surname must be at least 3 characters long.")
                .NotEmpty().WithMessage("Surname is required.");
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Age must be between 18 and 120.");
        }
    }
}
