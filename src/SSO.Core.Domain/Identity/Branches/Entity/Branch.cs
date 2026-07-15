using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Branches.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.Branches.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityBranch), Priority = 0)]
	public sealed class Branch : IdentityAuditableEntity
	{
		public Guid OrganizationId { get; set; }
		public Guid? ParentBranchId { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }

		public Branch()
		{
		}
	}
}
