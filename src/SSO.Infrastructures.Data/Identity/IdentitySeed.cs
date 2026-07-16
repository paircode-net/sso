using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Infrastructures.Data.Identity
{
	public static class IdentitySeed
	{
		public static readonly Guid DevOrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
		public static readonly Guid DevProductId = Guid.Parse("22222222-2222-2222-2222-222222222222");
		public static readonly Guid DevPlatformProductId = Guid.Parse("22222222-2222-2222-2222-222222222221");
		public static readonly Guid DevUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
		public static readonly Guid DevMembershipId = Guid.Parse("44444444-4444-4444-4444-444444444444");
		public static readonly Guid DevBranchHqId = Guid.Parse("55555555-5555-5555-5555-555555555555");
		public static readonly Guid DevBranchFilialId = Guid.Parse("66666666-6666-6666-6666-666666666666");
		public static readonly Guid DevPermissionAccessId = Guid.Parse("71111111-1111-1111-1111-111111111111");
		public static readonly Guid DevPermissionHqReportsId = Guid.Parse("72222222-2222-2222-2222-222222222222");
		public static readonly Guid DevPermissionFilialOpsId = Guid.Parse("73333333-3333-3333-3333-333333333333");
		public static readonly Guid DevPermissionAdminPlatformId = Guid.Parse("74111111-1111-1111-1111-111111111111");
		public static readonly Guid DevPermissionAdminOrgId = Guid.Parse("74222222-2222-2222-2222-222222222222");
		public static readonly Guid DevPermissionAdminAuditId = Guid.Parse("74333333-3333-3333-3333-333333333333");
		public static readonly Guid DevPermissionAdminSessionsId = Guid.Parse("74444444-4444-4444-4444-444444444444");
		public static readonly Guid DevPermissionAdminMenusId = Guid.Parse("74555555-5555-5555-5555-555555555555");
		public static readonly Guid DevRoleOrgMemberId = Guid.Parse("81111111-1111-1111-1111-111111111111");
		public static readonly Guid DevRoleHqManagerId = Guid.Parse("82222222-2222-2222-2222-222222222222");
		public static readonly Guid DevRoleFilialStaffId = Guid.Parse("83333333-3333-3333-3333-333333333333");
		public static readonly Guid DevRolePlatformAdminId = Guid.Parse("84444444-4444-4444-4444-444444444444");
		public static readonly Guid DevRoleOrgAdminId = Guid.Parse("85555555-5555-5555-5555-555555555555");
		public static readonly Guid DevMenuHomeId = Guid.Parse("91111111-1111-1111-1111-111111111111");
		public static readonly Guid DevMenuHqReportsId = Guid.Parse("92222222-2222-2222-2222-222222222222");
		public static readonly Guid DevMenuFilialOpsId = Guid.Parse("93333333-3333-3333-3333-333333333333");
		public static readonly Guid DevEntraIdpId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
		public static readonly Guid DevGoogleIdpId = Guid.Parse("a2222222-2222-2222-2222-222222222222");
		public static readonly Guid DevLdapIdpId = Guid.Parse("a3333333-3333-3333-3333-333333333333");

		public const string DevUserEmail = "admin@sso.local";
		public const string DevUserPassword = "ChangeMe!123";
		public const string PermissionAccess = "sso.access";
		public const string PermissionHqReports = "hq.reports";
		public const string PermissionFilialOps = "filial.ops";
		public const string AdminClientId = SsoClients.AdminApiClientId;
		public const string AdminClientSecret = SsoClients.AdminApiClientSecret;

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

			if (!await context.Products.AnyAsync(x => x.Id == DevPlatformProductId))
			{
				var platformProduct = new Product
				{
					Id = DevPlatformProductId,
					Name = "SSO Platform",
					Code = "sso-platform"
				};
				platformProduct.MarkCreated();
				context.Products.Add(platformProduct);
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

			await EnsureBranchesAsync(context);
			await EnsureAuthzCatalogAsync(context);
			await EnsureOpenIddictClientsAsync(services);
		}

		private static async Task EnsureBranchesAsync(IdentityDbContext context)
		{
			if (!await context.Branches.AnyAsync(x => x.Id == DevBranchHqId))
			{
				var hq = new Branch
				{
					Id = DevBranchHqId,
					OrganizationId = DevOrganizationId,
					ParentBranchId = null,
					Name = "HQ",
					Code = "hq"
				};
				hq.MarkCreated();
				context.Branches.Add(hq);
			}

			if (!await context.Branches.AnyAsync(x => x.Id == DevBranchFilialId))
			{
				var filial = new Branch
				{
					Id = DevBranchFilialId,
					OrganizationId = DevOrganizationId,
					ParentBranchId = DevBranchHqId,
					Name = "Filial",
					Code = "filial"
				};
				filial.MarkCreated();
				context.Branches.Add(filial);
			}

			await context.SaveChangesAsync();
		}

		private static async Task EnsureAuthzCatalogAsync(IdentityDbContext context)
		{
			await EnsurePermissionAsync(context, DevPermissionAccessId, PermissionAccess, "SSO Access");
			await EnsurePermissionAsync(context, DevPermissionHqReportsId, PermissionHqReports, "HQ Reports");
			await EnsurePermissionAsync(context, DevPermissionFilialOpsId, PermissionFilialOps, "Filial Operations");
			await EnsurePermissionAsync(context, DevPermissionAdminPlatformId, SsoAdminPermissions.Platform, "SSO Platform Admin");
			await EnsurePermissionAsync(context, DevPermissionAdminOrgId, SsoAdminPermissions.Org, "SSO Org Admin");
			await EnsurePermissionAsync(context, DevPermissionAdminAuditId, SsoAdminPermissions.AuditRead, "SSO Audit Read");
			await EnsurePermissionAsync(context, DevPermissionAdminSessionsId, SsoAdminPermissions.SessionsRevoke, "SSO Sessions Revoke");
			await EnsurePermissionAsync(context, DevPermissionAdminMenusId, SsoAdminPermissions.Menus, "SSO Menus Admin");

			await EnsureRoleAsync(context, DevRoleOrgMemberId, "org-member", "Organization Member");
			await EnsureRoleAsync(context, DevRoleHqManagerId, "hq-manager", "HQ Manager");
			await EnsureRoleAsync(context, DevRoleFilialStaffId, "filial-staff", "Filial Staff");
			await EnsureRoleAsync(context, DevRolePlatformAdminId, "platform-admin", "Platform Admin");
			await EnsureRoleAsync(context, DevRoleOrgAdminId, "org-admin", "Organization Admin");

			await EnsureRolePermissionAsync(context, DevRoleOrgMemberId, DevPermissionAccessId);
			await EnsureRolePermissionAsync(context, DevRoleHqManagerId, DevPermissionHqReportsId);
			await EnsureRolePermissionAsync(context, DevRoleFilialStaffId, DevPermissionFilialOpsId);
			await EnsureRolePermissionAsync(context, DevRolePlatformAdminId, DevPermissionAdminPlatformId);
			await EnsureRolePermissionAsync(context, DevRolePlatformAdminId, DevPermissionAdminOrgId);
			await EnsureRolePermissionAsync(context, DevRolePlatformAdminId, DevPermissionAdminAuditId);
			await EnsureRolePermissionAsync(context, DevRolePlatformAdminId, DevPermissionAdminSessionsId);
			await EnsureRolePermissionAsync(context, DevRolePlatformAdminId, DevPermissionAdminMenusId);
			await EnsureRolePermissionAsync(context, DevRoleOrgAdminId, DevPermissionAdminOrgId);
			await EnsureRolePermissionAsync(context, DevRoleOrgAdminId, DevPermissionAdminSessionsId);

			await EnsureAssignmentAsync(context, DevUserId, DevRoleOrgMemberId, DevOrganizationId, null, DevProductId);
			await EnsureAssignmentAsync(context, DevUserId, DevRoleHqManagerId, DevOrganizationId, DevBranchHqId, DevProductId);
			await EnsureAssignmentAsync(context, DevUserId, DevRoleFilialStaffId, DevOrganizationId, DevBranchFilialId, DevProductId);
			await EnsureAssignmentAsync(context, DevUserId, DevRolePlatformAdminId, organizationId: null, branchId: null, DevPlatformProductId);
			await EnsureAssignmentAsync(context, DevUserId, DevRoleOrgAdminId, DevOrganizationId, null, DevPlatformProductId);

			await EnsureClientBindingAsync(context, SsoClients.DevSpaClientId, DevProductId);
			await EnsureClientBindingAsync(context, SsoClients.DevServiceClientId, DevProductId);
			await EnsureClientBindingAsync(context, AdminClientId, DevPlatformProductId);

			await EnsureMenuAsync(context, DevMenuHomeId, DevProductId, "home", "Home", "/app", PermissionAccess, 10);
			await EnsureMenuAsync(context, DevMenuHqReportsId, DevProductId, "hq-reports", "HQ Reports", "/app/hq/reports", PermissionHqReports, 20);
			await EnsureMenuAsync(context, DevMenuFilialOpsId, DevProductId, "filial-ops", "Filial Ops", "/app/filial/ops", PermissionFilialOps, 30);

			await EnsureExternalIdpAsync(
				context,
				DevEntraIdpId,
				ExternalIdpTypes.Entra,
				"entra-homolog",
				"Microsoft Entra ID",
				isEnabled: true,
				allowJit: false,
				authority: "https://login.microsoftonline.com/common/v2.0",
				clientId: null);
			await EnsureExternalIdpAsync(
				context,
				DevGoogleIdpId,
				ExternalIdpTypes.Google,
				"google",
				"Google",
				isEnabled: false,
				allowJit: false,
				authority: "https://accounts.google.com",
				clientId: null);
			await EnsureExternalIdpAsync(
				context,
				DevLdapIdpId,
				ExternalIdpTypes.Ldap,
				"ldap",
				"LDAP / Active Directory",
				isEnabled: false,
				allowJit: false,
				authority: null,
				clientId: null);

			await context.SaveChangesAsync();
		}

		private static async Task EnsurePermissionAsync(IdentityDbContext context, Guid id, string code, string name)
		{
			if (await context.Permissions.AnyAsync(x => x.Id == id))
			{
				return;
			}

			var permission = new Permission { Id = id, Code = code, Name = name };
			permission.MarkCreated();
			context.Permissions.Add(permission);
		}

		private static async Task EnsureRoleAsync(IdentityDbContext context, Guid id, string code, string name)
		{
			if (await context.AuthRoles.AnyAsync(x => x.Id == id))
			{
				return;
			}

			var role = new Role { Id = id, Code = code, Name = name };
			role.MarkCreated();
			context.AuthRoles.Add(role);
		}

		private static async Task EnsureRolePermissionAsync(IdentityDbContext context, Guid roleId, Guid permissionId)
		{
			if (await context.RolePermissions.AnyAsync(x =>
				!x.IsDeleted && x.RoleId == roleId && x.PermissionId == permissionId))
			{
				return;
			}

			var link = new RolePermission { RoleId = roleId, PermissionId = permissionId };
			link.MarkCreated();
			context.RolePermissions.Add(link);
		}

		private static async Task EnsureAssignmentAsync(
			IdentityDbContext context,
			Guid userId,
			Guid roleId,
			Guid? organizationId,
			Guid? branchId,
			Guid productId)
		{
			if (await context.UserRoleAssignments.AnyAsync(x =>
				!x.IsDeleted
				&& x.UserId == userId
				&& x.RoleId == roleId
				&& x.OrganizationId == organizationId
				&& x.BranchId == branchId
				&& x.ProductId == productId))
			{
				return;
			}

			var assignment = new UserRoleAssignment
			{
				UserId = userId,
				RoleId = roleId,
				OrganizationId = organizationId,
				BranchId = branchId,
				ProductId = productId
			};
			assignment.MarkCreated();
			context.UserRoleAssignments.Add(assignment);
		}

		private static async Task EnsureClientBindingAsync(IdentityDbContext context, string clientId, Guid productId)
		{
			if (await context.ClientProductBindings.AnyAsync(x => !x.IsDeleted && x.ClientId == clientId))
			{
				return;
			}

			var binding = new ClientProductBinding { ClientId = clientId, ProductId = productId };
			binding.MarkCreated();
			context.ClientProductBindings.Add(binding);
		}

		private static async Task EnsureMenuAsync(
			IdentityDbContext context,
			Guid id,
			Guid productId,
			string code,
			string title,
			string route,
			string permissionCode,
			int sortOrder)
		{
			if (await context.MenuItems.AnyAsync(x => x.Id == id))
			{
				return;
			}

			var menu = new MenuItem
			{
				Id = id,
				ProductId = productId,
				Code = code,
				Title = title,
				Route = route,
				PermissionCode = permissionCode,
				SortOrder = sortOrder
			};
			menu.MarkCreated();
			context.MenuItems.Add(menu);
		}

		private static async Task EnsureExternalIdpAsync(
			IdentityDbContext context,
			Guid id,
			string providerType,
			string code,
			string displayName,
			bool isEnabled,
			bool allowJit,
			string authority,
			string clientId)
		{
			if (await context.ExternalIdentityProviders.AnyAsync(x => x.Id == id))
			{
				return;
			}

			var idp = new ExternalIdentityProvider
			{
				Id = id,
				ProviderType = providerType,
				Code = code,
				DisplayName = displayName,
				IsEnabled = isEnabled,
				AllowJitProvisioning = allowJit,
				Authority = authority,
				ClientId = clientId
			};
			idp.MarkCreated();
			context.ExternalIdentityProviders.Add(idp);
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

			if (await applicationManager.FindByClientIdAsync(AdminClientId) is null)
			{
				await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
				{
					ClientId = AdminClientId,
					ClientSecret = AdminClientSecret,
					DisplayName = "SSO Admin API",
					ClientType = ClientTypes.Confidential,
					ConsentType = ConsentTypes.Implicit,
					Permissions =
					{
						Permissions.Endpoints.Authorization,
						Permissions.Endpoints.Token,
						Permissions.Endpoints.Revocation,
						Permissions.GrantTypes.AuthorizationCode,
						Permissions.GrantTypes.RefreshToken,
						Permissions.GrantTypes.ClientCredentials,
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
		}
	}
}
