using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Memberships.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.Memberships.Specifications
{
	public class MembershipUserOrganizationAlreadyExistsSpecification : DomainSpecification<Membership>
	{
		private IIdentityDbContextReader Reader { get; set; }

		public MembershipUserOrganizationAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A membership for this user and organization already exists!";
		}

		public override Expression<Func<Membership, bool>> ToExpression()
			=> membership => CheckRule(membership);

		private bool CheckRule(Membership membership)
		{
			return Reader.Query<Membership>().Any(x =>
				!x.IsDeleted
				&& x.UserId == membership.UserId
				&& x.OrganizationId == membership.OrganizationId
				&& x.Id != membership.Id);
		}
	}
}
