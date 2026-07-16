using System;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	/// <summary>
	/// Caller admin scope from the authenticated principal (feature 00002 / F00002-D2).
	/// </summary>
	public interface ICurrentAdminContext
	{
		bool IsAuthenticated { get; }
		bool IsPlatformAdmin { get; }
		Guid? OrganizationId { get; }

		/// <summary>
		/// Allows access when platform admin, or when <paramref name="organizationId"/> matches the token org.
		/// Throws <see cref="UnauthorizedAccessException"/> otherwise.
		/// </summary>
		void EnsureCanAccessOrganization(Guid organizationId);
	}
}
