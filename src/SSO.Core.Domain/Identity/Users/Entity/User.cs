using BAYSOFT.Abstractions.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSO.Core.Domain.Identity.Users.Entity
{
	public sealed class User : IdentityUser<Guid>, IDomainEntityBase
	{
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		public bool IsDeleted { get; set; }

		[NotMapped]
		public string Password { get; set; }

		public User()
		{
			Id = Guid.NewGuid();
		}

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
