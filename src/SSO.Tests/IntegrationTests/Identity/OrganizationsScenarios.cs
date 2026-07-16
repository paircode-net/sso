using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Identity;
using System.Net;
using System.Text;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class OrganizationsScenarios
	{
		[TestMethod]
		public async Task POST_Organization_Should_Return_Created()
		{
			var organization = new Organization { Name = "Acme", Code = "acme" };

			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var response = await client.PostAsync(
				"/api/identity/organizations",
				new StringContent(JsonConvert.SerializeObject(organization), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		}

		[TestMethod]
		public async Task GET_Organization_By_Id_Should_Return_Ok()
		{
			var seed = IdentityCollections.Organization(1, "org-one");

			using var server = ServerHelper.Create()
				.SetupData<IdentityDbContext, Organization>(new[] { seed });
			using var client = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var response = await client.GetAsync($"/api/identity/organizations/{seed.Id}");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[TestMethod]
		public async Task GET_Organization_Without_Auth_Should_Return_Unauthorized()
		{
			using var client = ServerHelper.Create().CreateClient();
			var response = await client.GetAsync($"/api/identity/organizations/{IdentitySeed.DevOrganizationId}");
			Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		}

		[TestMethod]
		public async Task GET_Organization_With_Only_Access_Permission_Should_Return_Forbidden()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreateAuthenticatedClient(server, IdentitySeed.PermissionAccess);

			var response = await client.GetAsync($"/api/identity/organizations/{IdentitySeed.DevOrganizationId}");
			Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
		}
	}
}
