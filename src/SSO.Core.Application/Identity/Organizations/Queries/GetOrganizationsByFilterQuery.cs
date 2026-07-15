using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Organizations.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Organizations.Queries
{
	public sealed class GetOrganizationsByFilterQuery : ApplicationRequest<Organization, GetOrganizationsByFilterQueryResponse>
	{
		public GetOrganizationsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetOrganizationsByFilterQueryResponse : ApplicationResponse<Organization>
	{
		public GetOrganizationsByFilterQueryResponse(Tuple<int, int, WrapRequest<Organization>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetOrganizationsByFilterQueryResponse(int statusCode, WrapRequest<Organization> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetOrganizationsByFilterQueryHandler : ApplicationRequestHandler<Organization, GetOrganizationsByFilterQuery, GetOrganizationsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetOrganizationsByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Organization> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetOrganizationsByFilterQueryResponse> Handle(GetOrganizationsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<Organization>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetOrganizationsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetOrganizationsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetOrganizationsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
