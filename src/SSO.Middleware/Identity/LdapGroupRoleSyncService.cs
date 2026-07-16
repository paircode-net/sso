using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Infrastructures.Data.Identity;

namespace SSO.Middleware.Identity
{
	public sealed class LdapGroupRoleSyncService
	{
		private readonly IdentityDbContext _db;

		public LdapGroupRoleSyncService(IdentityDbContext db)
		{
			_db = db;
		}

		public async Task<int> SyncAsync(
			Guid userId,
			Guid organizationId,
			IReadOnlyList<string> memberOfGroups,
			CancellationToken cancellationToken = default)
		{
			if (memberOfGroups.Count == 0)
			{
				return 0;
			}

			var maps = await _db.LdapGroupRoleMaps
				.AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == organizationId)
				.ToListAsync(cancellationToken);

			if (maps.Count == 0)
			{
				return 0;
			}

			var matched = maps.Where(m => memberOfGroups.Any(g => GroupMatches(g, m.GroupIdentifier))).ToList();
			var added = 0;

			foreach (var map in matched)
			{
				var exists = await _db.UserRoleAssignments.AnyAsync(x =>
					!x.IsDeleted
					&& x.UserId == userId
					&& x.RoleId == map.RoleId
					&& x.OrganizationId == organizationId
					&& x.ProductId == map.ProductId
					&& x.BranchId == map.BranchId,
					cancellationToken);

				if (exists)
				{
					continue;
				}

				var assignment = new UserRoleAssignment
				{
					Id = Guid.NewGuid(),
					UserId = userId,
					RoleId = map.RoleId,
					OrganizationId = organizationId,
					ProductId = map.ProductId,
					BranchId = map.BranchId
				};
				assignment.MarkCreated();
				_db.UserRoleAssignments.Add(assignment);
				added++;
			}

			if (added > 0)
			{
				await _db.SaveChangesAsync(cancellationToken);
			}

			return added;
		}

		public static bool GroupMatches(string memberOf, string configured)
		{
			if (string.IsNullOrWhiteSpace(memberOf) || string.IsNullOrWhiteSpace(configured))
			{
				return false;
			}

			if (string.Equals(memberOf.Trim(), configured.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			// CN=Foo,OU=... vs configured "Foo" or "CN=Foo"
			var cn = ExtractCn(memberOf);
			var configuredCn = ExtractCn(configured) ?? configured.Trim();
			return cn is not null
				&& string.Equals(cn, configuredCn, StringComparison.OrdinalIgnoreCase);
		}

		private static string? ExtractCn(string value)
		{
			var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var cnPart = parts.FirstOrDefault(p => p.StartsWith("CN=", StringComparison.OrdinalIgnoreCase));
			return cnPart is null ? null : cnPart[3..];
		}
	}
}
