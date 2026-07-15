using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Infrastructures.Data.Identity;
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

			using var client = ServerHelper.Create().CreateClient();

			var response = await client.PostAsync(
				"/api/identity/organizations",
				new StringContent(JsonConvert.SerializeObject(organization), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		}

		[TestMethod]
		public async Task GET_Organization_By_Id_Should_Return_Ok()
		{
			var seed = IdentityCollections.Organization(1, "org-one");

			using var client = ServerHelper.Create()
				.SetupData<IdentityDbContext, Organization>(new[] { seed })
				.CreateClient();

			var response = await client.GetAsync($"/api/identity/organizations/{seed.Id}");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}
	}
}
