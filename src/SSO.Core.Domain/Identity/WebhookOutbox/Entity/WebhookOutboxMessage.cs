using System;
using BAYSOFT.Abstractions.Core.Domain.Entities;

namespace SSO.Core.Domain.Identity.WebhookOutbox.Entity
{
	public static class WebhookOutboxStatuses
	{
		public const string Pending = "pending";
		public const string Delivered = "delivered";
		public const string Failed = "failed";
	}

	public static class WebhookEventTypes
	{
		public const string SessionRevoked = "session.revoked";
	}

	/// <summary>Durable outbox for AuthClient webhooks (feature 00005).</summary>
	public sealed class WebhookOutboxMessage : DomainEntity<Guid>
	{
		public string EventType { get; set; } = string.Empty;
		public string ClientId { get; set; } = string.Empty;
		public string PayloadJson { get; set; } = string.Empty;
		public string Status { get; set; } = WebhookOutboxStatuses.Pending;
		public int AttemptCount { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? NextAttemptAt { get; set; }
		public DateTime? DeliveredAt { get; set; }
		public string? LastError { get; set; }

		public WebhookOutboxMessage()
		{
		}

		public static WebhookOutboxMessage Create(string eventType, string clientId, string payloadJson)
		{
			return new WebhookOutboxMessage
			{
				Id = Guid.NewGuid(),
				EventType = eventType,
				ClientId = clientId ?? string.Empty,
				PayloadJson = payloadJson,
				Status = WebhookOutboxStatuses.Pending,
				AttemptCount = 0,
				CreatedAt = DateTime.UtcNow,
				NextAttemptAt = DateTime.UtcNow
			};
		}
	}
}
