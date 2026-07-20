using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using Microsoft.AspNetCore.Identity;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using System;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Specifications
{
	public sealed class OrganizationInviteAcceptingUserEmailMismatchSpecification : DomainSpecification<OrganizationInvite>
	{
		private readonly UserManager<User> _userManager;

		public OrganizationInviteAcceptingUserEmailMismatchSpecification(UserManager<User> userManager)
		{
			_userManager = userManager;
			SpecificationMessage = "O e-mail autenticado não corresponde ao convite.";
		}

		public override Expression<Func<OrganizationInvite, bool>> ToExpression()
			=> invite => CheckRule(invite);

		private bool CheckRule(OrganizationInvite invite)
		{
			if (invite.AcceptedUserId is not Guid userId)
			{
				return false;
			}

			var user = _userManager.FindByIdAsync(userId.ToString()).GetAwaiter().GetResult();
			if (user is null)
			{
				return false;
			}

			return !string.Equals(user.Email, invite.Email, StringComparison.OrdinalIgnoreCase);
		}
	}
}
