using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Infrastructures.Data.Identity
{
	public sealed class IdentityDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<User, IdentityRole<Guid>, Guid>
	{
		public static string Schema => "IdentityDb";

		public IdentityDbContext()
		{
		}

		public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.HasDefaultSchema(Schema);
		}
	}
}
