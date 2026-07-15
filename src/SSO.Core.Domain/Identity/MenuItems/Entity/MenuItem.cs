using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.MenuItems.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.MenuItems.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityMenuItem), Priority = 0)]
	public sealed class MenuItem : IdentityAuditableEntity
	{
		public Guid ProductId { get; set; }
		public string Code { get; set; }
		public string Title { get; set; }
		public string Route { get; set; }
		/// <summary>Permission code that unlocks this menu entry in product UIs.</summary>
		public string PermissionCode { get; set; }
		public int SortOrder { get; set; }

		public MenuItem()
		{
		}
	}
}
