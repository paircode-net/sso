using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Roles.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.Roles.Specifications
{
	public class RoleCodeAlreadyExistsSpecification : DomainSpecification<Role>
	{
		private IIdentityDbContextReader Reader { get; set; }
		public RoleCodeAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A record with this code already exists!";
		}

		override public Expression<Func<Role, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(Role entity)
		{
			return Reader.Query<Role>().Any(x => !x.IsDeleted && x.Code == entity.Code && x.Id != entity.Id);
		}
	}
}
