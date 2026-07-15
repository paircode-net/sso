using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.ClientProductBindings.Resources;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.ClientProductBindings.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityClientProductBinding), Priority = 0)]
	public sealed class ClientProductBinding : IdentityAuditableEntity
	{
		public string ClientId { get; set; }
		public Guid ProductId { get; set; }

		public ClientProductBinding()
		{
		}
	}
}
