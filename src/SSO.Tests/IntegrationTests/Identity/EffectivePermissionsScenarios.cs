using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class EffectivePermissionsScenarios
	{
		[TestMethod]
		public async Task Platform_Scoped_Assignment_Should_Resolve_Without_Organization()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectivePermissionsResolver>();

			var permissions = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				organizationId: null,
				branchId: null,
				IdentitySeed.AdminClientId);

			CollectionAssert.Contains(permissions.ToList(), SsoAdminPermissions.Platform);
			CollectionAssert.Contains(permissions.ToList(), SsoAdminPermissions.AuditRead);
			CollectionAssert.DoesNotContain(permissions.ToList(), IdentitySeed.PermissionAccess);
		}

		[TestMethod]
		public async Task Org_Context_Should_Return_Only_OrgWide_Permissions()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectivePermissionsResolver>();

			var permissions = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				branchId: null,
				SsoClients.DevSpaClientId);

			CollectionAssert.AreEquivalent(
				new[] { IdentitySeed.PermissionAccess },
				permissions.ToList());
		}

		[TestMethod]
		public async Task Hq_Context_Should_Include_Hq_And_OrgWide_But_Not_Filial()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectivePermissionsResolver>();

			var permissions = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchHqId,
				SsoClients.DevSpaClientId);

			CollectionAssert.AreEquivalent(
				new[] { IdentitySeed.PermissionAccess, IdentitySeed.PermissionHqReports },
				permissions.ToList());
			Assert.IsFalse(permissions.Contains(IdentitySeed.PermissionFilialOps));
		}

		[TestMethod]
		public async Task Filial_Context_Should_Not_Inherit_Hq_Permissions_From_Parent()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectivePermissionsResolver>();

			var permissions = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchFilialId,
				SsoClients.DevSpaClientId);

			CollectionAssert.AreEquivalent(
				new[] { IdentitySeed.PermissionAccess, IdentitySeed.PermissionFilialOps },
				permissions.ToList());
			Assert.IsFalse(
				permissions.Contains(IdentitySeed.PermissionHqReports),
				"HQ permissions must not inherit to child Branch (ADR-004).");
		}

		[TestMethod]
		public async Task Unknown_Client_Should_Yield_No_Permissions()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectivePermissionsResolver>();

			var permissions = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchHqId,
				clientId: "unknown-client");

			Assert.AreEqual(0, permissions.Count);
		}

		[TestMethod]
		public async Task TokenClaimsFactory_Should_Emit_Branch_Specific_Permissions()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();

			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var claimsFactory = scope.ServiceProvider.GetRequiredService<TokenClaimsFactory>();
			var user = await userManager.FindByIdAsync(IdentitySeed.DevUserId.ToString());
			Assert.IsNotNull(user);

			var principal = await claimsFactory.CreateUserPrincipalAsync(
				user!,
				new[] { OpenIddictConstants.Scopes.OpenId },
				SsoClients.DevSpaClientId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchHqId);

			Assert.AreEqual(IdentitySeed.DevBranchHqId.ToString("D"), principal.FindFirst(SsoClaimTypes.BranchId)?.Value);
			var permissions = principal.FindAll(SsoClaimTypes.Permissions).Select(c => c.Value).ToList();
			CollectionAssert.Contains(permissions, IdentitySeed.PermissionHqReports);
			CollectionAssert.DoesNotContain(permissions, IdentitySeed.PermissionFilialOps);
		}
	}
}
