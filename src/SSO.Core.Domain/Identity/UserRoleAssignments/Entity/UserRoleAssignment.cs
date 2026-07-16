using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.UserRoleAssignments.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.UserRoleAssignments.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityUserRoleAssignment), Priority = 0)]
	public sealed class UserRoleAssignment : IdentityAuditableEntity
	{
		public Guid UserId { get; set; }
		public Guid RoleId { get; set; }
		/// <summary>Null = platform-scoped (F00002-D2); set = tenant assignment.</summary>
		public Guid? OrganizationId { get; set; }
		/// <summary>Null = org-wide baseline; set = only that branch (no parent inheritance).</summary>
		public Guid? BranchId { get; set; }
		public Guid ProductId { get; set; }

		public UserRoleAssignment()
		{
		}
	}
}
