using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using SSO.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Default.Samples.Entity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Default.Samples.Queries
{
    public sealed class GetSampleByIdQuery : ApplicationRequest<Sample, GetSampleByIdQueryResponse>
    {
        public GetSampleByIdQuery()
        {
            ConfigKeys(x => x.Id);

            ConfigSuppressedProperties(x => x.Id);

            // TODO: SUPPRESSED RESPONSE PROPERTIES

            Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
        }
    }

    public sealed class GetSampleByIdQueryResponse : ApplicationResponse<Sample>
    {
    public GetSampleByIdQueryResponse(Tuple<int, int, WrapRequest<Sample>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple)
            : base(tuple)
        {
        }

        public GetSampleByIdQueryResponse(int statusCode, WrapRequest<Sample> request, object data, string message = "Successful operation!", long? resultCount = null)
            : base(statusCode, request, data, message, resultCount)
        {
        }
    }
    public sealed class GetSampleByIdQueryHandler : ApplicationRequestHandler<Sample, GetSampleByIdQuery, GetSampleByIdQueryResponse>
    {
        private ILoggerFactory Logger { get; set; }
        private IMediator Mediator { get; set; }
        private IStringLocalizer Localizer { get; set; }
        private IDefaultDbContextReader Reader { get; set; }
        public GetSampleByIdQueryHandler(
            ILoggerFactory logger,
            IMediator mediator,
            IStringLocalizer<Sample> localizer,
            IDefaultDbContextReader reader
        )
        {
            Logger = logger;
            Mediator = mediator;
            Localizer = localizer;
            Reader = reader;
        }
        public override async Task<GetSampleByIdQueryResponse> Handle(GetSampleByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                long resultCount = 1;

                var id = request.Project(x => x.Id);

                var data = await Reader
                    .Query<Sample>()
                    .Where(x => x.Id == id)
                    .Select(request)
                    .SingleOrDefaultAsync();

                if (data == null)
                {
                    throw new EntityNotFoundException<Sample>(Localizer);
                }

                return new GetSampleByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
            }
            catch (Exception exception)
            {
                Logger.CreateLogger<GetSampleByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);

                return new GetSampleByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
            }
        }
    }
}