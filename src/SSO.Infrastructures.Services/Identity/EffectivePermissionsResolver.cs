using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;

namespace SSO.Infrastructures.Services.Identity
{
	/// <summary>
	/// Resolves effective permission codes for User × Organization × Branch × Product,
	/// plus platform-scoped assignments (OrganizationId null — F00002-D2).
	/// Branch match is exact (ADR-004): no parent→child inheritance.
	/// Org-wide assignments (BranchId null) apply in every branch of that org+product.
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
					var tenantRoleIds = await _reader
						.Query<UserRoleAssignment>()
						.AsNoTracking()
						.Where(x => !x.IsDeleted
							&& x.UserId == userId
							&& x.OrganizationId == orgId
							&& x.ProductId == productId.Value
							&& (x.BranchId == null || (branchId != null && x.BranchId == branchId.Value)))
						.Select(x => x.RoleId)
						.ToListAsync(cancellationToken);

					foreach (var roleId in tenantRoleIds)
					{
						roleIds.Add(roleId);
					}
				}
			}

			if (roleIds.Count == 0)
			{
				return Array.Empty<string>();
			}

			var permissionIds = await _reader
				.Query<RolePermission>()
				.AsNoTracking()
				.Where(x => !x.IsDeleted && roleIds.Contains(x.RoleId))
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
