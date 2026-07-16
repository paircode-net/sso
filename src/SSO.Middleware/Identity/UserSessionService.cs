using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.RevokedSessions.Entity;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using SSO.Core.Domain.Identity.WebhookOutbox.Entity;
using SSO.Infrastructures.Data.Identity;

namespace SSO.Middleware.Identity
{
	public sealed class UserSessionService : IUserSessionService
	{
		/// <summary>Deny-list TTL ≥ access token lifetime (15m) with margin.</summary>
		public static readonly TimeSpan DenyListTtl = TimeSpan.FromMinutes(30);

		private readonly IOpenIddictTokenManager _tokenManager;
		private readonly IAuthAuditService _auditService;
		private readonly IdentityDbContext _db;

		public UserSessionService(
			IOpenIddictTokenManager tokenManager,
			IAuthAuditService auditService,
			IdentityDbContext db)
		{
			_tokenManager = tokenManager;
			_auditService = auditService;
			_db = db;
		}

		public async Task<UserSession> EnsureSessionAsync(
			Guid userId,
			string? clientId,
			Guid? organizationId,
			Guid? branchId,
			Guid? existingSessionId = null,
			CancellationToken cancellationToken = default)
		{
			clientId ??= string.Empty;

			if (existingSessionId is Guid sid)
			{
				var existing = await _db.UserSessions
					.FirstOrDefaultAsync(x => x.Id == sid && x.UserId == userId && !x.IsDeleted, cancellationToken);

				if (existing is not null && existing.IsActive)
				{
					existing.OrganizationId = organizationId ?? existing.OrganizationId;
					existing.BranchId = branchId;
					if (!string.IsNullOrWhiteSpace(clientId))
					{
						existing.ClientId = clientId;
					}

					existing.Touch();
					await _db.SaveChangesAsync(cancellationToken);
					return existing;
				}
			}

			var session = UserSession.Create(userId, clientId, organizationId, branchId);
			_db.UserSessions.Add(session);
			await _db.SaveChangesAsync(cancellationToken);
			return session;
		}

		public async Task<IReadOnlyList<UserSession>> ListForUserAsync(
			Guid userId,
			bool includeRevoked = false,
			CancellationToken cancellationToken = default)
		{
			var query = _db.UserSessions.AsNoTracking()
				.Where(x => x.UserId == userId && !x.IsDeleted);

			if (!includeRevoked)
			{
				query = query.Where(x => x.RevokedAt == null);
			}

			return await query
				.OrderByDescending(x => x.LastSeenAt)
				.Take(100)
				.ToListAsync(cancellationToken);
		}

		public async Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken = default)
		{
			var now = DateTime.UtcNow;
			var inDenyList = await _db.RevokedSessions.AsNoTracking()
				.AnyAsync(x => x.SessionId == sessionId && x.ExpiresAt > now, cancellationToken);
			if (inDenyList)
			{
				return true;
			}

			var session = await _db.UserSessions.AsNoTracking()
				.FirstOrDefaultAsync(x => x.Id == sessionId && !x.IsDeleted, cancellationToken);
			return session is null || session.RevokedAt is not null;
		}

		public async Task<bool> RevokeSessionAsync(
			Guid sessionId,
			string? reason = null,
			CancellationToken cancellationToken = default)
		{
			var session = await _db.UserSessions
				.FirstOrDefaultAsync(x => x.Id == sessionId && !x.IsDeleted, cancellationToken);
			if (session is null)
			{
				return false;
			}

			if (session.IsActive)
			{
				await RevokeInternalAsync(session, reason ?? "session.revoke", revokeOpenIddictForUser: true, cancellationToken);
			}

			return true;
		}

		public async Task<int> RevokeAllForUserAsync(
			Guid userId,
			string reason = null,
			CancellationToken cancellationToken = default)
		{
			reason ??= "tokens.revoked";
			var sessions = await _db.UserSessions
				.Where(x => x.UserId == userId && !x.IsDeleted && x.RevokedAt == null)
				.ToListAsync(cancellationToken);

			foreach (var session in sessions)
			{
				await RevokeInternalAsync(session, reason, revokeOpenIddictForUser: false, cancellationToken);
			}

			var revokedTokens = 0;
			var subject = userId.ToString("D");
			await foreach (var token in _tokenManager.FindBySubjectAsync(subject, cancellationToken))
			{
				await _tokenManager.TryRevokeAsync(token, cancellationToken);
				revokedTokens++;
			}

			await _auditService.WriteAsync(
				AuthAuditEvent.Create(
					AuthAuditEventTypes.TokensRevoked,
					AuthAuditOutcomes.Success,
					userId: userId,
					detail: $"{reason};sessions={sessions.Count};tokens={revokedTokens}"),
				cancellationToken);

			return sessions.Count > 0 ? sessions.Count : revokedTokens;
		}

		private async Task RevokeInternalAsync(
			UserSession session,
			string reason,
			bool revokeOpenIddictForUser,
			CancellationToken cancellationToken)
		{
			session.Revoke(reason);

			var alreadyDenied = await _db.RevokedSessions
				.AnyAsync(x => x.SessionId == session.Id, cancellationToken);
			if (!alreadyDenied)
			{
				_db.RevokedSessions.Add(RevokedSession.Create(
					session.Id,
					session.UserId,
					session.ClientId,
					reason,
					DenyListTtl));
			}

			await EnqueueSessionRevokedWebhooksAsync(session, reason, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);

			await _auditService.WriteAsync(
				AuthAuditEvent.Create(
					AuthAuditEventTypes.SessionRevoked,
					AuthAuditOutcomes.Success,
					userId: session.UserId,
					clientId: session.ClientId,
					detail: $"sid={session.Id:D};reason={reason}"),
				cancellationToken);

			if (revokeOpenIddictForUser)
			{
				var subject = session.UserId.ToString("D");
				await foreach (var token in _tokenManager.FindBySubjectAsync(subject, cancellationToken))
				{
					await _tokenManager.TryRevokeAsync(token, cancellationToken);
				}
			}
		}

		private async Task EnqueueSessionRevokedWebhooksAsync(
			UserSession session,
			string reason,
			CancellationToken cancellationToken)
		{
			var endpoints = await _db.ClientWebhookEndpoints.AsNoTracking()
				.Where(x => !x.IsDeleted && x.IsEnabled)
				.ToListAsync(cancellationToken);

			if (endpoints.Count == 0)
			{
				return;
			}

			var payload = JsonSerializer.Serialize(new
			{
				type = WebhookEventTypes.SessionRevoked,
				session_id = session.Id.ToString("D"),
				user_id = session.UserId.ToString("D"),
				client_id = session.ClientId,
				organization_id = session.OrganizationId?.ToString("D"),
				branch_id = session.BranchId?.ToString("D"),
				revoked_at = DateTime.UtcNow.ToString("O"),
				reason
			});

			foreach (var endpoint in endpoints)
			{
				// Notify subscribers; filter by matching client when configured for a specific client.
				if (!string.IsNullOrWhiteSpace(session.ClientId)
					&& !string.Equals(endpoint.ClientId, session.ClientId, StringComparison.Ordinal)
					&& !string.Equals(endpoint.ClientId, "*", StringComparison.Ordinal))
				{
					continue;
				}

				_db.WebhookOutbox.Add(WebhookOutboxMessage.Create(
					WebhookEventTypes.SessionRevoked,
					endpoint.ClientId,
					payload));
			}
		}
	}
}
