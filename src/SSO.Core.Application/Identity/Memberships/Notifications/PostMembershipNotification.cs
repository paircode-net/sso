using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Memberships.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Memberships.Notifications
{
	public sealed class PostMembershipNotification : INotification
	{
		public Membership Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostMembershipNotification(Membership payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostMembershipNotificationHandler : INotificationHandler<PostMembershipNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostMembershipNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostMembershipNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostMembershipNotificationHandler>()
				.Log(LogLevel.Information, "Membership posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
