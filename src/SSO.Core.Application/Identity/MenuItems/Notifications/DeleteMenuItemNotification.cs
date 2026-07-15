using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.MenuItems.Notifications
{
	public sealed class DeleteMenuItemNotification : INotification
	{
		public MenuItem Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteMenuItemNotification(MenuItem payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteMenuItemNotificationHandler : INotificationHandler<DeleteMenuItemNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteMenuItemNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteMenuItemNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteMenuItemNotificationHandler>()
				.Log(LogLevel.Information, "MenuItem deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
