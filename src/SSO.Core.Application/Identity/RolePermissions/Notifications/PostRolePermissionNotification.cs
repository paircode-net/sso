using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.RolePermissions.Notifications
{
	public sealed class PostRolePermissionNotification : INotification
	{
		public RolePermission Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostRolePermissionNotification(RolePermission payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostRolePermissionNotificationHandler : INotificationHandler<PostRolePermissionNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostRolePermissionNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostRolePermissionNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostRolePermissionNotificationHandler>()
				.Log(LogLevel.Information, "RolePermission posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
