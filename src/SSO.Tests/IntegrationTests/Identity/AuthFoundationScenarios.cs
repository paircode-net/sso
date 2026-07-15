using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Tests.Helpers;
using System.Net;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class AuthFoundationScenarios
	{
		[TestMethod]
		public async Task Host_With_Identity_And_OpenIddict_Should_Respond_Ok_On_Samples()
		{
			using var client = ServerHelper.Create().CreateClient();

			var response = await client.GetAsync("/api/default/samples");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[TestMethod]
		public void Identity_And_OpenIddict_Services_Should_Be_Registered()
		{
			using var server = ServerHelper.Create();

			Assert.IsNotNull(server.Services.GetService<IdentityDbContext>());
			Assert.IsNotNull(server.Services.GetService<UserManager<User>>());
			Assert.IsNotNull(server.Services.GetService<IOpenIddictApplicationManager>());
		}
	}
}
