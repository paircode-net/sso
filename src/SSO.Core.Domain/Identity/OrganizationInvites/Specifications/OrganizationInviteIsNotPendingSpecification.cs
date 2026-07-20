using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using System;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Specifications
{
	public sealed class OrganizationInviteIsNotPendingSpecification : DomainSpecification<OrganizationInvite>
	{
		public OrganizationInviteIsNotPendingSpecification()
		{
			SpecificationMessage = "Convite já respondido.";
		}

		public override Expression<Func<OrganizationInvite, bool>> ToExpression()
			=> invite => invite.Status != OrganizationInviteStatuses.Pending;
	}
}
