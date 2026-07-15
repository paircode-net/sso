using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Organizations.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Organizations.Notifications
{
	public sealed class PostOrganizationNotification : INotification
	{
		public Organization Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostOrganizationNotification(Organization payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostOrganizationNotificationHandler : INotificationHandler<PostOrganizationNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostOrganizationNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostOrganizationNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostOrganizationNotificationHandler>()
				.Log(LogLevel.Information, "Organization posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
