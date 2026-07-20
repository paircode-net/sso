using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using System;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Specifications
{
	public sealed class OrganizationInviteIsExpiredSpecification : DomainSpecification<OrganizationInvite>
	{
		public OrganizationInviteIsExpiredSpecification()
		{
			SpecificationMessage = "Convite expirado ou já utilizado.";
		}

		public override Expression<Func<OrganizationInvite, bool>> ToExpression()
			=> invite => invite.ExpiresAt <= DateTime.UtcNow;
	}
}
