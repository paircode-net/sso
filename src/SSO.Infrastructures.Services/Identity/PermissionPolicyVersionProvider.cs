using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;

namespace SSO.Infrastructures.Services.Identity
{
	public sealed class PermissionPolicyVersionProvider : IPermissionPolicyVersionProvider
	{
		private readonly IIdentityDbContextReader _reader;

		public PermissionPolicyVersionProvider(IIdentityDbContextReader reader)
		{
			_reader = reader;
		}

		public async Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
		{
			var permissionTicks = await _reader.Query<Permission>().AsNoTracking()
				.Select(x => x.UpdatedAt ?? x.CreatedAt)
				.ToListAsync(cancellationToken);
			var rolePermissionTicks = await _reader.Query<RolePermission>().AsNoTracking()
				.Select(x => x.UpdatedAt ?? x.CreatedAt)
				.ToListAsync(cancellationToken);
			var assignmentTicks = await _reader.Query<UserRoleAssignment>().AsNoTracking()
				.Select(x => x.UpdatedAt ?? x.CreatedAt)
				.ToListAsync(cancellationToken);

			long MaxTicks(System.Collections.Generic.List<DateTime> values)
				=> values.Count == 0 ? 0 : values.Max(v => v.ToUniversalTime().Ticks);

			var max = Math.Max(
				MaxTicks(permissionTicks),
				Math.Max(MaxTicks(rolePermissionTicks), MaxTicks(assignmentTicks)));

			return max <= 0 ? "0" : max.ToString();
		}
	}
}
