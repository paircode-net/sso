using SSO.Core.Domain.Identity._Shared;
using System;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Entity
{
	public sealed class OrganizationInvite : IdentityAuditableEntity
	{
		public Guid OrganizationId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string TokenHash { get; set; } = string.Empty;
		public string Status { get; set; } = OrganizationInviteStatuses.Pending;
		public DateTime ExpiresAt { get; set; }
		public Guid InvitedByUserId { get; set; }
		public DateTime? RespondedAt { get; set; }
		public Guid? AcceptedUserId { get; set; }

		public bool IsPending() =>
			Status == OrganizationInviteStatuses.Pending && ExpiresAt > DateTime.UtcNow;

		public OrganizationInvite()
		{
		}
	}
}
