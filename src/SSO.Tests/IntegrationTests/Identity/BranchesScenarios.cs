using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;
using System.Net;
using System.Text;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class BranchesScenarios
	{
		[TestMethod]
		public async Task POST_Branch_Should_Return_Created()
		{
			var branch = new Branch
			{
				OrganizationId = IdentitySeed.DevOrganizationId,
				ParentBranchId = IdentitySeed.DevBranchHqId,
				Name = "Regional",
				Code = $"reg-{Guid.NewGuid():N}".Substring(0, 16)
			};

			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreateOrgAdminClient(server, IdentitySeed.DevOrganizationId);

			var response = await client.PostAsync(
				"/api/identity/branches",
				new StringContent(JsonConvert.SerializeObject(branch), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		}

		[TestMethod]
		public async Task POST_Branch_For_Other_Org_As_OrgAdmin_Should_Fail()
		{
			var otherOrgId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
			var branch = new Branch
			{
				OrganizationId = otherOrgId,
				Name = "Foreign",
				Code = $"frn-{Guid.NewGuid():N}".Substring(0, 16)
			};

			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreateOrgAdminClient(server, IdentitySeed.DevOrganizationId);

			var response = await client.PostAsync(
				"/api/identity/branches",
				new StringContent(JsonConvert.SerializeObject(branch), Encoding.UTF8, "application/json"));

			// Domain throws UnauthorizedAccessException → wrapped error response (not 201)
			Assert.AreNotEqual(HttpStatusCode.Created, response.StatusCode);
		}
	}
}
