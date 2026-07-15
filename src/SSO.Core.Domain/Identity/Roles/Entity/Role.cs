using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Roles.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.Roles.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityRole), Priority = 0)]
	public sealed class Role : IdentityAuditableEntity
	{
		public string Code { get; set; }
		public string Name { get; set; }

		public Role()
		{
		}
	}
}
