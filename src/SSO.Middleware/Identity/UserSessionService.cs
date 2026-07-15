using System;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;

namespace SSO.Middleware.Identity
{
	public sealed class UserSessionService : IUserSessionService
	{
		private readonly IOpenIddictTokenManager _tokenManager;
		private readonly IAuthAuditService _auditService;

		public UserSessionService(IOpenIddictTokenManager tokenManager, IAuthAuditService auditService)
		{
			_tokenManager = tokenManager;
			_auditService = auditService;
		}

		public async Task<int> RevokeAllForUserAsync(Guid userId, string reason = null, CancellationToken cancellationToken = default)
		{
			var subject = userId.ToString("D");
			var revoked = 0;

			await foreach (var token in _tokenManager.FindBySubjectAsync(subject, cancellationToken))
			{
				await _tokenManager.TryRevokeAsync(token, cancellationToken);
				revoked++;
			}

			await _auditService.WriteAsync(
				AuthAuditEvent.Create(
					AuthAuditEventTypes.TokensRevoked,
					AuthAuditOutcomes.Success,
					userId: userId,
					detail: reason ?? $"revoked={revoked}"),
				cancellationToken);

			return revoked;
		}
	}
}
