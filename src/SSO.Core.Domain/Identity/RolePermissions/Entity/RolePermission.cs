using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.RolePermissions.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.RolePermissions.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityRolePermission), Priority = 0)]
	public sealed class RolePermission : IdentityAuditableEntity
	{
		public Guid RoleId { get; set; }
		public Guid PermissionId { get; set; }

		public RolePermission()
		{
		}
	}
}
