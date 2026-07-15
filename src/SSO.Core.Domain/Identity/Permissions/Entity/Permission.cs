using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Permissions.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.Permissions.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityPermission), Priority = 0)]
	public sealed class Permission : IdentityAuditableEntity
	{
		public string Code { get; set; }
		public string Name { get; set; }

		public Permission()
		{
		}
	}
}
