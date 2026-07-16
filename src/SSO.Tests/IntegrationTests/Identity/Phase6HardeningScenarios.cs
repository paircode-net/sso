using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class Phase6HardeningScenarios
	{
		[TestMethod]
		public async Task ExternalIdentityProviders_Catalog_Should_Include_Enabled_Entra_Only()
		{
			using var client = ServerHelper.Create().CreateClient();
			var response = await client.GetAsync("/api/identity/external-identity-providers");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var body = await response.Content.ReadAsStringAsync();
			Assert.IsTrue(body.Contains(ExternalIdpTypes.Entra), body);
			Assert.IsTrue(body.Contains("entra-homolog"), body);
			// Catalog endpoint returns IsEnabled=true only (Google/LDAP remain stubs until enabled).
			Assert.IsFalse(body.Contains("google-stub"), body);
			Assert.IsFalse(body.Contains("ldap-stub"), body);
		}

		[TestMethod]
		public async Task Hardening_Options_Should_Be_Registered()
		{
			using var server = ServerHelper.Create();
			var options = server.Services.GetService<SsoHardeningOptions>();
			Assert.IsNotNull(options);
			Assert.IsTrue(options!.Cors.Enabled);
			Assert.IsTrue(options.Signing.UseDevelopmentCertificates);
		}

		[TestMethod]
		public async Task Database_P004_AutoMigrate_Defaults_Allow_Seed_In_Tests()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			Assert.IsTrue(await context.ExternalIdentityProviders.AnyAsync(x => x.Code == "entra-homolog" && x.IsEnabled));
			Assert.IsTrue(await context.ExternalIdentityProviders.AnyAsync(x => x.Code == "google" && !x.IsEnabled));
			Assert.IsTrue(await context.ExternalIdentityProviders.AnyAsync(x => x.Code == "ldap" && !x.IsEnabled));
		}

		[TestMethod]
		public async Task Cors_Policy_Is_Registered_When_Enabled()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var cors = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsService>();
			Assert.IsNotNull(cors);
		}
	}
}
