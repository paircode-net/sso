using BAYSOFT.Abstractions.Core.Domain.Entities;
using System;

namespace SSO.Core.Domain.Identity._Shared
{
	public abstract class IdentityAuditableEntity : DomainEntity<Guid>
	{
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		public bool IsDeleted { get; set; }

		public void MarkCreated()
		{
			CreatedAt = DateTime.UtcNow;
			IsDeleted = false;
			DeletedAt = null;
		}

		public void TouchUpdated()
		{
			UpdatedAt = DateTime.UtcNow;
		}

		public void MarkDeleted()
		{
			IsDeleted = true;
			DeletedAt = DateTime.UtcNow;
			UpdatedAt = DeletedAt;
		}
	}
}
