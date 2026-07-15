using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.Products.Entity;

namespace SSO.Core.Domain.Identity.Products.Validations.EntityValidations
{
	public sealed class ProductValidator : EntityValidator<Product>
	{
		public ProductValidator()
		{
			RuleFor(x => x.Name).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Name).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Name).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Code).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Code).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Code).MaximumLength(64).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
		}
	}
}
