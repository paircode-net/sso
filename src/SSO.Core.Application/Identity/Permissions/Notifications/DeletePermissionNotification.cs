using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Permissions.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Permissions.Notifications
{
	public sealed class DeletePermissionNotification : INotification
	{
		public Permission Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeletePermissionNotification(Permission payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeletePermissionNotificationHandler : INotificationHandler<DeletePermissionNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeletePermissionNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeletePermissionNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeletePermissionNotificationHandler>()
				.Log(LogLevel.Information, "Permission deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
