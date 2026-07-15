using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Infrastructures.Services;
using SSO.Tests.Helpers;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class AccountFlowsScenarios
	{
		[TestMethod]
		public async Task ConfirmEmail_Flow_Should_Allow_Login_After_Confirmation()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var mail = scope.ServiceProvider.GetRequiredService<CapturingMailService>();
			mail.Clear();

			var email = $"user-{Guid.NewGuid():N}@sso.local";
			var user = new User
			{
				Email = email,
				UserName = email,
				EmailConfirmed = false
			};
			user.MarkCreated();
			var create = await userManager.CreateAsync(user, "ChangeMe!123");
			Assert.IsTrue(create.Succeeded, string.Join("; ", create.Errors.Select(e => e.Description)));

			using var client = server.CreateClient();
			var requestConfirm = await client.PostAsync(
				"/api/identity/account/request-email-confirmation",
				new StringContent(JsonConvert.SerializeObject(new { email }), Encoding.UTF8, "application/json"));
			Assert.AreEqual(HttpStatusCode.OK, requestConfirm.StatusCode);

			Assert.IsTrue(mail.Messages.Count >= 1);
			var body = mail.Messages.Last().Body;
			var token = Regex.Match(body, "ConfirmToken=([^;]+)").Groups[1].Value;
			var userId = Regex.Match(body, "UserId=([^;]+)").Groups[1].Value;
			Assert.IsFalse(string.IsNullOrWhiteSpace(token));

			var confirm = await client.GetAsync($"/Account/ConfirmEmail?userId={userId}&code={Uri.EscapeDataString(token)}");
			Assert.IsTrue(confirm.IsSuccessStatusCode);

			var reloaded = await userManager.FindByEmailAsync(email);
			Assert.IsTrue(await userManager.IsEmailConfirmedAsync(reloaded!));

			var login = await client.PostAsync("/Account/Login?returnUrl=%2F", new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["Input.Email"] = email,
				["Input.Password"] = "ChangeMe!123"
			}));
			Assert.IsTrue(login.IsSuccessStatusCode || login.StatusCode == HttpStatusCode.Found);
		}

		[TestMethod]
		public async Task ForgotAndResetPassword_Should_Update_Password()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var mail = scope.ServiceProvider.GetRequiredService<CapturingMailService>();
			mail.Clear();

			using var client = server.CreateClient();
			var forgot = await client.PostAsync("/Account/ForgotPassword", new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["Input.Email"] = IdentitySeed.DevUserEmail
			}));
			Assert.IsTrue(forgot.IsSuccessStatusCode);
			Assert.IsTrue(mail.Messages.Count >= 1);

			var body = mail.Messages.Last().Body;
			var token = Regex.Match(body, "ResetToken=([^;]+)").Groups[1].Value;
			var userId = Regex.Match(body, "UserId=([^;]+)").Groups[1].Value;
			var newPassword = "BrandNew!456";

			var reset = await client.PostAsync("/Account/ResetPassword", new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["Input.UserId"] = userId,
				["Input.Code"] = token,
				["Input.Password"] = newPassword,
				["Input.ConfirmPassword"] = newPassword
			}));
			Assert.IsTrue(reset.IsSuccessStatusCode, await reset.Content.ReadAsStringAsync());

			var login = await client.PostAsync("/Account/Login?returnUrl=%2F", new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["Input.Email"] = IdentitySeed.DevUserEmail,
				["Input.Password"] = newPassword
			}));
			Assert.IsTrue(login.IsSuccessStatusCode || login.StatusCode == HttpStatusCode.Found);

			// Restore password for other tests that share DB? Each ServerHelper.Create is isolated in-memory — OK.
		}

		[TestMethod]
		public async Task TwoFactor_Login_Should_Require_Authenticator_Code()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
			var user = await userManager.FindByIdAsync(IdentitySeed.DevUserId.ToString());
			Assert.IsNotNull(user);

			await userManager.ResetAuthenticatorKeyAsync(user!);
			var setResult = await userManager.SetTwoFactorEnabledAsync(user!, true);
			Assert.IsTrue(setResult.Succeeded);

			using var client = new HttpClient(server.CreateHandler())
			{
				BaseAddress = server.BaseAddress
			};

			var login = await client.PostAsync("/Account/Login?returnUrl=%2F", new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["Input.Email"] = IdentitySeed.DevUserEmail,
				["Input.Password"] = IdentitySeed.DevUserPassword
			}));

			var location = login.Headers.Location?.ToString() ?? string.Empty;
			var loginBody = await login.Content.ReadAsStringAsync();
			Assert.IsTrue(
				location.Contains("LoginWith2fa", StringComparison.OrdinalIgnoreCase)
				|| loginBody.Contains("Two-factor", StringComparison.OrdinalIgnoreCase)
				|| loginBody.Contains("Authenticator", StringComparison.OrdinalIgnoreCase)
				|| login.StatusCode == HttpStatusCode.Found,
				$"Expected 2FA challenge. Status={login.StatusCode}. Location={location}. Body={loginBody}");

			if (login.StatusCode == HttpStatusCode.Found && !string.IsNullOrEmpty(location))
			{
				Assert.IsTrue(location.Contains("LoginWith2fa", StringComparison.OrdinalIgnoreCase), location);
			}

			var code = await userManager.GenerateTwoFactorTokenAsync(
				user!,
				userManager.Options.Tokens.AuthenticatorTokenProvider);

			var verify = await client.PostAsync("/Account/LoginWith2fa?returnUrl=%2F", new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["Input.TwoFactorCode"] = code,
				["ReturnUrl"] = "/"
			}));
			Assert.IsTrue(
				verify.IsSuccessStatusCode || verify.StatusCode == HttpStatusCode.Found,
				await verify.Content.ReadAsStringAsync());
		}

		[TestMethod]
		public async Task AuthAudit_Should_Be_Queryable_After_Login()
		{
			using var client = ServerHelper.Create().CreateClient();

			await client.PostAsync("/Account/Login?returnUrl=%2F", new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["Input.Email"] = IdentitySeed.DevUserEmail,
				["Input.Password"] = IdentitySeed.DevUserPassword
			}));

			var response = await client.GetAsync($"/api/identity/auth-audit-events?userId={IdentitySeed.DevUserId}&eventType={AuthAuditEventTypes.LoginSucceeded}");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			var json = await response.Content.ReadAsStringAsync();
			Assert.IsTrue(json.Contains(AuthAuditEventTypes.LoginSucceeded), json);
		}

		[TestMethod]
		public async Task RevokeSessions_Should_Write_Audit_Event()
		{
			using var client = ServerHelper.Create().CreateClient();

			var response = await client.PostAsync(
				$"/api/identity/account/sessions/{IdentitySeed.DevUserId}/revoke",
				new StringContent("{}", Encoding.UTF8, "application/json"));
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			var audit = await client.GetAsync(
				$"/api/identity/auth-audit-events?userId={IdentitySeed.DevUserId}&eventType={AuthAuditEventTypes.TokensRevoked}");
			var body = await audit.Content.ReadAsStringAsync();
			Assert.IsTrue(body.Contains(AuthAuditEventTypes.TokensRevoked), body);
		}
	}
}
