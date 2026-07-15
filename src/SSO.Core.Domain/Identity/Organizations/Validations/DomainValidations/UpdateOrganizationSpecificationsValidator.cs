using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Organizations.Specifications;
using SSO.Core.Domain.Identity.Organizations.Entity;

namespace SSO.Core.Domain.Identity.Organizations.Validations.DomainValidations
{
	public sealed class UpdateOrganizationSpecificationsValidator : DomainValidator<Organization>
	{
		public UpdateOrganizationSpecificationsValidator(OrganizationCodeAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<Organization>(spec.Not(), spec.ToString()));
		}
	}
}
