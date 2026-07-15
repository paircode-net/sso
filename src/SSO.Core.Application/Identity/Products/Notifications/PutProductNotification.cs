using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Products.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Products.Notifications
{
	public sealed class PutProductNotification : INotification
	{
		public Product Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutProductNotification(Product payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutProductNotificationHandler : INotificationHandler<PutProductNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutProductNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutProductNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutProductNotificationHandler>()
				.Log(LogLevel.Information, "Product putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
