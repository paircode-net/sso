using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Permissions.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.Permissions.Specifications
{
	public class PermissionCodeAlreadyExistsSpecification : DomainSpecification<Permission>
	{
		private IIdentityDbContextReader Reader { get; set; }
		public PermissionCodeAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A record with this code already exists!";
		}

		override public Expression<Func<Permission, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(Permission entity)
		{
			return Reader.Query<Permission>().Any(x => !x.IsDeleted && x.Code == entity.Code && x.Id != entity.Id);
		}
	}
}
