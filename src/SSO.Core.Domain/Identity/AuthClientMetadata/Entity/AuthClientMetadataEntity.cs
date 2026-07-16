using System;
using SSO.Core.Domain.Identity._Shared;
using SSO.Shared.Identity;

namespace SSO.Core.Domain.Identity.AuthClientMetadata.Entity
{
	/// <summary>Admin sidecar for OpenIddict applications (F00007-D2).</summary>
	public sealed class AuthClientMetadataEntity : IdentityAuditableEntity
	{
		public string ClientId { get; set; } = string.Empty;
		public string? DisplayName { get; set; }
		public bool IsSystem { get; set; }
		public bool IsFirstParty { get; set; }
		public bool IsEnabled { get; set; } = true;
		/// <summary>Always | First | Never</summary>
		public string RequireConsent { get; set; } = AuthClientConsentPolicies.First;
		/// <summary>TTL for Always+remember; default 180.</summary>
		public int ConsentRememberDays { get; set; } = 180;

		public AuthClientMetadataEntity()
		{
		}

		public static AuthClientMetadataEntity Create(
			string clientId,
			string? displayName,
			bool isSystem,
			bool isFirstParty,
			string requireConsent,
			int consentRememberDays = 180)
		{
			if (string.Equals(requireConsent, AuthClientConsentPolicies.Never, StringComparison.OrdinalIgnoreCase)
				&& !isFirstParty)
			{
				throw new InvalidOperationException("RequireConsent=Never requires IsFirstParty=true.");
			}

			var entity = new AuthClientMetadataEntity
			{
				Id = Guid.NewGuid(),
				ClientId = clientId,
				DisplayName = displayName,
				IsSystem = isSystem,
				IsFirstParty = isFirstParty,
				IsEnabled = true,
				RequireConsent = requireConsent,
				ConsentRememberDays = consentRememberDays <= 0 ? 180 : consentRememberDays
			};
			entity.MarkCreated();
			return entity;
		}
	}
}
