using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Infrastructures.Data.Identity
{
	public static class IdentitySeed
	{
		public static readonly Guid DevOrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
		public static readonly Guid DevProductId = Guid.Parse("22222222-2222-2222-2222-222222222222");
		public static readonly Guid DevUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
		public static readonly Guid DevMembershipId = Guid.Parse("44444444-4444-4444-4444-444444444444");

		public const string DevUserEmail = "admin@sso.local";
		public const string DevUserPassword = "ChangeMe!123";

		public static async Task EnsureAsync(IServiceProvider services)
		{
			var context = services.GetRequiredService<IdentityDbContext>();
			if (!context.Database.IsRelational())
			{
				return;
			}

			if (!await context.Organizations.AnyAsync(x => x.Id == DevOrganizationId))
			{
				var organization = new Organization
				{
					Id = DevOrganizationId,
					Name = "Dev Organization",
					Code = "dev-org"
				};
				organization.MarkCreated();
				context.Organizations.Add(organization);
			}

			if (!await context.Products.AnyAsync(x => x.Id == DevProductId))
			{
				var product = new Product
				{
					Id = DevProductId,
					Name = "Dev Product",
					Code = "dev-product"
				};
				product.MarkCreated();
				context.Products.Add(product);
			}

			await context.SaveChangesAsync();

			var userManager = services.GetRequiredService<UserManager<User>>();
			var existingUser = await userManager.FindByIdAsync(DevUserId.ToString());
			if (existingUser == null)
			{
				var user = new User
				{
					Id = DevUserId,
					Email = DevUserEmail,
					UserName = DevUserEmail,
					EmailConfirmed = true
				};
				user.MarkCreated();

				var result = await userManager.CreateAsync(user, DevUserPassword);
				if (!result.Succeeded)
				{
					throw new InvalidOperationException(
						"Failed to seed identity user: " + string.Join("; ", result.Errors));
				}
			}

			if (!await context.Memberships.AnyAsync(x => x.Id == DevMembershipId))
			{
				var membership = new Membership
				{
					Id = DevMembershipId,
					UserId = DevUserId,
					OrganizationId = DevOrganizationId
				};
				membership.MarkCreated();
				context.Memberships.Add(membership);
				await context.SaveChangesAsync();
			}
		}
	}
}
