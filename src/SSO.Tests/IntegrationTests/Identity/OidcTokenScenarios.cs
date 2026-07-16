using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Identity.Users.Entity;
using OpenIddict.Abstractions;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class OidcTokenScenarios
	{
		[TestMethod]
		public async Task ClientCredentials_Should_Return_AccessToken_Jwt()
		{
			using var client = ServerHelper.Create().CreateClient();

			using var content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["grant_type"] = "client_credentials",
				["client_id"] = SsoClients.DevServiceClientId,
				["client_secret"] = SsoClients.DevServiceClientSecret,
				["scope"] = "openid"
			});

			var response = await client.PostAsync("/connect/token", content);
			var body = await response.Content.ReadAsStringAsync();

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);

			using var json = JsonDocument.Parse(body);
			var accessToken = json.RootElement.GetProperty("access_token").GetString();
			Assert.IsFalse(string.IsNullOrWhiteSpace(accessToken));

			var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
			Assert.IsTrue(jwt.ValidTo > DateTime.UtcNow);
			Assert.IsFalse(string.IsNullOrWhiteSpace(jwt.Issuer));
		}

		[TestMethod]
		public async Task Revocation_Endpoint_Should_Accept_Token()
		{
			using var client = ServerHelper.Create().CreateClient();

			using var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["grant_type"] = "client_credentials",
				["client_id"] = SsoClients.DevServiceClientId,
				["client_secret"] = SsoClients.DevServiceClientSecret
			});

			var tokenResponse = await client.PostAsync("/connect/token", tokenContent);
			var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
			Assert.AreEqual(HttpStatusCode.OK, tokenResponse.StatusCode, tokenBody);

			using var tokenJson = JsonDocument.Parse(tokenBody);
			var accessToken = tokenJson.RootElement.GetProperty("access_token").GetString();

			using var revokeContent = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["token"] = accessToken!,
				["client_id"] = SsoClients.DevServiceClientId,
				["client_secret"] = SsoClients.DevServiceClientSecret
			});

			var revokeResponse = await client.PostAsync("/connect/revoke", revokeContent);
			Assert.IsTrue(
				revokeResponse.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
				$"Unexpected revoke status: {revokeResponse.StatusCode}");
		}

		[TestMethod]
		public async Task TokenClaimsFactory_Should_Include_Organization_And_Permissions_For_Member()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();

			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var claimsFactory = scope.ServiceProvider.GetRequiredService<TokenClaimsFactory>();

			var user = await userManager.FindByIdAsync(IdentitySeed.DevUserId.ToString());
			Assert.IsNotNull(user);

			var principal = await claimsFactory.CreateUserPrincipalAsync(
				user!,
				new[] { OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.Email },
				SsoClients.DevSpaClientId,
				IdentitySeed.DevOrganizationId,
				branchId: null);

			Assert.AreEqual(IdentitySeed.DevOrganizationId.ToString("D"), principal.FindFirst(SsoClaimTypes.OrganizationId)?.Value);
			Assert.IsTrue(principal.FindAll(SsoClaimTypes.Permissions).Any(c => c.Value == "sso.access"));
			Assert.IsFalse(string.IsNullOrWhiteSpace(principal.FindFirst(SsoClaimTypes.SessionId)?.Value));
		}

		[TestMethod]
		public async Task SwitchContext_Grant_Should_Reject_Missing_AccessToken()
		{
			using var client = ServerHelper.Create().CreateClient();

			using var switchRequest = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["grant_type"] = SsoGrantTypes.SwitchContext,
				["client_id"] = SsoClients.DevSpaClientId,
				["organization_id"] = IdentitySeed.DevOrganizationId.ToString("D")
			});

			var switchResponse = await client.PostAsync("/connect/token", switchRequest);
			Assert.AreEqual(HttpStatusCode.BadRequest, switchResponse.StatusCode);
		}

		[TestMethod]
		public async Task Account_Login_Should_Accept_Dev_Credentials()
		{
			using var client = ServerHelper.Create().CreateClient();

			var response = await client.PostAsync("/Account/Login?returnUrl=%2F", new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["Input.Email"] = IdentitySeed.DevUserEmail,
				["Input.Password"] = IdentitySeed.DevUserPassword
			}));

			Assert.IsTrue(
				response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Found,
				$"Unexpected login status: {response.StatusCode}");
		}

		[TestMethod]
		public async Task Authorize_Without_Login_Should_Redirect_To_Login()
		{
			using var server = ServerHelper.Create();
			using var client = new HttpClient(server.CreateHandler())
			{
				BaseAddress = server.BaseAddress
			};

			var authorizeUrl =
				"/connect/authorize" +
				"?client_id=" + Uri.EscapeDataString(SsoClients.DevSpaClientId) +
				"&response_type=code" +
				"&scope=openid%20offline_access" +
				"&redirect_uri=" + Uri.EscapeDataString("https://localhost/callback") +
				"&code_challenge=challenge" +
				"&code_challenge_method=plain" +
				"&state=xyz";

			var response = await client.GetAsync(authorizeUrl);
			var body = await response.Content.ReadAsStringAsync();

			// Cookie auth challenge redirects to Login; TestServer may surface redirect or login HTML.
			Assert.IsTrue(
				response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found or HttpStatusCode.OK,
				$"Unexpected authorize status: {response.StatusCode}. Body: {body}");

			if (response.Headers.Location != null)
			{
				Assert.IsTrue(
					response.Headers.Location.ToString().Contains("/Account/Login", StringComparison.OrdinalIgnoreCase),
					$"Expected redirect to login, got: {response.Headers.Location}");
			}
			else
			{
				Assert.IsTrue(
					body.Contains("Login", StringComparison.OrdinalIgnoreCase) ||
					body.Contains("Email", StringComparison.OrdinalIgnoreCase),
					"Expected login page content when not redirected.");
			}
		}
	}
}
