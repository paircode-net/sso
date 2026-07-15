using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.MenuItems.Notifications
{
	public sealed class PutMenuItemNotification : INotification
	{
		public MenuItem Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutMenuItemNotification(MenuItem payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutMenuItemNotificationHandler : INotificationHandler<PutMenuItemNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutMenuItemNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutMenuItemNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutMenuItemNotificationHandler>()
				.Log(LogLevel.Information, "MenuItem putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
