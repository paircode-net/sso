using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.ClientProductBindings.Specifications
{
	public class ClientProductBindingClientIdAlreadyExistsSpecification : DomainSpecification<ClientProductBinding>
	{
		private IIdentityDbContextReader Reader { get; set; }
		public ClientProductBindingClientIdAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A record with this clientid already exists!";
		}

		override public Expression<Func<ClientProductBinding, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(ClientProductBinding entity)
		{
			return Reader.Query<ClientProductBinding>().Any(x => !x.IsDeleted && x.ClientId == entity.ClientId && x.Id != entity.Id);
		}
	}
}
