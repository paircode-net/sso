using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity.EntityMappings;

namespace SSO.Infrastructures.Data.Identity
{
	public sealed class IdentityDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<User, IdentityRole<Guid>, Guid>
	{
		public static string Schema => "IdentityDb";

		public DbSet<Organization> Organizations { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Membership> Memberships { get; set; }

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

			builder.Entity<User>(entity =>
			{
				entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
				entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
				entity.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
				entity.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);
				entity.Ignore(e => e.Password);
			});

			builder.ApplyConfiguration(new OrganizationMap());
			builder.ApplyConfiguration(new ProductMap());
			builder.ApplyConfiguration(new MembershipMap());
		}
	}
}
