using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class ObservabilityScenarios
	{
		[TestMethod]
		public async Task Health_Live_Should_Return_Ok()
		{
			using var server = ServerHelper.Create();
			using var client = server.CreateClient();

			var response = await client.GetAsync("/health/live");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}

		[TestMethod]
		public async Task Health_Ready_Should_Return_Ok_When_Db_Available()
		{
			using var server = ServerHelper.Create();
			using var client = server.CreateClient();

			var response = await client.GetAsync("/health/ready");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			var body = await response.Content.ReadAsStringAsync();
			StringAssert.Contains(body, "Healthy");
		}

		[TestMethod]
		public void LogRedaction_Should_Strip_Bearer_And_Password()
		{
			var message = LogRedaction.RedactMessage(
				"Authorization: Bearer super-secret-token password payload {\"password\":\"ChangeMe!123\",\"user\":\"a\"}");

			StringAssert.Contains(message, LogRedaction.Redacted);
			Assert.IsFalse(message.Contains("super-secret-token", System.StringComparison.Ordinal));
			Assert.IsFalse(message.Contains("ChangeMe!123", System.StringComparison.Ordinal));
		}

		[TestMethod]
		public void LogRedaction_Should_Mark_Sensitive_Headers()
		{
			Assert.IsTrue(LogRedaction.IsSensitiveHeader("Authorization"));
			Assert.IsTrue(LogRedaction.IsSensitiveHeader("Cookie"));
			Assert.IsFalse(LogRedaction.IsSensitiveHeader("User-Agent"));
			Assert.AreEqual(LogRedaction.Redacted, LogRedaction.RedactHeaderValue("Authorization", "Bearer abc"));
		}

		[TestMethod]
		public void Auth_Metrics_Should_Record_Without_Throwing()
		{
			SsoAuthMetrics.RecordLoginSuccess();
			SsoAuthMetrics.RecordLoginFailure("invalid_password");
			SsoAuthMetrics.RecordTokenIssued("authorization_code");
			SsoAuthMetrics.RecordSwitchContextSuccess();
			SsoAuthMetrics.RecordSwitchContextFailure("no_membership");
			SsoAuthMetrics.RecordRateLimited("/Account/Login");
			SsoAuthMetrics.RecordJwtShape(3, 512);
		}

		[TestMethod]
		public async Task Login_Failure_Should_Not_Echo_Password_In_Response()
		{
			using var server = ServerHelper.Create();
			using var client = server.CreateClient();

			var form = new MultipartFormDataContent
			{
				{ new StringContent("nobody@sso.local"), "Input.Email" },
				{ new StringContent("SecretPasswordShouldNotLeak"), "Input.Password" }
			};

			var response = await client.PostAsync("/Account/Login", form);
			var body = await response.Content.ReadAsStringAsync();

			Assert.IsFalse(
				body.Contains("SecretPasswordShouldNotLeak", System.StringComparison.Ordinal),
				"Login response must not echo the submitted password.");
		}
	}
}
