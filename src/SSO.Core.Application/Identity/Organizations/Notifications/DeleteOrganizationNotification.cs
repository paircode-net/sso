using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Organizations.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Organizations.Notifications
{
	public sealed class DeleteOrganizationNotification : INotification
	{
		public Organization Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteOrganizationNotification(Organization payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteOrganizationNotificationHandler : INotificationHandler<DeleteOrganizationNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteOrganizationNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteOrganizationNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteOrganizationNotificationHandler>()
				.Log(LogLevel.Information, "Organization deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
