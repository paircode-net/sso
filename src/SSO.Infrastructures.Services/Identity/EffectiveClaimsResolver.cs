using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;

namespace SSO.Infrastructures.Services.Identity
{
	/// <summary>
	/// Resolves typed claims: RoleClaim first, then UserClaimAssignment overrides (F00008-D5).
	/// Context matching mirrors EffectivePermissionsResolver (ADR-004 exact branch).
	/// </summary>
	public sealed class EffectiveClaimsResolver : IEffectiveClaimsResolver
	{
		private readonly IIdentityDbContextReader _reader;

		public EffectiveClaimsResolver(IIdentityDbContextReader reader)
		{
			_reader = reader;
		}

		public async Task<IReadOnlyDictionary<string, string>> ResolveAsync(
			Guid userId,
			Guid? organizationId,
			Guid? branchId,
			string? clientId,
			CancellationToken cancellationToken = default)
		{
			var productId = await ResolveProductIdAsync(clientId, cancellationToken);
			if (productId is null)
			{
				return new Dictionary<string, string>();
			}

			var definitions = await _reader.Query<ClaimDefinition>().AsNoTracking()
				.Where(x => !x.IsDeleted
					&& x.IsEnabled
					&& (x.ProductId == null || x.ProductId == productId.Value))
				.ToListAsync(cancellationToken);

			if (definitions.Count == 0)
			{
				return new Dictionary<string, string>();
			}

			var definitionById = definitions.ToDictionary(x => x.Id);
			var definitionIds = definitionById.Keys.ToList();

			var roleIds = await ResolveRoleIdsAsync(userId, organizationId, branchId, productId.Value, cancellationToken);
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (roleIds.Count > 0)
			{
				var roleIdList = roleIds.ToList();
				var roleClaims = await _reader.Query<RoleClaim>().AsNoTracking()
					.Where(x => !x.IsDeleted
						&& roleIdList.Contains(x.RoleId)
						&& definitionIds.Contains(x.ClaimDefinitionId))
					.ToListAsync(cancellationToken);

				foreach (var rc in roleClaims)
				{
					if (definitionById.TryGetValue(rc.ClaimDefinitionId, out var def))
					{
						result[def.Code] = rc.Value;
					}
				}
			}

			var userAssignments = await _reader.Query<UserClaimAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted
					&& x.UserId == userId
					&& x.ProductId == productId.Value
					&& definitionIds.Contains(x.ClaimDefinitionId))
				.ToListAsync(cancellationToken);

			foreach (var assignment in userAssignments)
			{
				if (!definitionById.TryGetValue(assignment.ClaimDefinitionId, out var def))
				{
					continue;
				}

				if (assignment.OrganizationId == null)
				{
					if (assignment.BranchId == null)
					{
						result[def.Code] = assignment.Value;
					}

					continue;
				}

				if (organizationId is not Guid orgId || assignment.OrganizationId != orgId)
				{
					continue;
				}

				var hasMembership = await _reader.Query<Membership>().AsNoTracking()
					.AnyAsync(
						x => !x.IsDeleted && x.UserId == userId && x.OrganizationId == orgId,
						cancellationToken);
				if (!hasMembership)
				{
					continue;
				}

				if (assignment.BranchId == null
					|| (branchId != null && assignment.BranchId == branchId.Value))
				{
					result[def.Code] = assignment.Value;
				}
			}

			return result
				.OrderBy(x => x.Key)
				.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
		}

		private async Task<HashSet<Guid>> ResolveRoleIdsAsync(
			Guid userId,
			Guid? organizationId,
			Guid? branchId,
			Guid productId,
			CancellationToken cancellationToken)
		{
			var roleIds = new HashSet<Guid>();

			var platformRoleIds = await _reader.Query<UserRoleAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted
					&& x.UserId == userId
					&& x.OrganizationId == null
					&& x.ProductId == productId
					&& x.BranchId == null)
				.Select(x => x.RoleId)
				.ToListAsync(cancellationToken);

			foreach (var id in platformRoleIds)
			{
				roleIds.Add(id);
			}

			if (organizationId is Guid orgId)
			{
				var hasMembership = await _reader.Query<Membership>().AsNoTracking()
					.AnyAsync(
						x => !x.IsDeleted && x.UserId == userId && x.OrganizationId == orgId,
						cancellationToken);

				if (hasMembership)
				{
					var tenantRoleIds = await _reader.Query<UserRoleAssignment>().AsNoTracking()
						.Where(x => !x.IsDeleted
							&& x.UserId == userId
							&& x.OrganizationId == orgId
							&& x.ProductId == productId
							&& (x.BranchId == null || (branchId != null && x.BranchId == branchId.Value)))
						.Select(x => x.RoleId)
						.ToListAsync(cancellationToken);

					foreach (var id in tenantRoleIds)
					{
						roleIds.Add(id);
					}
				}
			}

			return roleIds;
		}

		private async Task<Guid?> ResolveProductIdAsync(string? clientId, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				return null;
			}

			return await _reader.Query<ClientProductBinding>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.ClientId == clientId)
				.Select(x => (Guid?)x.ProductId)
				.FirstOrDefaultAsync(cancellationToken);
		}
	}
}
