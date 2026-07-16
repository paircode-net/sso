using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.UserSessions.Entity;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	public interface IAuthAuditService
	{
		Task WriteAsync(AuthAuditEvent auditEvent, CancellationToken cancellationToken = default);
	}

	public interface IUserSessionService
	{
		Task<UserSession> EnsureSessionAsync(
			Guid userId,
			string? clientId,
			Guid? organizationId,
			Guid? branchId,
			Guid? existingSessionId = null,
			CancellationToken cancellationToken = default);

		Task<IReadOnlyList<UserSession>> ListForUserAsync(
			Guid userId,
			bool includeRevoked = false,
			CancellationToken cancellationToken = default);

		Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken = default);

		Task<bool> RevokeSessionAsync(
			Guid sessionId,
			string? reason = null,
			CancellationToken cancellationToken = default);

		Task<int> RevokeAllForUserAsync(
			Guid userId,
			string reason = null,
			CancellationToken cancellationToken = default);
	}
}
