using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using SSO.Core.Application.Default.Samples.Notifications;
using SSO.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Default.Samples.Services;
using SSO.Core.Domain.Default.Samples.Entity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Put;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Default.Samples.Commands
{
    public sealed class PutSampleCommand : ApplicationRequest<Sample, PutSampleCommandResponse>
    {
        public PutSampleCommand()
        {
            ConfigKeys(x => x.Id);

            ConfigSuppressedProperties(x => x.Id);

            // TODO: SUPPRESSED RESPONSE PROPERTIES

            Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
        }
    }

    public sealed class PutSampleCommandResponse : ApplicationResponse<Sample>
    {
        public PutSampleCommandResponse(Tuple<int, int, WrapRequest<Sample>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple)
            : base(tuple)
        {
        }

        public PutSampleCommandResponse(int statusCode, WrapRequest<Sample> request, object data, string message = "Successful operation!", long? resultCount = null)
            : base(statusCode, request, data, message, resultCount)
        {
        }
    }
    public sealed class PutSampleCommandHandler : ApplicationRequestHandler<Sample, PutSampleCommand, PutSampleCommandResponse>
    {
        private ILoggerFactory Logger { get; set; }
        private IMediator Mediator { get; set; }
        private IStringLocalizer Localizer { get; set; }
        private IDefaultDbContextWriter Writer { get; set; }
        public PutSampleCommandHandler(
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
        public override async Task<PutSampleCommandResponse> Handle(PutSampleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                request.IsValid(Localizer, true);

                var id = request.Project(x => x.Id);

                var data = await Writer
                    .Query<Sample>()
                    .SingleOrDefaultAsync(x => x.Id == id);

                if (data == null)
                {
                    throw new EntityNotFoundException<Sample>(Localizer);
                }

                request.Put(data);

                await Mediator.Send(new UpdateSampleServiceRequest(data));

                await Writer.CommitAsync(cancellationToken);

                await Mediator.Publish(new PutSampleNotification(data));

                return new PutSampleCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
            }
            catch (Exception exception)
            {
                Logger.CreateLogger<PutSampleCommandHandler>().Log(LogLevel.Error, exception, exception.Message);

                return new PutSampleCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
            }
        }
    }
}
