using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ClientProductBindings.Notifications
{
	public sealed class PostClientProductBindingNotification : INotification
	{
		public ClientProductBinding Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostClientProductBindingNotification(ClientProductBinding payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostClientProductBindingNotificationHandler : INotificationHandler<PostClientProductBindingNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostClientProductBindingNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostClientProductBindingNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostClientProductBindingNotificationHandler>()
				.Log(LogLevel.Information, "ClientProductBinding posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
