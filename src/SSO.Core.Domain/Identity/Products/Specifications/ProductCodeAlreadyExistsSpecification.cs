using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Products.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.Products.Specifications
{
	public class ProductCodeAlreadyExistsSpecification : DomainSpecification<Product>
	{
		private IIdentityDbContextReader Reader { get; set; }
		public ProductCodeAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A record with this code already exists!";
		}

		override public Expression<Func<Product, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(Product entity)
		{
			return Reader.Query<Product>().Any(x => !x.IsDeleted && x.Code == entity.Code && x.Id != entity.Id);
		}
	}
}
