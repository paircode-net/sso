using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Memberships.Entity;

namespace SSO.Infrastructures.Services.Identity
{
	/// <summary>
	/// Phase 2 stub: grants a basic access permission when a membership exists.
	/// Full Role→Permission resolution lands in Phase 3.
	/// </summary>
	public sealed class EffectivePermissionsResolver : IEffectivePermissionsResolver
	{
		private readonly IIdentityDbContextReader _reader;

		public EffectivePermissionsResolver(IIdentityDbContextReader reader)
		{
			_reader = reader;
		}

		public async Task<IReadOnlyList<string>> ResolveAsync(
			Guid userId,
			Guid? organizationId,
			Guid? branchId,
			string? clientId,
			CancellationToken cancellationToken = default)
		{
			if (organizationId is null)
			{
				return Array.Empty<string>();
			}

			var hasMembership = await _reader
				.Query<Membership>()
				.AsNoTracking()
				.AnyAsync(
					x => !x.IsDeleted
						&& x.UserId == userId
						&& x.OrganizationId == organizationId.Value,
					cancellationToken);

			if (!hasMembership)
			{
				return Array.Empty<string>();
			}

			// Stub until Permission aggregate (Phase 3 / 5)
			return new[] { "sso.access" };
		}
	}
}
