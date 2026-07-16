using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	public sealed class ExternalIdentityProvider : IdentityAuditableEntity
	{
		/// <summary>Null = available globally; otherwise scoped to an organization.</summary>
		public Guid? OrganizationId { get; set; }
		/// <summary>Entra | Google | Ldap</summary>
		public string ProviderType { get; set; }
		public string Code { get; set; }
		public string DisplayName { get; set; }
		public bool IsEnabled { get; set; }
		/// <summary>When true, first external login may create a local User (F00006-D1). Default false.</summary>
		public bool AllowJitProvisioning { get; set; }
		public string Authority { get; set; }
		public string ClientId { get; set; }

		public ExternalIdentityProvider()
		{
		}
	}
}
