using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.Branches.Specifications
{
	public sealed class BranchParentWouldCreateCycleSpecification : DomainSpecification<Branch>
	{
		private IIdentityDbContextReader Reader { get; set; }

		public BranchParentWouldCreateCycleSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "ParentBranchId would create a cycle in the branch tree!";
		}

		override public Expression<Func<Branch, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(Branch entity)
		{
			if (entity.ParentBranchId is null)
			{
				return false;
			}

			var siblings = Reader.Query<Branch>()
				.Where(x => !x.IsDeleted && x.OrganizationId == entity.OrganizationId)
				.ToList();

			// Include the entity under update with its proposed ParentBranchId.
			var index = siblings.FindIndex(x => x.Id == entity.Id);
			if (index >= 0)
			{
				siblings[index] = entity;
			}
			else
			{
				siblings.Add(entity);
			}

			return global::SSO.Core.Domain.Identity.Branches.BranchAncestry.WouldCreateCycle(siblings, entity);
		}
	}
}
