using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.RolePermissions.Notifications
{
	public sealed class PutRolePermissionNotification : INotification
	{
		public RolePermission Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutRolePermissionNotification(RolePermission payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutRolePermissionNotificationHandler : INotificationHandler<PutRolePermissionNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutRolePermissionNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutRolePermissionNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutRolePermissionNotificationHandler>()
				.Log(LogLevel.Information, "RolePermission putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
