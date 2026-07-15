using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;

namespace SSO.Core.Domain.Identity.ClientProductBindings.Validations.EntityValidations
{
	public sealed class ClientProductBindingValidator : EntityValidator<ClientProductBinding>
	{
		public ClientProductBindingValidator()
		{
			RuleFor(x => x.ClientId).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.ClientId).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.ClientId).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.ProductId).NotEmpty().WithMessage("'{PropertyName}' is required!");
		}
	}
}
