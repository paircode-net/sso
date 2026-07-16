using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;

namespace SSO.Infrastructures.Services.Identity
{
	public sealed class ClaimPolicyVersionProvider : IClaimPolicyVersionProvider
	{
		private readonly IIdentityDbContextReader _reader;

		public ClaimPolicyVersionProvider(IIdentityDbContextReader reader)
		{
			_reader = reader;
		}

		public async Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
		{
			var definitionTicks = await _reader.Query<ClaimDefinition>().AsNoTracking()
				.Select(x => x.UpdatedAt ?? x.CreatedAt)
				.ToListAsync(cancellationToken);
			var roleClaimTicks = await _reader.Query<RoleClaim>().AsNoTracking()
				.Select(x => x.UpdatedAt ?? x.CreatedAt)
				.ToListAsync(cancellationToken);
			var assignmentTicks = await _reader.Query<UserClaimAssignment>().AsNoTracking()
				.Select(x => x.UpdatedAt ?? x.CreatedAt)
				.ToListAsync(cancellationToken);

			long MaxTicks(System.Collections.Generic.List<DateTime> values)
				=> values.Count == 0 ? 0 : values.Max(v => v.ToUniversalTime().Ticks);

			var max = Math.Max(
				MaxTicks(definitionTicks),
				Math.Max(MaxTicks(roleClaimTicks), MaxTicks(assignmentTicks)));

			return max <= 0 ? "0" : max.ToString();
		}
	}
}
