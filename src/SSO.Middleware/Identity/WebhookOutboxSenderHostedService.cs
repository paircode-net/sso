using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SSO.Core.Domain.Identity.WebhookOutbox.Entity;
using SSO.Infrastructures.Data.Identity;

namespace SSO.Middleware.Identity
{
	/// <summary>Drains WebhookOutbox with HMAC-SHA256 delivery (feature 00005).</summary>
	public sealed class WebhookOutboxSenderHostedService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<WebhookOutboxSenderHostedService> _logger;

		public WebhookOutboxSenderHostedService(
			IServiceScopeFactory scopeFactory,
			IHttpClientFactory httpClientFactory,
			ILogger<WebhookOutboxSenderHostedService> logger)
		{
			_scopeFactory = scopeFactory;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await ProcessBatchAsync(stoppingToken);
				}
				catch (Exception ex) when (ex is not OperationCanceledException)
				{
					_logger.LogWarning(ex, "Webhook outbox drain failed");
				}

				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}

		internal async Task ProcessBatchAsync(CancellationToken cancellationToken)
		{
			using var scope = _scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
			var now = DateTime.UtcNow;

			var batch = await db.WebhookOutbox
				.Where(x => x.Status == WebhookOutboxStatuses.Pending
					&& (x.NextAttemptAt == null || x.NextAttemptAt <= now)
					&& x.AttemptCount < 8)
				.OrderBy(x => x.CreatedAt)
				.Take(20)
				.ToListAsync(cancellationToken);

			if (batch.Count == 0)
			{
				return;
			}

			var client = _httpClientFactory.CreateClient("sso-webhooks");

			foreach (var message in batch)
			{
				var endpoint = await db.ClientWebhookEndpoints.AsNoTracking()
					.FirstOrDefaultAsync(
						x => !x.IsDeleted && x.IsEnabled && x.ClientId == message.ClientId,
						cancellationToken);

				if (endpoint is null)
				{
					message.Status = WebhookOutboxStatuses.Failed;
					message.LastError = "endpoint_missing";
					continue;
				}

				message.AttemptCount++;
				try
				{
					using var request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url)
					{
						Content = new StringContent(message.PayloadJson, Encoding.UTF8, "application/json")
					};
					var signature = ComputeHmacSha256(endpoint.HmacSecret, message.PayloadJson);
					request.Headers.TryAddWithoutValidation("X-SSO-Signature", "sha256=" + signature);
					request.Headers.TryAddWithoutValidation("X-SSO-Event", message.EventType);

					using var response = await client.SendAsync(request, cancellationToken);
					if (response.IsSuccessStatusCode)
					{
						message.Status = WebhookOutboxStatuses.Delivered;
						message.DeliveredAt = DateTime.UtcNow;
						message.LastError = null;
					}
					else
					{
						message.LastError = $"http_{(int)response.StatusCode}";
						message.NextAttemptAt = DateTime.UtcNow.AddSeconds(Math.Pow(2, message.AttemptCount));
						if (message.AttemptCount >= 8)
						{
							message.Status = WebhookOutboxStatuses.Failed;
						}
					}
				}
				catch (Exception ex)
				{
					message.LastError = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
					message.NextAttemptAt = DateTime.UtcNow.AddSeconds(Math.Pow(2, message.AttemptCount));
					if (message.AttemptCount >= 8)
					{
						message.Status = WebhookOutboxStatuses.Failed;
					}
				}
			}

			await db.SaveChangesAsync(cancellationToken);
		}

		public static string ComputeHmacSha256(string secret, string payload)
		{
			var key = Encoding.UTF8.GetBytes(secret);
			var data = Encoding.UTF8.GetBytes(payload);
			using var hmac = new HMACSHA256(key);
			return Convert.ToHexString(hmac.ComputeHash(data)).ToLowerInvariant();
		}
	}
}
