using Microsoft.AspNetCore.Identity;
using System;

namespace SSO.Core.Domain.Identity.Users.Entity
{
	public sealed class User : IdentityUser<Guid>
	{
		public User()
		{
			Id = Guid.NewGuid();
		}
	}
}
