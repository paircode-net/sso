using System;
using System.Threading;
using System.Threading.Tasks;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	public interface IAuthAuditService
	{
		Task WriteAsync(AuthAuditEvent auditEvent, CancellationToken cancellationToken = default);
	}

	public interface IUserSessionService
	{
		Task<int> RevokeAllForUserAsync(Guid userId, string reason = null, CancellationToken cancellationToken = default);
	}
}
