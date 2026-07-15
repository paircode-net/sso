using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ClientProductBindings.Notifications
{
	public sealed class PutClientProductBindingNotification : INotification
	{
		public ClientProductBinding Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutClientProductBindingNotification(ClientProductBinding payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutClientProductBindingNotificationHandler : INotificationHandler<PutClientProductBindingNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutClientProductBindingNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutClientProductBindingNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutClientProductBindingNotificationHandler>()
				.Log(LogLevel.Information, "ClientProductBinding putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
