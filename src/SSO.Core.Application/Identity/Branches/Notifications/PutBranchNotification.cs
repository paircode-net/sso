using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Branches.Notifications
{
	public sealed class PutBranchNotification : INotification
	{
		public Branch Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutBranchNotification(Branch payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutBranchNotificationHandler : INotificationHandler<PutBranchNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutBranchNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutBranchNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutBranchNotificationHandler>()
				.Log(LogLevel.Information, "Branch putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
