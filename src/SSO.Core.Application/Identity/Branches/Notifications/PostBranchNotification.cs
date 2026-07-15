using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Branches.Notifications
{
	public sealed class PostBranchNotification : INotification
	{
		public Branch Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostBranchNotification(Branch payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostBranchNotificationHandler : INotificationHandler<PostBranchNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostBranchNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostBranchNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostBranchNotificationHandler>()
				.Log(LogLevel.Information, "Branch posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
