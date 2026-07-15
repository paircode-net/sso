using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Memberships.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Memberships.Notifications
{
	public sealed class PutMembershipNotification : INotification
	{
		public Membership Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutMembershipNotification(Membership payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutMembershipNotificationHandler : INotificationHandler<PutMembershipNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutMembershipNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutMembershipNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutMembershipNotificationHandler>()
				.Log(LogLevel.Information, "Membership putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
