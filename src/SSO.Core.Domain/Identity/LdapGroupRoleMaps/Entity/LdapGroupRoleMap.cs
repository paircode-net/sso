using System;
using SSO.Core.Domain.Identity._Shared;

namespace SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity
{
	/// <summary>Maps an LDAP group DN/CN to an SSO Role on login (F00006-D4).</summary>
	public sealed class LdapGroupRoleMap : IdentityAuditableEntity
	{
		public Guid OrganizationId { get; set; }
		/// <summary>Full DN preferred; CN match is case-insensitive fallback.</summary>
		public string GroupIdentifier { get; set; } = string.Empty;
		public Guid RoleId { get; set; }
		public Guid ProductId { get; set; }
		public Guid? BranchId { get; set; }

		public LdapGroupRoleMap()
		{
		}

		public static LdapGroupRoleMap Create(
			Guid organizationId,
			string groupIdentifier,
			Guid roleId,
			Guid productId,
			Guid? branchId = null)
		{
			var map = new LdapGroupRoleMap
			{
				Id = Guid.NewGuid(),
				OrganizationId = organizationId,
				GroupIdentifier = groupIdentifier?.Trim() ?? string.Empty,
				RoleId = roleId,
				ProductId = productId,
				BranchId = branchId
			};
			map.MarkCreated();
			return map;
		}
	}
}
