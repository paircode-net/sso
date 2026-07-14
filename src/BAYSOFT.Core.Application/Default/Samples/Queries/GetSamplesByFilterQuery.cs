using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using BAYSOFT.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;
using BAYSOFT.Core.Domain.Default.Samples.Entity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BAYSOFT.Core.Application.Default.Samples.Queries
{
    public sealed class GetSamplesByFilterQuery : ApplicationRequest<Sample, GetSamplesByFilterQueryResponse>
    {
        public GetSamplesByFilterQuery()
        {
            ConfigKeys(x => x.Id);

            ConfigSuppressedProperties(x => x.Id);

            // TODO: SUPPRESSED RESPONSE PROPERTIES

            // TODO: ByFilterQuery RULES
        }
    }

    public sealed class GetSamplesByFilterQueryResponse : ApplicationResponse<Sample>
    {
        public GetSamplesByFilterQueryResponse(Tuple<int, int, WrapRequest<Sample>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple)
            : base(tuple)
        {
        }

        public GetSamplesByFilterQueryResponse(int statusCode, WrapRequest<Sample> request, object data, string message = "Successful operation!", long? resultCount = null)
            : base(statusCode, request, data, message, resultCount)
        {
        }
    }
    public sealed class GetSamplesByFilterQueryHandler : ApplicationRequestHandler<Sample, GetSamplesByFilterQuery, GetSamplesByFilterQueryResponse>
    {
        private ILoggerFactory Logger { get; set; }
        private IMediator Mediator { get; set; }
        private IStringLocalizer Localizer { get; set; }
        private IDefaultDbContextReader Reader { get; set; }
        public GetSamplesByFilterQueryHandler(
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
        public override async Task<GetSamplesByFilterQueryResponse> Handle(GetSamplesByFilterQuery request, CancellationToken cancellationToken)
        {
            try
            {
                long resultCount = 1;

                var id = request.Project(x => x.Id);

                var data = await Reader
                    .Query<Sample>()
                    .AsNoTracking()
                    .FullSearch(request, out resultCount)
                    .ToListAsync(cancellationToken);

                return new GetSamplesByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
            }
            catch (Exception exception)
            {
                Logger.CreateLogger<GetSamplesByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);

                return new GetSamplesByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
            }
        }
    }
}
