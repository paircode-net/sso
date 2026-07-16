using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Branches;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Shared.Identity;

namespace SSO.Infrastructures.Services.Identity
{
	/// <summary>
	/// Resolves effective permission codes for User × Organization × Branch × Product.
	/// Default: exact branch match (ADR-004). Opt-in ancestor inheritance (ADR-008).
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
			var productId = await ResolveProductIdAsync(clientId, cancellationToken);
			if (productId is null)
			{
				return Array.Empty<string>();
			}

			var roleIds = new HashSet<Guid>();

			var platformRoleIds = await _reader
				.Query<UserRoleAssignment>()
				.AsNoTracking()
				.Where(x => !x.IsDeleted
					&& x.UserId == userId
					&& x.OrganizationId == null
					&& x.ProductId == productId.Value
					&& x.BranchId == null)
				.Select(x => x.RoleId)
				.ToListAsync(cancellationToken);

			foreach (var roleId in platformRoleIds)
			{
				roleIds.Add(roleId);
			}

			if (organizationId is Guid orgId)
			{
				var hasMembership = await _reader
					.Query<Membership>()
					.AsNoTracking()
					.AnyAsync(
						x => !x.IsDeleted
							&& x.UserId == userId
							&& x.OrganizationId == orgId,
						cancellationToken);

				if (hasMembership)
				{
					var org = await _reader.Query<Organization>().AsNoTracking()
						.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == orgId, cancellationToken);
					var inheritanceOn = BranchAuthzInheritancePolicies.IsEnabled(org?.BranchAuthzInheritance);

					IReadOnlyList<Guid> ancestorIds = Array.Empty<Guid>();
					if (inheritanceOn && branchId is Guid activeBranch)
					{
						var branches = await _reader.Query<Branch>().AsNoTracking()
							.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
							.ToListAsync(cancellationToken);
						ancestorIds = BranchAncestry.GetAncestorIds(branches, activeBranch);
					}

					var tenantAssignments = await _reader
						.Query<UserRoleAssignment>()
						.AsNoTracking()
						.Where(x => !x.IsDeleted
							&& x.UserId == userId
							&& x.OrganizationId == orgId
							&& x.ProductId == productId.Value)
						.ToListAsync(cancellationToken);

					foreach (var assignment in tenantAssignments)
					{
						if (assignment.BranchId == null)
						{
							roleIds.Add(assignment.RoleId);
							continue;
						}

						if (branchId != null && assignment.BranchId == branchId.Value)
						{
							roleIds.Add(assignment.RoleId);
							continue;
						}

						if (inheritanceOn
							&& assignment.Inheritable
							&& ancestorIds.Contains(assignment.BranchId.Value))
						{
							roleIds.Add(assignment.RoleId);
						}
					}
				}
			}

			if (roleIds.Count == 0)
			{
				return Array.Empty<string>();
			}

			var roleIdList = roleIds.ToList();
			var permissionIds = await _reader
				.Query<RolePermission>()
				.AsNoTracking()
				.Where(x => !x.IsDeleted && roleIdList.Contains(x.RoleId))
				.Select(x => x.PermissionId)
				.Distinct()
				.ToListAsync(cancellationToken);

			if (permissionIds.Count == 0)
			{
				return Array.Empty<string>();
			}

			return await _reader
				.Query<Permission>()
				.AsNoTracking()
				.Where(x => !x.IsDeleted && permissionIds.Contains(x.Id))
				.Select(x => x.Code)
				.Distinct()
				.OrderBy(x => x)
				.ToListAsync(cancellationToken);
		}

		private async Task<Guid?> ResolveProductIdAsync(string? clientId, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				return null;
			}

			return await _reader
				.Query<ClientProductBinding>()
				.AsNoTracking()
				.Where(x => !x.IsDeleted && x.ClientId == clientId)
				.Select(x => (Guid?)x.ProductId)
				.FirstOrDefaultAsync(cancellationToken);
		}
	}
}
