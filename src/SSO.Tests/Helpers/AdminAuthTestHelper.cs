using Microsoft.AspNetCore.TestHost;
using SSO.Shared.Identity;

namespace SSO.Tests.Helpers
{
	public static class AdminAuthTestHelper
	{
		public static HttpClient CreateAuthenticatedClient(
			TestServer server,
			params string[] permissions)
			=> CreateAuthenticatedClient(server, organizationId: null, permissions);

		public static HttpClient CreateAuthenticatedClient(
			TestServer server,
			Guid? organizationId,
			params string[] permissions)
		{
			var client = server.CreateClient();
			ApplyAuthHeaders(client, organizationId, permissions);
			return client;
		}

		public static void ApplyAuthHeaders(
			HttpClient client,
			Guid? organizationId,
			params string[] permissions)
		{
			client.DefaultRequestHeaders.Remove(SsoTestingAuthDefaults.PermissionsHeader);
			client.DefaultRequestHeaders.Remove(SsoTestingAuthDefaults.OrganizationIdHeader);
			client.DefaultRequestHeaders.Remove(SsoTestingAuthDefaults.UserIdHeader);

			client.DefaultRequestHeaders.Add(
				SsoTestingAuthDefaults.PermissionsHeader,
				string.Join(",", permissions));

			if (organizationId is Guid orgId)
			{
				client.DefaultRequestHeaders.Add(
					SsoTestingAuthDefaults.OrganizationIdHeader,
					orgId.ToString("D"));
			}
		}

		public static HttpClient CreatePlatformAdminClient(TestServer server)
			=> CreateAuthenticatedClient(
				server,
				organizationId: null,
				SsoAdminPermissions.Platform,
				SsoAdminPermissions.Org,
				SsoAdminPermissions.AuditRead,
				SsoAdminPermissions.SessionsRevoke,
				SsoAdminPermissions.Menus);

		public static HttpClient CreateOrgAdminClient(TestServer server, Guid organizationId)
			=> CreateAuthenticatedClient(
				server,
				organizationId,
				SsoAdminPermissions.Org,
				SsoAdminPermissions.SessionsRevoke);
	}
}
