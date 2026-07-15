using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class DynamicPermissionsScenarios
	{
		[TestMethod]
		public async Task New_Permission_Linked_To_Role_Appears_In_Jwt_Claims_After_Reissue()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();

			var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var claimsFactory = scope.ServiceProvider.GetRequiredService<TokenClaimsFactory>();
			var versionBefore = await scope.ServiceProvider
				.GetRequiredService<IPermissionPolicyVersionProvider>()
				.GetVersionAsync();

			var user = await userManager.FindByIdAsync(IdentitySeed.DevUserId.ToString());
			Assert.IsNotNull(user);

			var before = await claimsFactory.CreateUserPrincipalAsync(
				user!,
				new[] { OpenIddictConstants.Scopes.OpenId },
				SsoClients.DevSpaClientId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchHqId);

			Assert.IsFalse(before.FindAll(SsoClaimTypes.Permissions).Any(c => c.Value == "dynamic.feature"));

			var permission = new Permission
			{
				Code = "dynamic.feature",
				Name = "Dynamic Feature"
			};
			permission.MarkCreated();
			context.Permissions.Add(permission);
			await context.SaveChangesAsync();

			var link = new RolePermission
			{
				RoleId = IdentitySeed.DevRoleHqManagerId,
				PermissionId = permission.Id
			};
			link.MarkCreated();
			context.RolePermissions.Add(link);
			await context.SaveChangesAsync();

			var after = await claimsFactory.CreateUserPrincipalAsync(
				user!,
				new[] { OpenIddictConstants.Scopes.OpenId },
				SsoClients.DevSpaClientId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchHqId);

			Assert.IsTrue(
				after.FindAll(SsoClaimTypes.Permissions).Any(c => c.Value == "dynamic.feature"),
				"New permission code must appear on next token issuance without client redeploy.");

			var versionAfter = after.FindFirst(SsoClaimTypes.PermissionVersion)?.Value;
			Assert.IsFalse(string.IsNullOrWhiteSpace(versionAfter));
			Assert.AreNotEqual(versionBefore, versionAfter, "perm_ver should bump when policy catalog changes.");
		}

		[TestMethod]
		public async Task Effective_Menus_Should_Filter_By_Branch_Permissions()
		{
			using var client = ServerHelper.Create().CreateClient();

			var hq = await client.GetAsync(
				$"/api/identity/menus/effective" +
				$"?userId={IdentitySeed.DevUserId}" +
				$"&organizationId={IdentitySeed.DevOrganizationId}" +
				$"&branchId={IdentitySeed.DevBranchHqId}" +
				$"&clientId={SsoClients.DevSpaClientId}");

			Assert.AreEqual(HttpStatusCode.OK, hq.StatusCode);
			using var hqJson = JsonDocument.Parse(await hq.Content.ReadAsStringAsync());
			var hqMenus = hqJson.RootElement.GetProperty("menus").EnumerateArray().Select(x => x.GetProperty("code").GetString()).ToList();
			CollectionAssert.Contains(hqMenus, "home");
			CollectionAssert.Contains(hqMenus, "hq-reports");
			CollectionAssert.DoesNotContain(hqMenus, "filial-ops");

			var filial = await client.GetAsync(
				$"/api/identity/menus/effective" +
				$"?userId={IdentitySeed.DevUserId}" +
				$"&organizationId={IdentitySeed.DevOrganizationId}" +
				$"&branchId={IdentitySeed.DevBranchFilialId}" +
				$"&clientId={SsoClients.DevSpaClientId}");

			Assert.AreEqual(HttpStatusCode.OK, filial.StatusCode);
			using var filialJson = JsonDocument.Parse(await filial.Content.ReadAsStringAsync());
			var filialMenus = filialJson.RootElement.GetProperty("menus").EnumerateArray().Select(x => x.GetProperty("code").GetString()).ToList();
			CollectionAssert.Contains(filialMenus, "home");
			CollectionAssert.Contains(filialMenus, "filial-ops");
			CollectionAssert.DoesNotContain(filialMenus, "hq-reports");
		}
	}
}
