using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Memberships.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Memberships.Notifications
{
	public sealed class DeleteMembershipNotification : INotification
	{
		public Membership Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteMembershipNotification(Membership payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteMembershipNotificationHandler : INotificationHandler<DeleteMembershipNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteMembershipNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteMembershipNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteMembershipNotificationHandler>()
				.Log(LogLevel.Information, "Membership deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
