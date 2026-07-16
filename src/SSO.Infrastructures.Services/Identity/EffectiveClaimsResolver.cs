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
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Shared.Identity;

namespace SSO.Infrastructures.Services.Identity
{
	/// <summary>
	/// Typed claims: RoleClaim then UserClaimAssignment override (F00008-D5).
	/// Branch inheritance (ADR-008): active wins; missing codes filled from nearest inheritable ancestor (F00009-D3).
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
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			// Platform-scoped user claims
			var platformClaims = await _reader.Query<UserClaimAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted
					&& x.UserId == userId
					&& x.ProductId == productId.Value
					&& x.OrganizationId == null
					&& x.BranchId == null
					&& definitionIds.Contains(x.ClaimDefinitionId))
				.ToListAsync(cancellationToken);
			ApplyUserOverrides(result, definitionById, platformClaims);

			if (organizationId is not Guid orgId)
			{
				return Sort(result);
			}

			var hasMembership = await _reader.Query<Membership>().AsNoTracking()
				.AnyAsync(x => !x.IsDeleted && x.UserId == userId && x.OrganizationId == orgId, cancellationToken);
			if (!hasMembership)
			{
				return Sort(result);
			}

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

			var roleAssignments = await _reader.Query<UserRoleAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted
					&& x.UserId == userId
					&& x.OrganizationId == orgId
					&& x.ProductId == productId.Value)
				.ToListAsync(cancellationToken);

			var userClaims = await _reader.Query<UserClaimAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted
					&& x.UserId == userId
					&& x.OrganizationId == orgId
					&& x.ProductId == productId.Value
					&& definitionIds.Contains(x.ClaimDefinitionId))
				.ToListAsync(cancellationToken);

			// --- Active layer (org-wide + exact) ---
			var activeRoleIds = roleAssignments
				.Where(x => x.BranchId == null || (branchId != null && x.BranchId == branchId.Value))
				.Select(x => x.RoleId)
				.Distinct()
				.ToList();

			await MergeRoleClaimsAsync(result, definitionById, definitionIds, activeRoleIds, overwrite: true, cancellationToken);

			var activeUserClaims = userClaims
				.Where(x => x.BranchId == null || (branchId != null && x.BranchId == branchId.Value))
				.ToList();
			ApplyUserOverrides(result, definitionById, activeUserClaims);

			// --- Ancestor layers (nearest first): fill missing only ---
			if (inheritanceOn)
			{
				foreach (var ancestorId in ancestorIds)
				{
					var layer = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

					var ancestorRoleIds = roleAssignments
						.Where(x => x.Inheritable && x.BranchId == ancestorId)
						.Select(x => x.RoleId)
						.Distinct()
						.ToList();
					await MergeRoleClaimsAsync(layer, definitionById, definitionIds, ancestorRoleIds, overwrite: true, cancellationToken);

					var ancestorUserClaims = userClaims
						.Where(x => x.Inheritable && x.BranchId == ancestorId)
						.ToList();
					ApplyUserOverrides(layer, definitionById, ancestorUserClaims);

					foreach (var pair in layer)
					{
						if (!result.ContainsKey(pair.Key))
						{
							result[pair.Key] = pair.Value;
						}
					}
				}
			}

			return Sort(result);
		}

		private async Task MergeRoleClaimsAsync(
			Dictionary<string, string> target,
			Dictionary<Guid, ClaimDefinition> definitionById,
			List<Guid> definitionIds,
			List<Guid> roleIds,
			bool overwrite,
			CancellationToken cancellationToken)
		{
			if (roleIds.Count == 0)
			{
				return;
			}

			var roleClaims = await _reader.Query<RoleClaim>().AsNoTracking()
				.Where(x => !x.IsDeleted
					&& roleIds.Contains(x.RoleId)
					&& definitionIds.Contains(x.ClaimDefinitionId))
				.ToListAsync(cancellationToken);

			foreach (var rc in roleClaims)
			{
				if (!definitionById.TryGetValue(rc.ClaimDefinitionId, out var def))
				{
					continue;
				}

				if (overwrite || !target.ContainsKey(def.Code))
				{
					target[def.Code] = rc.Value;
				}
			}
		}

		private static void ApplyUserOverrides(
			Dictionary<string, string> target,
			Dictionary<Guid, ClaimDefinition> definitionById,
			IEnumerable<UserClaimAssignment> assignments)
		{
			foreach (var assignment in assignments)
			{
				if (definitionById.TryGetValue(assignment.ClaimDefinitionId, out var def))
				{
					target[def.Code] = assignment.Value;
				}
			}
		}

		private static IReadOnlyDictionary<string, string> Sort(Dictionary<string, string> result)
			=> result
				.OrderBy(x => x.Key)
				.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

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
