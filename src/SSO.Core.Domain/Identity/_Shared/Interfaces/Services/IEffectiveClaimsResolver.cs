using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	/// <summary>Effective typed claims for User × Org × Branch × Product (feature 00008).</summary>
	public interface IEffectiveClaimsResolver
	{
		/// <summary>Returns code → value (JWT type is sso_c_{code}). User overrides Role (F00008-D5).</summary>
		Task<IReadOnlyDictionary<string, string>> ResolveAsync(
			Guid userId,
			Guid? organizationId,
			Guid? branchId,
			string? clientId,
			CancellationToken cancellationToken = default);
	}

	public interface IClaimPolicyVersionProvider
	{
		Task<string> GetVersionAsync(CancellationToken cancellationToken = default);
	}
}
