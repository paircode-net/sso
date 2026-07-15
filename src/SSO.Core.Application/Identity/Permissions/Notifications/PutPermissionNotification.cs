using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Permissions.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Permissions.Notifications
{
	public sealed class PutPermissionNotification : INotification
	{
		public Permission Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutPermissionNotification(Permission payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutPermissionNotificationHandler : INotificationHandler<PutPermissionNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutPermissionNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutPermissionNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutPermissionNotificationHandler>()
				.Log(LogLevel.Information, "Permission putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
