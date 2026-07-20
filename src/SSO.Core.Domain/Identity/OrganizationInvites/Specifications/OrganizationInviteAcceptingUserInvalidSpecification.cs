using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using Microsoft.AspNetCore.Identity;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using System;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Specifications
{
	public sealed class OrganizationInviteAcceptingUserInvalidSpecification : DomainSpecification<OrganizationInvite>
	{
		private readonly UserManager<User> _userManager;

		public OrganizationInviteAcceptingUserInvalidSpecification(UserManager<User> userManager)
		{
			_userManager = userManager;
			SpecificationMessage = "Usuário inválido.";
		}

		public override Expression<Func<OrganizationInvite, bool>> ToExpression()
			=> invite => CheckRule(invite);

		private bool CheckRule(OrganizationInvite invite)
		{
			if (invite.AcceptedUserId is not Guid userId)
			{
				return true;
			}

			var user = _userManager.FindByIdAsync(userId.ToString()).GetAwaiter().GetResult();
			return user is null || user.IsDeleted;
		}
	}
}
