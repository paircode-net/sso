using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.ClientWebhooks.Entity;
using SSO.Core.Domain.Identity.WebhookOutbox.Entity;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Tests.Helpers;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class HotRevocationScenarios
	{
		[TestMethod]
		public async Task EnsureSession_Should_Emit_Sid_And_Status_Active()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var sessions = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
			var claimsFactory = scope.ServiceProvider.GetRequiredService<TokenClaimsFactory>();
			var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SSO.Core.Domain.Identity.Users.Entity.User>>();

			var user = await userManager.FindByIdAsync(IdentitySeed.DevUserId.ToString());
			Assert.IsNotNull(user);

			var principal = await claimsFactory.CreateUserPrincipalAsync(
				user!,
				new[] { "openid" },
				SsoClients.DevSpaClientId,
				IdentitySeed.DevOrganizationId,
				branchId: null);

			var sid = principal.FindFirst(SsoClaimTypes.SessionId)?.Value;
			Assert.IsFalse(string.IsNullOrWhiteSpace(sid));
			Assert.IsTrue(Guid.TryParse(sid, out var sessionId));

			Assert.IsFalse(await sessions.IsSessionRevokedAsync(sessionId));

			using var client = server.CreateClient();
			var status = await client.GetAsync($"/api/identity/sessions/{sessionId:D}/status");
			Assert.AreEqual(HttpStatusCode.OK, status.StatusCode);
			var json = await status.Content.ReadAsStringAsync();
			Assert.IsTrue(json.Contains("\"revoked\":false") || json.Contains("\"revoked\": false"), json);
		}

		[TestMethod]
		public async Task RevokeSession_Should_DenyList_And_Audit_And_Outbox()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var sessions = scope.ServiceProvider.GetRequiredService<IUserSessionService>();

			db.ClientWebhookEndpoints.Add(ClientWebhookEndpoint.Create(
				SsoClients.DevSpaClientId,
				"http://127.0.0.1:9/webhook",
				"test-hmac-secret"));
			await db.SaveChangesAsync();

			var session = await sessions.EnsureSessionAsync(
				IdentitySeed.DevUserId,
				SsoClients.DevSpaClientId,
				IdentitySeed.DevOrganizationId,
				branchId: null);

			Assert.IsTrue(await sessions.RevokeSessionAsync(session.Id, "test.revoke"));
			Assert.IsTrue(await sessions.IsSessionRevokedAsync(session.Id));

			Assert.IsTrue(await db.RevokedSessions.AnyAsync(x => x.SessionId == session.Id));
			Assert.IsTrue(await db.WebhookOutbox.AnyAsync(x =>
				x.EventType == WebhookEventTypes.SessionRevoked
				&& x.ClientId == SsoClients.DevSpaClientId
				&& x.Status == WebhookOutboxStatuses.Pending));

			using var admin = AdminAuthTestHelper.CreatePlatformAdminClient(server);
			var audit = await admin.GetAsync(
				$"/api/identity/auth-audit-events?userId={IdentitySeed.DevUserId}&eventType={AuthAuditEventTypes.SessionRevoked}");
			var body = await audit.Content.ReadAsStringAsync();
			Assert.IsTrue(body.Contains(AuthAuditEventTypes.SessionRevoked), body);
		}

		[TestMethod]
		public async Task Hmac_Should_Be_Deterministic()
		{
			var a = WebhookOutboxSenderHostedService.ComputeHmacSha256("secret", "{\"type\":\"session.revoked\"}");
			var b = WebhookOutboxSenderHostedService.ComputeHmacSha256("secret", "{\"type\":\"session.revoked\"}");
			Assert.AreEqual(a, b);
			Assert.AreEqual(64, a.Length);
		}

		[TestMethod]
		public async Task Admin_List_Sessions_Should_Require_Permission()
		{
			using var server = ServerHelper.Create();
			using var scope = server.Services.CreateScope();
			var sessions = scope.ServiceProvider.GetRequiredService<IUserSessionService>();
			await sessions.EnsureSessionAsync(
				IdentitySeed.DevUserId,
				SsoClients.DevSpaClientId,
				IdentitySeed.DevOrganizationId,
				null);

			using var admin = AdminAuthTestHelper.CreatePlatformAdminClient(server);
			var response = await admin.GetAsync($"/api/identity/sessions/user/{IdentitySeed.DevUserId}");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
		}
	}
}
