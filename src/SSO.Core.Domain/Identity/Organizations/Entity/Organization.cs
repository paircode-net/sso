using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Organizations.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.Organizations.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityOrganization), Priority = 0)]
	public sealed class Organization : IdentityAuditableEntity
	{
		public string Name { get; set; }
		public string Code { get; set; }

		public Organization()
		{
		}
	}
}
