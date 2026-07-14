using SSO.Core.Domain.Default.Samples.Entity;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Default.Samples.Notifications
{
    public sealed class PatchSampleNotification : INotification
    {
        public Sample Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public PatchSampleNotification(Sample payload)
        {
            Payload = payload;
            CreatedAt = DateTime.UtcNow;
        }
    }

    public sealed class PatchSampleNotificationHandler : INotificationHandler<PatchSampleNotification>
    {
        private ILoggerFactory Logger { get; set; }
        private IMediator Mediator { get; set; }
        public PatchSampleNotificationHandler(
            ILoggerFactory logger,
            IMediator mediator)
        {
            Logger = logger;
            Mediator = mediator;
        }
        public Task Handle(PatchSampleNotification notification, CancellationToken cancellationToken)
        {
            Logger.CreateLogger<PatchSampleNotificationHandler>()
                .Log(LogLevel.Information, $"Sample patched! - Event Created At: {notification.CreatedAt:yyyy-MM-dd HH:mm:ss} Payload: {JsonConvert.SerializeObject(notification.Payload)}");

            return Task.CompletedTask;
        }
    }
}
