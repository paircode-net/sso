using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	public interface IEffectivePermissionsResolver
	{
		Task<IReadOnlyList<string>> ResolveAsync(
			Guid userId,
			Guid? organizationId,
			Guid? branchId,
			string? clientId,
			CancellationToken cancellationToken = default);
	}
}
