using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.MenuItems.Notifications
{
	public sealed class PostMenuItemNotification : INotification
	{
		public MenuItem Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostMenuItemNotification(MenuItem payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostMenuItemNotificationHandler : INotificationHandler<PostMenuItemNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostMenuItemNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostMenuItemNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostMenuItemNotificationHandler>()
				.Log(LogLevel.Information, "MenuItem posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
