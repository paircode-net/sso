using System;
using BAYSOFT.Abstractions.Core.Domain.Entities;

namespace SSO.Core.Domain.Identity.AuthAuditEvents.Entity
{
	/// <summary>Append-only AuthN/AuthZ audit trail (Phase 4).</summary>
	public sealed class AuthAuditEvent : DomainEntity<Guid>
	{
		public DateTime CreatedAt { get; set; }
		public string EventType { get; set; }
		public string Outcome { get; set; }
		public Guid? UserId { get; set; }
		public string Email { get; set; }
		public string ClientId { get; set; }
		public string IpAddress { get; set; }
		public string Detail { get; set; }

		public AuthAuditEvent()
		{
		}

		public static AuthAuditEvent Create(
			string eventType,
			string outcome,
			Guid? userId = null,
			string email = null,
			string clientId = null,
			string ipAddress = null,
			string detail = null)
		{
			return new AuthAuditEvent
			{
				Id = Guid.NewGuid(),
				CreatedAt = DateTime.UtcNow,
				EventType = eventType,
				Outcome = outcome,
				UserId = userId,
				Email = email,
				ClientId = clientId,
				IpAddress = ipAddress,
				Detail = detail
			};
		}
	}

	public static class AuthAuditEventTypes
	{
		public const string LoginSucceeded = "login.succeeded";
		public const string LoginFailed = "login.failed";
		public const string LoginLockedOut = "login.locked_out";
		public const string LoginRequiresTwoFactor = "login.requires_2fa";
		public const string TwoFactorSucceeded = "login.2fa.succeeded";
		public const string TwoFactorFailed = "login.2fa.failed";
		public const string EmailConfirmRequested = "email.confirm.requested";
		public const string EmailConfirmed = "email.confirmed";
		public const string PasswordResetRequested = "password.reset.requested";
		public const string PasswordResetCompleted = "password.reset.completed";
		public const string TwoFactorEnabled = "2fa.enabled";
		public const string Logout = "logout";
		public const string TokensRevoked = "tokens.revoked";
		public const string SessionRevoked = "session.revoked";
	}

	public static class AuthAuditOutcomes
	{
		public const string Success = "success";
		public const string Failure = "failure";
	}
}
