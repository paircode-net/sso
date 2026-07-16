using System;
using SSO.Core.Domain.Identity._Shared;

namespace SSO.Core.Domain.Identity.RoleClaims.Entity
{
	/// <summary>Typed claim attached to a Role (inherited via UserRoleAssignment context).</summary>
	public sealed class RoleClaim : IdentityAuditableEntity
	{
		public Guid RoleId { get; set; }
		public Guid ClaimDefinitionId { get; set; }
		public string Value { get; set; } = string.Empty;

		public RoleClaim()
		{
		}

		public static RoleClaim Create(Guid roleId, Guid claimDefinitionId, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidOperationException("value_required");
			}

			var entity = new RoleClaim
			{
				Id = Guid.NewGuid(),
				RoleId = roleId,
				ClaimDefinitionId = claimDefinitionId,
				Value = value
			};
			entity.MarkCreated();
			return entity;
		}
	}
}
