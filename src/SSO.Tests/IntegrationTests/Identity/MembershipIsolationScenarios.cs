using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Identity;
using System.Net;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class MembershipIsolationScenarios
	{
		[TestMethod]
		public async Task GET_Memberships_Filtered_By_Organization_Should_Isolate_Tenants()
		{
			var orgA = IdentityCollections.Organization(10, "org-a");
			var orgB = IdentityCollections.Organization(11, "org-b");

			using var server = ServerHelper.Create()
				.SetupData<IdentityDbContext, Organization>(new[] { orgA, orgB });

			using (var scope = server.Services.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
				var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

				var userA = new User { Email = "a@test.local", UserName = "a@test.local" };
				userA.MarkCreated();
				var userB = new User { Email = "b@test.local", UserName = "b@test.local" };
				userB.MarkCreated();

				Assert.IsTrue((await userManager.CreateAsync(userA, "ChangeMe!123")).Succeeded);
				Assert.IsTrue((await userManager.CreateAsync(userB, "ChangeMe!123")).Succeeded);

				var membershipA = IdentityCollections.Membership(20, userA.Id, orgA.Id);
				var membershipB = IdentityCollections.Membership(21, userB.Id, orgB.Id);
				context.Memberships.AddRange(membershipA, membershipB);
				await context.SaveChangesAsync();
			}

			using var client = AdminAuthTestHelper.CreateOrgAdminClient(server, orgA.Id);
			var response = await client.GetAsync($"/api/identity/memberships?organizationId={orgA.Id}");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var body = JObject.Parse(await response.Content.ReadAsStringAsync());
			var data = body["data"] as JArray ?? body["Data"] as JArray;
			Assert.IsNotNull(data);

			foreach (var item in data!)
			{
				var organizationId = item["organizationId"]?.ToString() ?? item["OrganizationId"]?.ToString();
				Assert.AreEqual(orgA.Id.ToString(), organizationId, ignoreCase: true);
			}
		}
	}
}
