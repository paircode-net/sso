using BAYSOFT.Core.Domain.Default.Samples.Entity;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BAYSOFT.Core.Application.Default.Samples.Notifications
{
    public sealed class PutSampleNotification : INotification
    {
        public Sample Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public PutSampleNotification(Sample payload)
        {
            Payload = payload;
            CreatedAt = DateTime.UtcNow;
        }
    }

    public sealed class PutSampleNotificationHandler : INotificationHandler<PutSampleNotification>
    {
        private ILoggerFactory Logger { get; set; }
        private IMediator Mediator { get; set; }
        public PutSampleNotificationHandler(
            ILoggerFactory logger,
            IMediator mediator)
        {
            Logger = logger;
            Mediator = mediator;
        }
        public Task Handle(PutSampleNotification notification, CancellationToken cancellationToken)
        {
            Logger.CreateLogger<PutSampleNotificationHandler>()
                .Log(LogLevel.Information, $"Sample putted! - Event Created At: {notification.CreatedAt:yyyy-MM-dd HH:mm:ss} Payload: {JsonConvert.SerializeObject(notification.Payload)}");

            return Task.CompletedTask;
        }
    }
}
