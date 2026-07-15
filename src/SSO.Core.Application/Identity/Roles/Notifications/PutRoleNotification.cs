using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Roles.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Roles.Notifications
{
	public sealed class PutRoleNotification : INotification
	{
		public Role Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutRoleNotification(Role payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutRoleNotificationHandler : INotificationHandler<PutRoleNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutRoleNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutRoleNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutRoleNotificationHandler>()
				.Log(LogLevel.Information, "Role putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
