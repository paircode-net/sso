using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Users.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.Users.Specifications
{
	public class UserEmailAlreadyExistsSpecification : DomainSpecification<User>
	{
		private IIdentityDbContextReader Reader { get; set; }

		public UserEmailAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A user with this email already exists!";
		}

		public override Expression<Func<User, bool>> ToExpression()
			=> user => CheckRule(user);

		private bool CheckRule(User user)
		{
			var email = (user.Email ?? string.Empty).ToUpperInvariant();
			return Reader.Query<User>().Any(x =>
				!x.IsDeleted
				&& x.Email != null
				&& x.Email.ToUpper() == email
				&& x.Id != user.Id);
		}
	}
}
