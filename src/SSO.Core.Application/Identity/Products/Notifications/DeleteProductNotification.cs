using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Products.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Products.Notifications
{
	public sealed class DeleteProductNotification : INotification
	{
		public Product Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteProductNotification(Product payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteProductNotificationHandler : INotificationHandler<DeleteProductNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteProductNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteProductNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteProductNotificationHandler>()
				.Log(LogLevel.Information, "Product deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
