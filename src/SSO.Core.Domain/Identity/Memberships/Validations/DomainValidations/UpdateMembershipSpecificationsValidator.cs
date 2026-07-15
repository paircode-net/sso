using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Memberships.Specifications;

namespace SSO.Core.Domain.Identity.Memberships.Validations.DomainValidations
{
	public sealed class UpdateMembershipSpecificationsValidator : DomainValidator<Membership>
	{
		public UpdateMembershipSpecificationsValidator(
			MembershipUserOrganizationAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<Membership>(spec.Not(), spec.ToString()));
		}
	}
}
