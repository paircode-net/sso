using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.MenuItems.Entity;

namespace SSO.Core.Domain.Identity.MenuItems.Validations.EntityValidations
{
	public sealed class MenuItemValidator : EntityValidator<MenuItem>
	{
		public MenuItemValidator()
		{
			RuleFor(x => x.ProductId).NotEmpty().WithMessage("'{PropertyName}' is required!");
			RuleFor(x => x.Code).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Code).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Code).MaximumLength(64).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Title).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Title).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Title).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Route).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Route).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Route).MaximumLength(256).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.PermissionCode).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.PermissionCode).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.PermissionCode).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
		}
	}
}
