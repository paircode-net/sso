using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Organizations.Specifications;
using SSO.Core.Domain.Identity.Organizations.Entity;

namespace SSO.Core.Domain.Identity.Organizations.Validations.DomainValidations
{
	public sealed class CreateOrganizationSpecificationsValidator : DomainValidator<Organization>
	{
		public CreateOrganizationSpecificationsValidator(OrganizationCodeAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<Organization>(spec.Not(), spec.ToString()));
		}
	}
}
