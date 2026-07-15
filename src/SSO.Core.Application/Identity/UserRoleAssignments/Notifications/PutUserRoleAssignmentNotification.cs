using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.UserRoleAssignments.Notifications
{
	public sealed class PutUserRoleAssignmentNotification : INotification
	{
		public UserRoleAssignment Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutUserRoleAssignmentNotification(UserRoleAssignment payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutUserRoleAssignmentNotificationHandler : INotificationHandler<PutUserRoleAssignmentNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutUserRoleAssignmentNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutUserRoleAssignmentNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutUserRoleAssignmentNotificationHandler>()
				.Log(LogLevel.Information, "UserRoleAssignment putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
