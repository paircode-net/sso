using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.UserRoleAssignments.Notifications
{
	public sealed class DeleteUserRoleAssignmentNotification : INotification
	{
		public UserRoleAssignment Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteUserRoleAssignmentNotification(UserRoleAssignment payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteUserRoleAssignmentNotificationHandler : INotificationHandler<DeleteUserRoleAssignmentNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteUserRoleAssignmentNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteUserRoleAssignmentNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteUserRoleAssignmentNotificationHandler>()
				.Log(LogLevel.Information, "UserRoleAssignment deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
