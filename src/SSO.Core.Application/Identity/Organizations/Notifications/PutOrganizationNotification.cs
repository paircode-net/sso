using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Organizations.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Organizations.Notifications
{
	public sealed class PutOrganizationNotification : INotification
	{
		public Organization Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutOrganizationNotification(Organization payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutOrganizationNotificationHandler : INotificationHandler<PutOrganizationNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutOrganizationNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutOrganizationNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutOrganizationNotificationHandler>()
				.Log(LogLevel.Information, "Organization putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
