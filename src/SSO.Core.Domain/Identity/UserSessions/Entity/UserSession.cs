using System;
using SSO.Core.Domain.Identity._Shared;

namespace SSO.Core.Domain.Identity.UserSessions.Entity
{
	/// <summary>Product-facing SSO session (feature 00005). Id is the JWT <c>sid</c> claim.</summary>
	public sealed class UserSession : IdentityAuditableEntity
	{
		public Guid UserId { get; set; }
		public string ClientId { get; set; } = string.Empty;
		public Guid? OrganizationId { get; set; }
		public Guid? BranchId { get; set; }
		public DateTime LastSeenAt { get; set; }
		public DateTime? RevokedAt { get; set; }
		public string? RevokeReason { get; set; }

		public bool IsActive => RevokedAt is null && !IsDeleted;

		public UserSession()
		{
		}

		public static UserSession Create(
			Guid userId,
			string clientId,
			Guid? organizationId,
			Guid? branchId)
		{
			var now = DateTime.UtcNow;
			var session = new UserSession
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				ClientId = clientId ?? string.Empty,
				OrganizationId = organizationId,
				BranchId = branchId,
				LastSeenAt = now
			};
			session.MarkCreated();
			return session;
		}

		public void Touch()
		{
			LastSeenAt = DateTime.UtcNow;
			TouchUpdated();
		}

		public void Revoke(string? reason)
		{
			if (RevokedAt is not null)
			{
				return;
			}

			RevokedAt = DateTime.UtcNow;
			RevokeReason = reason;
			TouchUpdated();
		}
	}
}
