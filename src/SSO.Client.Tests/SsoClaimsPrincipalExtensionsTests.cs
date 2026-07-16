using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SSO.Client;
using SSO.Client.Authorization;
using SSO.Shared.Identity;

namespace SSO.Client.Tests
{
	[TestClass]
	public class SsoClaimsPrincipalExtensionsTests
	{
		[TestMethod]
		public void GetPermissions_Should_Read_Multi_Value_Claims()
		{
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(SsoClaimTypes.Permissions, "sso.access"),
				new Claim(SsoClaimTypes.Permissions, "hq.reports"),
				new Claim(SsoClaimTypes.PermissionVersion, "123"),
				new Claim(SsoClaimTypes.OrganizationId, IdentitySeedOrg)
			}, "test"));

			CollectionAssert.AreEquivalent(
				new[] { "hq.reports", "sso.access" },
				user.GetPermissions().ToList());
			Assert.AreEqual("123", user.GetPermissionVersion());
			Assert.AreEqual(Guid.Parse(IdentitySeedOrg), user.GetOrganizationId());
			Assert.IsTrue(user.HasPermission("hq.reports"));
			Assert.IsFalse(user.HasPermission("missing"));
		}

		private const string IdentitySeedOrg = "11111111-1111-1111-1111-111111111111";
	}

	[TestClass]
	public class PermissionAuthorizationHandlerTests
	{
		[TestMethod]
		public async Task Handler_Should_Succeed_When_Any_Permission_Matches()
		{
			var handler = new PermissionAuthorizationHandler();
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(SsoClaimTypes.Permissions, "hq.reports")
			}, "test"));
			var requirement = new PermissionRequirement("filial.ops", "hq.reports");
			var context = new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);

			await handler.HandleAsync(context);

			Assert.IsTrue(context.HasSucceeded);
		}

		[TestMethod]
		public async Task Handler_Should_Fail_When_Permission_Missing()
		{
			var handler = new PermissionAuthorizationHandler();
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(SsoClaimTypes.Permissions, "sso.access")
			}, "test"));
			var requirement = new PermissionRequirement("hq.reports");
			var context = new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);

			await handler.HandleAsync(context);

			Assert.IsFalse(context.HasSucceeded);
		}
	}
}
