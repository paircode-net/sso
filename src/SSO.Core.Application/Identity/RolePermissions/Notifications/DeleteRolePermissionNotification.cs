using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.RolePermissions.Notifications
{
	public sealed class DeleteRolePermissionNotification : INotification
	{
		public RolePermission Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteRolePermissionNotification(RolePermission payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteRolePermissionNotificationHandler : INotificationHandler<DeleteRolePermissionNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteRolePermissionNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteRolePermissionNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteRolePermissionNotificationHandler>()
				.Log(LogLevel.Information, "RolePermission deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
