using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Tests.Helpers;
using System.Net;
using System.Text;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class UsersScenarios
	{
		[TestMethod]
		public async Task POST_User_Should_Return_Created_And_Hash_Password()
		{
			var payload = new
			{
				email = "user1@test.local",
				userName = "user1@test.local",
				password = "ChangeMe!123"
			};

			using var server = ServerHelper.Create();
			using var client = server.CreateClient();

			var response = await client.PostAsync(
				"/api/identity/users",
				new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

			using var scope = server.Services.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var user = await userManager.FindByEmailAsync(payload.email);

			Assert.IsNotNull(user);
			Assert.IsFalse(string.IsNullOrWhiteSpace(user!.PasswordHash));
			Assert.IsTrue(await userManager.CheckPasswordAsync(user, payload.password));
		}

		[TestMethod]
		public async Task POST_User_Response_Data_Should_Not_Expose_Password()
		{
			var payload = new
			{
				email = "user2@test.local",
				userName = "user2@test.local",
				password = "ChangeMe!123"
			};

			using var client = ServerHelper.Create().CreateClient();

			var response = await client.PostAsync(
				"/api/identity/users",
				new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

			var body = JObject.Parse(await response.Content.ReadAsStringAsync());
			var data = body["data"] as JObject ?? body["Data"] as JObject;
			Assert.IsNotNull(data);

			var passwordToken = data!["password"] ?? data["Password"];
			Assert.IsTrue(passwordToken == null || passwordToken.Type == JTokenType.Null || string.IsNullOrEmpty(passwordToken.ToString()));

			var hashToken = data["passwordHash"] ?? data["PasswordHash"];
			Assert.IsTrue(hashToken == null || hashToken.Type == JTokenType.Null || string.IsNullOrEmpty(hashToken.ToString()));
		}
	}
}
