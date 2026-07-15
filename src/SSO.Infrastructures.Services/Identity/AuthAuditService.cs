using System.Threading;
using System.Threading.Tasks;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;

namespace SSO.Infrastructures.Services.Identity
{
	public sealed class AuthAuditService : IAuthAuditService
	{
		private readonly IIdentityDbContextWriter _writer;

		public AuthAuditService(IIdentityDbContextWriter writer)
		{
			_writer = writer;
		}

		public async Task WriteAsync(AuthAuditEvent auditEvent, CancellationToken cancellationToken = default)
		{
			await _writer.AddAsync(auditEvent);
			await _writer.CommitAsync(cancellationToken);
		}
	}
}
