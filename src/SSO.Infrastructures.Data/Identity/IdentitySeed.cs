using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

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

			await EnsureOpenIddictClientsAsync(services);
		}

		private static async Task EnsureOpenIddictClientsAsync(IServiceProvider services)
		{
			var scopeManager = services.GetRequiredService<IOpenIddictScopeManager>();
			var applicationManager = services.GetRequiredService<IOpenIddictApplicationManager>();

			if (await scopeManager.FindByNameAsync(Scopes.OpenId) is null)
			{
				await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
				{
					Name = Scopes.OpenId,
					DisplayName = "OpenID"
				});
			}

			if (await scopeManager.FindByNameAsync(Scopes.OfflineAccess) is null)
			{
				await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
				{
					Name = Scopes.OfflineAccess,
					DisplayName = "Offline access"
				});
			}

			if (await applicationManager.FindByClientIdAsync(SsoClients.DevSpaClientId) is null)
			{
				await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
				{
					ClientId = SsoClients.DevSpaClientId,
					DisplayName = "Dev Product SPA",
					ClientType = ClientTypes.Public,
					ConsentType = ConsentTypes.Implicit,
					RedirectUris =
					{
						new Uri("https://localhost/callback"),
						new Uri("https://localhost:5001/callback")
					},
					PostLogoutRedirectUris =
					{
						new Uri("https://localhost/"),
						new Uri("https://localhost:5001/")
					},
					Permissions =
					{
						Permissions.Endpoints.Authorization,
						Permissions.Endpoints.Token,
						Permissions.Endpoints.EndSession,
						Permissions.Endpoints.Revocation,
						Permissions.GrantTypes.AuthorizationCode,
						Permissions.GrantTypes.RefreshToken,
						Permissions.Prefixes.GrantType + SsoGrantTypes.SwitchContext,
						Permissions.ResponseTypes.Code,
						Permissions.Scopes.Email,
						Permissions.Scopes.Profile,
						Permissions.Scopes.Roles,
						Permissions.Prefixes.Scope + Scopes.OfflineAccess
					},
					Requirements =
					{
						Requirements.Features.ProofKeyForCodeExchange
					}
				});
			}

			if (await applicationManager.FindByClientIdAsync(SsoClients.DevServiceClientId) is null)
			{
				await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
				{
					ClientId = SsoClients.DevServiceClientId,
					ClientSecret = SsoClients.DevServiceClientSecret,
					DisplayName = "Dev Product Service",
					ClientType = ClientTypes.Confidential,
					Permissions =
					{
						Permissions.Endpoints.Token,
						Permissions.Endpoints.Revocation,
						Permissions.GrantTypes.ClientCredentials,
						Permissions.Prefixes.Scope + Scopes.OpenId
					}
				});
			}
		}
	}
}
