using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ClientProductBindings.Notifications
{
	public sealed class DeleteClientProductBindingNotification : INotification
	{
		public ClientProductBinding Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteClientProductBindingNotification(ClientProductBinding payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteClientProductBindingNotificationHandler : INotificationHandler<DeleteClientProductBindingNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteClientProductBindingNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteClientProductBindingNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteClientProductBindingNotificationHandler>()
				.Log(LogLevel.Information, "ClientProductBinding deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
