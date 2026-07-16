using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Validations.DomainValidations
{
	public sealed class CreateOrganizationInviteSpecificationsValidator : DomainValidator<OrganizationInvite>
	{
		public CreateOrganizationInviteSpecificationsValidator()
		{
		}
	}

	public sealed class CancelOrganizationInviteSpecificationsValidator : DomainValidator<OrganizationInvite>
	{
		public CancelOrganizationInviteSpecificationsValidator()
		{
		}
	}
}
