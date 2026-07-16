using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.AuthClientMetadata.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class AuthClientsScenarios
	{
		[TestMethod]
		public async Task Seed_System_Clients_Should_Have_FirstParty_Never_Metadata()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

			var spa = await db.AuthClientMetadata.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == SsoClients.DevSpaClientId);
			Assert.IsNotNull(spa);
			Assert.IsTrue(spa!.IsSystem);
			Assert.IsTrue(spa.IsFirstParty);
			Assert.AreEqual(AuthClientConsentPolicies.Never, spa.RequireConsent);
			Assert.IsTrue(spa.IsEnabled);
		}

		[TestMethod]
		public async Task Create_Should_Reject_Never_Without_FirstParty()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var payload = new
			{
				clientId = $"third-{Guid.NewGuid():N}",
				displayName = "Third party",
				clientType = "public",
				isFirstParty = false,
				requireConsent = AuthClientConsentPolicies.Never,
				redirectUris = new[] { "https://localhost/callback" },
				allowedScopes = new[] { "openid" }
			};

			var response = await client.PostAsync(
				"/api/identity/auth-clients",
				new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
			var body = await response.Content.ReadAsStringAsync();

			Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, body);
			Assert.IsTrue(body.Contains("never_requires_first_party", StringComparison.OrdinalIgnoreCase), body);
		}

		[TestMethod]
		public async Task Create_Public_Pkce_And_Confidential_Should_Succeed()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var spaId = $"spa-{Guid.NewGuid():N}";
			var spaResponse = await client.PostAsync(
				"/api/identity/auth-clients",
				new StringContent(JsonConvert.SerializeObject(new
				{
					clientId = spaId,
					displayName = "Test SPA",
					clientType = "public",
					isFirstParty = false,
					requireConsent = AuthClientConsentPolicies.Always,
					consentRememberDays = 30,
					redirectUris = new[] { "https://localhost/callback" },
					allowedScopes = new[] { "openid", "offline_access", "dev-product.reports" },
					requirePkce = true,
					allowAuthorizationCode = true
				}), Encoding.UTF8, "application/json"));
			var spaBody = await spaResponse.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, spaResponse.StatusCode, spaBody);

			var svcId = $"svc-{Guid.NewGuid():N}";
			var svcResponse = await client.PostAsync(
				"/api/identity/auth-clients",
				new StringContent(JsonConvert.SerializeObject(new
				{
					clientId = svcId,
					displayName = "Test Service",
					clientType = "confidential",
					isFirstParty = true,
					requireConsent = AuthClientConsentPolicies.Never,
					redirectUris = Array.Empty<string>(),
					allowedScopes = new[] { "openid" },
					allowAuthorizationCode = false,
					allowClientCredentials = true,
					requirePkce = false
				}), Encoding.UTF8, "application/json"));
			var svcBody = await svcResponse.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, svcResponse.StatusCode, svcBody);

			using var svcJson = JsonDocument.Parse(svcBody);
			Assert.IsFalse(string.IsNullOrWhiteSpace(svcJson.RootElement.GetProperty("client_secret").GetString()));

			var get = await client.GetAsync($"/api/identity/auth-clients/{spaId}");
			var getBody = await get.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, get.StatusCode, getBody);
			Assert.IsTrue(getBody.Contains(AuthClientConsentPolicies.Always), getBody);
		}

		[TestMethod]
		public async Task RotateSecret_Should_Invalidate_Previous_Secret()
		{
			using var server = ServerHelper.Create();
			using var admin = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var clientId = $"rot-{Guid.NewGuid():N}";
			var create = await admin.PostAsync(
				"/api/identity/auth-clients",
				new StringContent(JsonConvert.SerializeObject(new
				{
					clientId,
					displayName = "Rotate me",
					clientType = "confidential",
					isFirstParty = true,
					requireConsent = AuthClientConsentPolicies.Never,
					allowedScopes = new[] { "openid" },
					allowAuthorizationCode = false,
					allowClientCredentials = true,
					requirePkce = false
				}), Encoding.UTF8, "application/json"));
			var createBody = await create.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, create.StatusCode, createBody);
			using var createJson = JsonDocument.Parse(createBody);
			var oldSecret = createJson.RootElement.GetProperty("client_secret").GetString()!;

			var rotate = await admin.PostAsync($"/api/identity/auth-clients/{clientId}/rotate-secret", null);
			var rotateBody = await rotate.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, rotate.StatusCode, rotateBody);
			using var rotateJson = JsonDocument.Parse(rotateBody);
			var newSecret = rotateJson.RootElement.GetProperty("client_secret").GetString()!;
			Assert.AreNotEqual(oldSecret, newSecret);

			using var http = server.CreateClient();
			using var oldToken = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["grant_type"] = "client_credentials",
				["client_id"] = clientId,
				["client_secret"] = oldSecret,
				["scope"] = "openid"
			});
			var oldResponse = await http.PostAsync("/connect/token", oldToken);
			Assert.IsTrue(
				oldResponse.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized,
				$"old secret should fail: {oldResponse.StatusCode}");

			using var newToken = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["grant_type"] = "client_credentials",
				["client_id"] = clientId,
				["client_secret"] = newSecret,
				["scope"] = "openid"
			});
			var newResponse = await http.PostAsync("/connect/token", newToken);
			var newBody = await newResponse.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, newResponse.StatusCode, newBody);
		}

		[TestMethod]
		public async Task Disable_Should_Block_Token_Endpoint()
		{
			using var server = ServerHelper.Create();
			using var admin = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var clientId = $"dis-{Guid.NewGuid():N}";
			var create = await admin.PostAsync(
				"/api/identity/auth-clients",
				new StringContent(JsonConvert.SerializeObject(new
				{
					clientId,
					displayName = "Disable me",
					clientType = "confidential",
					isFirstParty = true,
					requireConsent = AuthClientConsentPolicies.Never,
					allowedScopes = new[] { "openid" },
					allowAuthorizationCode = false,
					allowClientCredentials = true,
					requirePkce = false
				}), Encoding.UTF8, "application/json"));
			var createBody = await create.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, create.StatusCode, createBody);
			using var createJson = JsonDocument.Parse(createBody);
			var secret = createJson.RootElement.GetProperty("client_secret").GetString()!;

			var disable = await admin.PostAsync($"/api/identity/auth-clients/{clientId}/disable", null);
			Assert.AreEqual(HttpStatusCode.OK, disable.StatusCode, await disable.Content.ReadAsStringAsync());

			using var http = server.CreateClient();
			using var token = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["grant_type"] = "client_credentials",
				["client_id"] = clientId,
				["client_secret"] = secret,
				["scope"] = "openid"
			});
			var response = await http.PostAsync("/connect/token", token);
			var body = await response.Content.ReadAsStringAsync();
			Assert.IsTrue(
				response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized,
				body);
			Assert.IsTrue(body.Contains("disabled", StringComparison.OrdinalIgnoreCase)
				|| body.Contains("invalid_client", StringComparison.OrdinalIgnoreCase), body);
		}

		[TestMethod]
		public async Task Create_Always_Client_Should_Persist_Consent_Policy()
		{
			using var server = ServerHelper.Create();
			using var admin = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var clientId = $"consent-{Guid.NewGuid():N}";
			var create = await admin.PostAsync(
				"/api/identity/auth-clients",
				new StringContent(JsonConvert.SerializeObject(new
				{
					clientId,
					displayName = "Consent Always",
					clientType = "public",
					isFirstParty = false,
					requireConsent = AuthClientConsentPolicies.Always,
					consentRememberDays = 30,
					redirectUris = new[] { "https://localhost/callback" },
					allowedScopes = new[] { "openid" },
					requirePkce = true,
					allowAuthorizationCode = true
				}), Encoding.UTF8, "application/json"));
			Assert.AreEqual(HttpStatusCode.OK, create.StatusCode, await create.Content.ReadAsStringAsync());

			var get = await admin.GetAsync($"/api/identity/auth-clients/{clientId}");
			var body = await get.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, get.StatusCode, body);
			Assert.IsTrue(body.Contains(AuthClientConsentPolicies.Always), body);
			Assert.IsTrue(body.Contains("30") || body.Contains("\"consent_remember_days\":30"), body);

			using var anonymous = server.CreateClient();
			var consent = await anonymous.GetAsync(
				$"/Account/Consent?returnUrl={Uri.EscapeDataString("/")}&clientId={Uri.EscapeDataString(clientId)}&scope=openid");
			var location = consent.Headers.Location?.ToString() ?? string.Empty;
			var consentBody = await consent.Content.ReadAsStringAsync();
			Assert.IsTrue(
				location.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase)
				|| consentBody.Contains("Login", StringComparison.OrdinalIgnoreCase)
				|| consent.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found or HttpStatusCode.Unauthorized,
				$"Anonymous consent should challenge login. Status={consent.StatusCode}. Location={location}");
		}

		[TestMethod]
		public void ConsentEvaluator_Matrix_Should_Match_Policies()
		{
			Assert.IsFalse(AuthClientConsentEvaluator.ShouldPrompt(AuthClientConsentPolicies.Never, false));
			Assert.IsFalse(AuthClientConsentEvaluator.ShouldPrompt(AuthClientConsentPolicies.Never, true));
			Assert.IsTrue(AuthClientConsentEvaluator.ShouldPrompt(AuthClientConsentPolicies.Always, false));
			Assert.IsFalse(AuthClientConsentEvaluator.ShouldPrompt(AuthClientConsentPolicies.Always, true));
			Assert.IsTrue(AuthClientConsentEvaluator.ShouldPrompt(AuthClientConsentPolicies.First, false));
			Assert.IsFalse(AuthClientConsentEvaluator.ShouldPrompt(AuthClientConsentPolicies.First, true));

			Assert.IsTrue(AuthClientConsentEvaluator.IsRememberExpired(
				AuthClientConsentPolicies.Always,
				DateTimeOffset.UtcNow.AddDays(-200),
				180,
				DateTime.UtcNow));
			Assert.IsFalse(AuthClientConsentEvaluator.IsRememberExpired(
				AuthClientConsentPolicies.Always,
				DateTimeOffset.UtcNow.AddDays(-10),
				180,
				DateTime.UtcNow));
			Assert.IsFalse(AuthClientConsentEvaluator.IsRememberExpired(
				AuthClientConsentPolicies.First,
				DateTimeOffset.UtcNow.AddDays(-400),
				180,
				DateTime.UtcNow));
		}

		[TestMethod]
		public async Task Seed_Never_Client_Authorize_Without_Login_Should_Not_Land_On_Consent()
		{
			using var server = ServerHelper.Create();
			using var browser = server.CreateClient();

			var authorizeUrl =
				$"/connect/authorize?client_id={Uri.EscapeDataString(SsoClients.DevSpaClientId)}"
				+ "&response_type=code"
				+ "&scope=openid"
				+ "&redirect_uri=" + Uri.EscapeDataString("https://localhost/callback")
				+ "&code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM"
				+ "&code_challenge_method=S256"
				+ "&state=xyz";

			var authorize = await browser.GetAsync(authorizeUrl);
			var location = authorize.Headers.Location?.ToString() ?? string.Empty;
			var body = await authorize.Content.ReadAsStringAsync();

			Assert.IsFalse(
				location.Contains("/Account/Consent", StringComparison.OrdinalIgnoreCase)
				|| body.Contains("Authorize application", StringComparison.OrdinalIgnoreCase),
				$"Unauthenticated authorize should go to Login, not Consent. Location={location}");
			Assert.IsTrue(
				location.Contains("/Account/Login", StringComparison.OrdinalIgnoreCase)
				|| body.Contains("Login", StringComparison.OrdinalIgnoreCase),
				$"Expected login challenge. Location={location}");
		}

		[TestMethod]
		public async Task AuthScopes_List_Should_Include_Product_Convention_Scopes()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var response = await client.GetAsync("/api/identity/auth-scopes");
			var body = await response.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);
			Assert.IsTrue(body.Contains("dev-product.reports"), body);
			Assert.IsTrue(body.Contains("sso-platform.admin"), body);
		}

		[TestMethod]
		public void Metadata_Create_Should_Reject_Never_Without_FirstParty()
		{
			Assert.ThrowsExactly<InvalidOperationException>(() =>
				AuthClientMetadataEntity.Create(
					"x",
					"x",
					isSystem: false,
					isFirstParty: false,
					AuthClientConsentPolicies.Never));
		}
	}
}
