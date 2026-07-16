using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Validations.EntityValidations
{
	public sealed class OrganizationInviteValidator : EntityValidator<OrganizationInvite>
	{
		public OrganizationInviteValidator()
		{
			RuleFor(x => x.OrganizationId).NotEmpty();
			RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
			RuleFor(x => x.TokenHash).NotEmpty().MaximumLength(128);
			RuleFor(x => x.Status).NotEmpty().MaximumLength(32);
			RuleFor(x => x.InvitedByUserId).NotEmpty();
			RuleFor(x => x.ExpiresAt).NotEmpty();
		}
	}
}
