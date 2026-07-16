using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class FederationScenarios
	{
		[TestMethod]
		public async Task AutoLink_Should_Require_Verified_Email_For_Google()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var federated = scope.ServiceProvider.GetRequiredService<FederatedAccountService>();

			var outcome = await federated.ResolveOrProvisionAsync(
				AuthenticationSchemes.Google,
				"google-sub-1",
				IdentitySeed.DevUserEmail,
				emailVerified: false,
				displayName: "Google");

			Assert.IsFalse(outcome.Succeeded);
			Assert.AreEqual("email_not_verified", outcome.Error);
		}

		[TestMethod]
		public async Task AutoLink_Should_Link_When_Email_Verified()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var federated = scope.ServiceProvider.GetRequiredService<FederatedAccountService>();

			var outcome = await federated.ResolveOrProvisionAsync(
				AuthenticationSchemes.Google,
				"google-sub-2",
				IdentitySeed.DevUserEmail,
				emailVerified: true,
				displayName: "Google");

			Assert.IsTrue(outcome.Succeeded, outcome.Error + " " + outcome.Detail);
			Assert.AreEqual("auto_linked", outcome.Detail);
			Assert.AreEqual(IdentitySeed.DevUserId, outcome.User!.Id);
		}

		[TestMethod]
		public async Task Jit_Should_Be_Rejected_When_Flag_Off()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var federated = scope.ServiceProvider.GetRequiredService<FederatedAccountService>();

			var outcome = await federated.ResolveOrProvisionAsync(
				AuthenticationSchemes.Google,
				"google-sub-3",
				$"new-{Guid.NewGuid():N}@example.com",
				emailVerified: true,
				displayName: "Google");

			Assert.IsFalse(outcome.Succeeded);
			Assert.AreEqual("user_not_provisioned", outcome.Error);
		}

		[TestMethod]
		public async Task Jit_Should_Create_When_Flag_On()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var idp = await db.ExternalIdentityProviders.FirstAsync(x => x.Id == IdentitySeed.DevGoogleIdpId);
			idp.AllowJitProvisioning = true;
			idp.IsEnabled = true;
			await db.SaveChangesAsync();

			var federated = scope.ServiceProvider.GetRequiredService<FederatedAccountService>();
			var email = $"jit-{Guid.NewGuid():N}@example.com";
			var outcome = await federated.ResolveOrProvisionAsync(
				AuthenticationSchemes.Google,
				"google-sub-jit",
				email,
				emailVerified: true,
				displayName: "Google");

			Assert.IsTrue(outcome.Succeeded, outcome.Error + " " + outcome.Detail);
			Assert.AreEqual("jit_created", outcome.Detail);
			Assert.AreEqual(email, outcome.User!.Email);
		}

		[TestMethod]
		public void GroupMatches_Should_Match_Cn_And_Full_Dn()
		{
			Assert.IsTrue(LdapGroupRoleSyncService.GroupMatches(
				"CN=AppAdmins,OU=Groups,DC=contoso,DC=com",
				"CN=AppAdmins,OU=Groups,DC=contoso,DC=com"));
			Assert.IsTrue(LdapGroupRoleSyncService.GroupMatches(
				"CN=AppAdmins,OU=Groups,DC=contoso,DC=com",
				"AppAdmins"));
			Assert.IsFalse(LdapGroupRoleSyncService.GroupMatches(
				"CN=AppAdmins,OU=Groups,DC=contoso,DC=com",
				"Other"));
		}

		[TestMethod]
		public async Task LdapGroupSync_Should_Create_Assignment()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var sync = scope.ServiceProvider.GetRequiredService<LdapGroupRoleSyncService>();

			db.LdapGroupRoleMaps.Add(LdapGroupRoleMap.Create(
				IdentitySeed.DevOrganizationId,
				"AppAdmins",
				IdentitySeed.DevRoleOrgAdminId,
				IdentitySeed.DevProductId));
			await db.SaveChangesAsync();

			var added = await sync.SyncAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				new[] { "CN=AppAdmins,OU=Groups,DC=contoso,DC=com" });

			Assert.IsTrue(added >= 1);
			Assert.IsTrue(await db.UserRoleAssignments.AnyAsync(x =>
				!x.IsDeleted
				&& x.UserId == IdentitySeed.DevUserId
				&& x.RoleId == IdentitySeed.DevRoleOrgAdminId
				&& x.OrganizationId == IdentitySeed.DevOrganizationId));
		}

		[TestMethod]
		public void EmailVerified_Claim_Parsing()
		{
			Assert.IsTrue(FederatedAccountService.IsEmailVerifiedFromClaims(
				new[] { new Claim("email_verified", "true") },
				AuthenticationSchemes.Google));
			Assert.IsFalse(FederatedAccountService.IsEmailVerifiedFromClaims(
				Array.Empty<Claim>(),
				AuthenticationSchemes.Google));
			Assert.IsTrue(FederatedAccountService.IsEmailVerifiedFromClaims(
				Array.Empty<Claim>(),
				AuthenticationSchemes.Ldap));
		}
	}
}
