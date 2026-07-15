using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Products.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Products.Notifications
{
	public sealed class PostProductNotification : INotification
	{
		public Product Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostProductNotification(Product payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostProductNotificationHandler : INotificationHandler<PostProductNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostProductNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostProductNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostProductNotificationHandler>()
				.Log(LogLevel.Information, "Product posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
