using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.UserRoleAssignments.Notifications
{
	public sealed class PostUserRoleAssignmentNotification : INotification
	{
		public UserRoleAssignment Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostUserRoleAssignmentNotification(UserRoleAssignment payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostUserRoleAssignmentNotificationHandler : INotificationHandler<PostUserRoleAssignmentNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostUserRoleAssignmentNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostUserRoleAssignmentNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostUserRoleAssignmentNotificationHandler>()
				.Log(LogLevel.Information, "UserRoleAssignment posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
