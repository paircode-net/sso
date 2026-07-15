using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.Permissions.Entity;

namespace SSO.Core.Domain.Identity.Permissions.Validations.EntityValidations
{
	public sealed class PermissionValidator : EntityValidator<Permission>
	{
		public PermissionValidator()
		{
			RuleFor(x => x.Code).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Code).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Code).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Name).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Name).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Name).MaximumLength(256).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
		}
	}
}
