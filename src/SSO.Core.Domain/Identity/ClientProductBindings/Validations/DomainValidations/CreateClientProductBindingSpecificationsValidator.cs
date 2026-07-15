using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.ClientProductBindings.Specifications;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;

namespace SSO.Core.Domain.Identity.ClientProductBindings.Validations.DomainValidations
{
	public sealed class CreateClientProductBindingSpecificationsValidator : DomainValidator<ClientProductBinding>
	{
		public CreateClientProductBindingSpecificationsValidator(ClientProductBindingClientIdAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<ClientProductBinding>(spec.Not(), spec.ToString()));
		}
	}
}
