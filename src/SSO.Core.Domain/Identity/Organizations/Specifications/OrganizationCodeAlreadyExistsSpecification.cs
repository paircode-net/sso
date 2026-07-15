using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Organizations.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.Organizations.Specifications
{
	public class OrganizationCodeAlreadyExistsSpecification : DomainSpecification<Organization>
	{
		private IIdentityDbContextReader Reader { get; set; }
		public OrganizationCodeAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A record with this code already exists!";
		}

		override public Expression<Func<Organization, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(Organization entity)
		{
			return Reader.Query<Organization>().Any(x => !x.IsDeleted && x.Code == entity.Code && x.Id != entity.Id);
		}
	}
}
