using MediatR;
using Microsoft.Extensions.Logging;
using SSO.Core.Domain.Identity.Users.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Users.Notifications
{
	public sealed class PostUserNotification : INotification
	{
		public User Payload { get; set; }
		public DateTime CreatedAt { get; set; }

		public PostUserNotification(User payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostUserNotificationHandler : INotificationHandler<PostUserNotification>
	{
		private ILoggerFactory Logger { get; set; }

		public PostUserNotificationHandler(ILoggerFactory logger)
		{
			Logger = logger;
		}

		public Task Handle(PostUserNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostUserNotificationHandler>()
				.Log(LogLevel.Information, "User posted! Id={UserId} Email={Email}", notification.Payload.Id, notification.Payload.Email);

			return Task.CompletedTask;
		}
	}
}
