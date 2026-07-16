using System;
using BAYSOFT.Abstractions.Core.Domain.Entities;

namespace SSO.Core.Domain.Identity.RevokedSessions.Entity
{
	/// <summary>SQL deny-list for hot access-token revocation (feature 00005).</summary>
	public sealed class RevokedSession : DomainEntity<Guid>
	{
		/// <summary>Same as UserSession.Id / JWT sid.</summary>
		public Guid SessionId { get; set; }
		public DateTime RevokedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public string? Reason { get; set; }
		public Guid? UserId { get; set; }
		public string? ClientId { get; set; }

		public RevokedSession()
		{
		}

		public static RevokedSession Create(
			Guid sessionId,
			Guid? userId,
			string? clientId,
			string? reason,
			TimeSpan denyListTtl)
		{
			var now = DateTime.UtcNow;
			return new RevokedSession
			{
				Id = Guid.NewGuid(),
				SessionId = sessionId,
				UserId = userId,
				ClientId = clientId,
				Reason = reason,
				RevokedAt = now,
				ExpiresAt = now.Add(denyListTtl)
			};
		}
	}
}
