using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Infrastructures.Data.Identity;
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

			using var client = ServerHelper.Create().CreateClient();

			var response = await client.PostAsync(
				"/api/identity/branches",
				new StringContent(JsonConvert.SerializeObject(branch), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		}
	}
}
