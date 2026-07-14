using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using BAYSOFT.Core.Application.Default.Samples.Notifications;
using BAYSOFT.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;
using BAYSOFT.Core.Domain.Default.Samples.Services;
using BAYSOFT.Core.Domain.Default.Samples.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BAYSOFT.Core.Application.Default.Samples.Commands
{
    public sealed class PostSampleCommand : ApplicationRequest<Sample, PostSampleCommandResponse>
    {
        public PostSampleCommand()
        {
            ConfigKeys(x => x.Id);

            ConfigSuppressedProperties(x => x.Id);

            // TODO: SUPPRESSED RESPONSE PROPERTIES

            // TODO: COMMAND RULES
        }
    }

    public sealed class PostSampleCommandResponse : ApplicationResponse<Sample>
    {
        public PostSampleCommandResponse(Tuple<int, int, WrapRequest<Sample>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple)
            : base(tuple)
        {
        }

        public PostSampleCommandResponse(int statusCode, WrapRequest<Sample> request, object data, string message = "Successful operation!", long? resultCount = null)
            : base(statusCode, request, data, message, resultCount)
        {
        }
    }
    public sealed class PostSampleCommandHandler : ApplicationRequestHandler<Sample, PostSampleCommand, PostSampleCommandResponse>
    {
        private ILoggerFactory Logger { get; set; }
        private IMediator Mediator { get; set; }
        private IStringLocalizer Localizer { get; set; }
        private IDefaultDbContextWriter Writer { get; set; }
        public PostSampleCommandHandler(
            ILoggerFactory logger,
            IMediator mediator,
            IStringLocalizer<Sample> localizer,
            IDefaultDbContextWriter writer
        )
        {
            Logger = logger;
            Mediator = mediator;
            Localizer = localizer;
            Writer = writer;
        }
        public override async Task<PostSampleCommandResponse> Handle(PostSampleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                request.IsValid(Localizer, true);

                var data = request.Post();

                await Mediator.Send(new CreateSampleServiceRequest(data));

                await Writer.CommitAsync(cancellationToken);

                await Mediator.Publish(new PostSampleNotification(data));

                return new PostSampleCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
            }
            catch (Exception exception)
            {
                Logger.CreateLogger<PostSampleCommandHandler>().Log(LogLevel.Error, exception, exception.Message);

                return new PostSampleCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
            }
        }
    }
}
