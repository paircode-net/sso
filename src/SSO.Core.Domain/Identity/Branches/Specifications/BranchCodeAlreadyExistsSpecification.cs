using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.Branches.Specifications
{
	public class BranchCodeAlreadyExistsSpecification : DomainSpecification<Branch>
	{
		private IIdentityDbContextReader Reader { get; set; }
		public BranchCodeAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A branch with this code already exists in the organization!";
		}

		override public Expression<Func<Branch, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(Branch entity)
		{
			return Reader.Query<Branch>().Any(x =>
				!x.IsDeleted
				&& x.OrganizationId == entity.OrganizationId
				&& x.Code == entity.Code
				&& x.Id != entity.Id);
		}
	}
}
