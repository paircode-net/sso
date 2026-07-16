using System;
using System.Collections.Generic;
using System.Linq;
using SSO.Core.Domain.Identity.Branches.Entity;

namespace SSO.Core.Domain.Identity.Branches
{
	/// <summary>Walk ParentBranchId for ancestry / cycle detection (ADR-008).</summary>
	public static class BranchAncestry
	{
		public const int MaxDepth = 32;

		/// <summary>Nearest parent first, then grandparent, … up to root.</summary>
		public static IReadOnlyList<Guid> GetAncestorIds(
			IEnumerable<Branch> organizationBranches,
			Guid branchId)
		{
			var byId = organizationBranches
				.Where(x => !x.IsDeleted)
				.ToDictionary(x => x.Id);

			var ancestors = new List<Guid>();
			if (!byId.TryGetValue(branchId, out var current))
			{
				return ancestors;
			}

			var guard = 0;
			var visited = new HashSet<Guid> { branchId };
			while (current.ParentBranchId is Guid parentId && guard++ < MaxDepth)
			{
				if (!visited.Add(parentId))
				{
					break;
				}

				ancestors.Add(parentId);
				if (!byId.TryGetValue(parentId, out current!))
				{
					break;
				}
			}

			return ancestors;
		}

		/// <summary>
		/// True when setting <paramref name="branch"/>.ParentBranchId would introduce a cycle
		/// (self-parent or ancestor of proposed parent is the branch itself).
		/// </summary>
		public static bool WouldCreateCycle(
			IEnumerable<Branch> organizationBranches,
			Branch branch)
		{
			if (branch.ParentBranchId is null)
			{
				return false;
			}

			if (branch.ParentBranchId == branch.Id)
			{
				return true;
			}

			var byId = organizationBranches
				.Where(x => !x.IsDeleted)
				.ToDictionary(x => x.Id);

			// Simulate: walk from proposed parent; if we hit branch.Id, cycle.
			var currentId = branch.ParentBranchId.Value;
			var guard = 0;
			var visited = new HashSet<Guid>();
			while (guard++ < MaxDepth)
			{
				if (currentId == branch.Id)
				{
					return true;
				}

				if (!visited.Add(currentId))
				{
					return true;
				}

				if (!byId.TryGetValue(currentId, out var node) || node.ParentBranchId is null)
				{
					return false;
				}

				// When updating, treat the entity's new parent as authoritative for itself.
				if (node.Id == branch.Id)
				{
					if (branch.ParentBranchId is null)
					{
						return false;
					}

					currentId = branch.ParentBranchId.Value;
					continue;
				}

				currentId = node.ParentBranchId.Value;
			}

			return true;
		}
	}
}
