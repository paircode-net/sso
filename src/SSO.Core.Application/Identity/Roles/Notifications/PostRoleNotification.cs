using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Roles.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Roles.Notifications
{
	public sealed class PostRoleNotification : INotification
	{
		public Role Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostRoleNotification(Role payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostRoleNotificationHandler : INotificationHandler<PostRoleNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostRoleNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostRoleNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostRoleNotificationHandler>()
				.Log(LogLevel.Information, "Role posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
