using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using BAYSOFT.Core.Domain.Default.Samples.Entity;
using FluentValidation;

namespace BAYSOFT.Core.Domain.Default.Samples.Validations.EntityValidations
{
    public sealed class SampleValidator : EntityValidator<Sample>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Description).NotNull().WithMessage("'{PropertyName}' cannot be null!");
            RuleFor(x => x.Description).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
            // RuleFor(x => x.Description).MinimumLength(3).WithMessage("'{PropertyName}' must have at least '{MinLength}' caracters!");
            RuleFor(x => x.Description).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
        }
    }
}