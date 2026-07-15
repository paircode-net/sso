using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Branches.Notifications
{
	public sealed class DeleteBranchNotification : INotification
	{
		public Branch Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteBranchNotification(Branch payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteBranchNotificationHandler : INotificationHandler<DeleteBranchNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteBranchNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteBranchNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteBranchNotificationHandler>()
				.Log(LogLevel.Information, "Branch deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
