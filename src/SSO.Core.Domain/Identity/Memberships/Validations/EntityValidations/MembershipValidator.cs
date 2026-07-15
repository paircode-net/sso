using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.Memberships.Entity;

namespace SSO.Core.Domain.Identity.Memberships.Validations.EntityValidations
{
	public sealed class MembershipValidator : EntityValidator<Membership>
	{
		public MembershipValidator()
		{
			RuleFor(x => x.UserId).NotEmpty().WithMessage("'{PropertyName}' is required!");
			RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("'{PropertyName}' is required!");
		}
	}
}
