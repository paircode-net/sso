using System;
using SSO.Core.Domain.Identity._Shared;
using SSO.Shared.Identity;

namespace SSO.Core.Domain.Identity.UserClaimAssignments.Entity
{
	/// <summary>Per-user typed claim in Org/Branch/Product context (feature 00008).</summary>
	public sealed class UserClaimAssignment : IdentityAuditableEntity
	{
		public Guid UserId { get; set; }
		public Guid ClaimDefinitionId { get; set; }
		public string Value { get; set; } = string.Empty;
		/// <summary>Null = platform-scoped; set = tenant.</summary>
		public Guid? OrganizationId { get; set; }
		/// <summary>Null = org-wide; set = exact branch only (ADR-004).</summary>
		public Guid? BranchId { get; set; }
		public Guid ProductId { get; set; }

		public UserClaimAssignment()
		{
		}

		public static UserClaimAssignment Create(
			Guid userId,
			Guid claimDefinitionId,
			string value,
			Guid productId,
			Guid? organizationId = null,
			Guid? branchId = null)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidOperationException("value_required");
			}

			var entity = new UserClaimAssignment
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				ClaimDefinitionId = claimDefinitionId,
				Value = value,
				OrganizationId = organizationId,
				BranchId = branchId,
				ProductId = productId
			};
			entity.MarkCreated();
			return entity;
		}
	}
}
