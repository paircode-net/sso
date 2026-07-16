using System;
using SSO.Core.Domain.Identity._Shared;

namespace SSO.Core.Domain.Identity.ClientWebhooks.Entity
{
	/// <summary>Webhook URL + HMAC secret per OpenIddict client_id (feature 00005).</summary>
	public sealed class ClientWebhookEndpoint : IdentityAuditableEntity
	{
		public string ClientId { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
		public string HmacSecret { get; set; } = string.Empty;
		public bool IsEnabled { get; set; } = true;

		public ClientWebhookEndpoint()
		{
		}

		public static ClientWebhookEndpoint Create(string clientId, string url, string hmacSecret)
		{
			var entity = new ClientWebhookEndpoint
			{
				Id = Guid.NewGuid(),
				ClientId = clientId,
				Url = url,
				HmacSecret = hmacSecret,
				IsEnabled = true
			};
			entity.MarkCreated();
			return entity;
		}
	}
}
