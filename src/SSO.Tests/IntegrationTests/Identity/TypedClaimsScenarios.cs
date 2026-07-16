using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class TypedClaimsScenarios
	{
		[TestMethod]
		public async Task Org_Context_Should_Emit_User_Override_Department_Finance()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectiveClaimsResolver>();

			var claims = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				branchId: null,
				SsoClients.DevSpaClientId);

			Assert.AreEqual("finance", claims[IdentitySeed.ClaimDepartment]);
			Assert.IsFalse(claims.ContainsKey(IdentitySeed.ClaimMfaRequired));
			Assert.IsFalse(claims.ContainsKey(IdentitySeed.ClaimCanExport));
		}

		[TestMethod]
		public async Task Hq_Context_Should_Include_Role_CanExport_And_Branch_Mfa()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectiveClaimsResolver>();

			var claims = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchHqId,
				SsoClients.DevSpaClientId);

			Assert.AreEqual("finance", claims[IdentitySeed.ClaimDepartment]);
			Assert.AreEqual("true", claims[IdentitySeed.ClaimCanExport]);
			Assert.AreEqual("true", claims[IdentitySeed.ClaimMfaRequired]);
		}

		[TestMethod]
		public async Task Filial_Context_Should_Not_Get_Hq_Only_Mfa_Or_Hq_Role_Export()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectiveClaimsResolver>();

			var claims = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				IdentitySeed.DevBranchFilialId,
				SsoClients.DevSpaClientId);

			Assert.AreEqual("finance", claims[IdentitySeed.ClaimDepartment]);
			Assert.IsFalse(
				claims.ContainsKey(IdentitySeed.ClaimMfaRequired),
				"HQ-only mfa_required must not apply at Filial (ADR-004).");
			Assert.IsFalse(
				claims.ContainsKey(IdentitySeed.ClaimCanExport),
				"hq-manager RoleClaim must not apply without HQ branch assignment.");
		}

		[TestMethod]
		public async Task User_Override_Should_Win_Over_RoleClaim()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var resolver = scope.ServiceProvider.GetRequiredService<IEffectiveClaimsResolver>();

			// RoleClaim org-member has department=operations; user assignment overrides to finance
			var claims = await resolver.ResolveAsync(
				IdentitySeed.DevUserId,
				IdentitySeed.DevOrganizationId,
				branchId: null,
				SsoClients.DevSpaClientId);

			Assert.AreEqual("finance", claims[IdentitySeed.ClaimDepartment]);
			Assert.AreNotEqual("operations", claims[IdentitySeed.ClaimDepartment]);
		}

		[TestMethod]
		public async Task TokenClaimsFactory_Should_Emit_Sso_C_Prefix_And_Claim_Ver()
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

			Assert.AreEqual(
				"finance",
				principal.FindFirst(TypedClaimNames.ToJwtType(IdentitySeed.ClaimDepartment))?.Value);
			Assert.AreEqual(
				"true",
				principal.FindFirst(TypedClaimNames.ToJwtType(IdentitySeed.ClaimCanExport))?.Value);
			Assert.IsFalse(string.IsNullOrWhiteSpace(principal.FindFirst(SsoClaimTypes.ClaimVersion)?.Value));
			Assert.IsFalse(string.IsNullOrWhiteSpace(principal.FindFirst(SsoClaimTypes.PermissionVersion)?.Value));
		}

		[TestMethod]
		public async Task Claim_Ver_Should_Bump_When_Assignment_Changes()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var versionProvider = scope.ServiceProvider.GetRequiredService<IClaimPolicyVersionProvider>();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

			var before = await versionProvider.GetVersionAsync();
			await Task.Delay(15);

			var assignment = await db.UserClaimAssignments
				.FirstAsync(x => !x.IsDeleted && x.UserId == IdentitySeed.DevUserId
					&& x.ClaimDefinitionId == IdentitySeed.DevClaimDepartmentId);
			assignment.Value = "legal";
			assignment.TouchUpdated();
			await db.SaveChangesAsync();

			var after = await versionProvider.GetVersionAsync();
			Assert.AreNotEqual(before, after);
		}

		[TestMethod]
		public async Task Api_Should_Create_ClaimDefinition_And_Reject_Invalid_ValueType()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var bad = await client.PostAsync(
				"/api/identity/claim-definitions",
				new StringContent(JsonConvert.SerializeObject(new
				{
					code = "bad_json",
					name = "Bad",
					valueType = "json"
				}), Encoding.UTF8, "application/json"));
			Assert.AreEqual(HttpStatusCode.BadRequest, bad.StatusCode);

			var code = $"dept_{Guid.NewGuid():N}"[..20];
			var ok = await client.PostAsync(
				"/api/identity/claim-definitions",
				new StringContent(JsonConvert.SerializeObject(new
				{
					code,
					name = "Custom Dept",
					valueType = ClaimValueTypes.String,
					productId = IdentitySeed.DevProductId
				}), Encoding.UTF8, "application/json"));
			Assert.AreEqual(HttpStatusCode.OK, ok.StatusCode, await ok.Content.ReadAsStringAsync());
		}

		[TestMethod]
		public void ClaimValueTypes_Should_Validate_Scalars()
		{
			Assert.IsTrue(ClaimValueTypes.TryValidate(ClaimValueTypes.Bool, "true", out _));
			Assert.IsFalse(ClaimValueTypes.TryValidate(ClaimValueTypes.Bool, "yes", out var err));
			Assert.AreEqual("value_must_be_bool", err);
			Assert.IsFalse(ClaimValueTypes.TryValidate(ClaimValueTypes.Int, "x", out _));
			Assert.ThrowsExactly<InvalidOperationException>(() =>
				ClaimDefinition.Create("x", "x", "json"));
		}

		[TestMethod]
		public async Task Jwt_Size_With_Many_Claims_Should_Stay_Under_Budget()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var claimsFactory = scope.ServiceProvider.GetRequiredService<TokenClaimsFactory>();

			for (var i = 0; i < 50; i++)
			{
				var def = ClaimDefinition.Create($"bulk_{i:D2}", $"Bulk {i}", ClaimValueTypes.String, IdentitySeed.DevProductId);
				db.ClaimDefinitions.Add(def);
				db.UserClaimAssignments.Add(UserClaimAssignment.Create(
					IdentitySeed.DevUserId,
					def.Id,
					$"v{i}",
					IdentitySeed.DevProductId,
					IdentitySeed.DevOrganizationId,
					branchId: null));
			}

			await db.SaveChangesAsync();

			var user = await userManager.FindByIdAsync(IdentitySeed.DevUserId.ToString());
			var principal = await claimsFactory.CreateUserPrincipalAsync(
				user!,
				new[] { OpenIddictConstants.Scopes.OpenId },
				SsoClients.DevSpaClientId,
				IdentitySeed.DevOrganizationId,
				branchId: null);

			var typedCount = principal.Claims
				.Count(c => c.Type.StartsWith(TypedClaimNames.Prefix, StringComparison.OrdinalIgnoreCase));
			Assert.IsTrue(typedCount >= 50, $"Expected >=50 typed claims, got {typedCount}");

			// Rough JWT budget: access tokens with ~50 short claims should stay well under 8 KB payload.
			var payloadChars = principal.Claims.Sum(c => c.Type.Length + c.Value.Length);
			Assert.IsTrue(payloadChars < 8000, $"Claim payload chars={payloadChars} exceeded budget.");
		}
	}
}
