using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Specifications;

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
		public CancelOrganizationInviteSpecificationsValidator(OrganizationInviteIsNotPendingSpecification notPending)
		{
			Add(nameof(notPending), new DomainRule<OrganizationInvite>(notPending.Not(), "Only pending invites can be cancelled."));
		}
	}

	public sealed class ResendOrganizationInviteSpecificationsValidator : DomainValidator<OrganizationInvite>
	{
		public ResendOrganizationInviteSpecificationsValidator(OrganizationInviteIsNotPendingSpecification notPending)
		{
			Add(nameof(notPending), new DomainRule<OrganizationInvite>(notPending.Not(), "Only pending invites can be resent."));
		}
	}

	public sealed class AcceptOrganizationInviteSpecificationsValidator : DomainValidator<OrganizationInvite>
	{
		public AcceptOrganizationInviteSpecificationsValidator(
			OrganizationInviteIsNotPendingSpecification notPending,
			OrganizationInviteIsExpiredSpecification expired,
			OrganizationInviteAcceptingUserInvalidSpecification userInvalid,
			OrganizationInviteAcceptingUserEmailMismatchSpecification emailMismatch)
		{
			Add(nameof(notPending), new DomainRule<OrganizationInvite>(notPending.Not(), "Convite expirado ou já utilizado."));
			Add(nameof(expired), new DomainRule<OrganizationInvite>(expired.Not(), expired.ToString()));
			Add(nameof(userInvalid), new DomainRule<OrganizationInvite>(userInvalid.Not(), userInvalid.ToString()));
			Add(nameof(emailMismatch), new DomainRule<OrganizationInvite>(emailMismatch.Not(), emailMismatch.ToString()));
		}
	}

	public sealed class DeclineOrganizationInviteSpecificationsValidator : DomainValidator<OrganizationInvite>
	{
		public DeclineOrganizationInviteSpecificationsValidator(
			OrganizationInviteIsNotPendingSpecification notPending,
			OrganizationInviteAcceptingUserEmailMismatchSpecification emailMismatch)
		{
			Add(nameof(notPending), new DomainRule<OrganizationInvite>(notPending.Not(), notPending.ToString()));
			Add(nameof(emailMismatch), new DomainRule<OrganizationInvite>(emailMismatch.Not(), emailMismatch.ToString()));
		}
	}
}
