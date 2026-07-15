using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Branches.Queries
{
	public sealed class GetBranchesByFilterQuery : ApplicationRequest<Branch, GetBranchesByFilterQueryResponse>
	{
		public GetBranchesByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetBranchesByFilterQueryResponse : ApplicationResponse<Branch>
	{
		public GetBranchesByFilterQueryResponse(Tuple<int, int, WrapRequest<Branch>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetBranchesByFilterQueryResponse(int statusCode, WrapRequest<Branch> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetBranchesByFilterQueryHandler : ApplicationRequestHandler<Branch, GetBranchesByFilterQuery, GetBranchesByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetBranchesByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Branch> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetBranchesByFilterQueryResponse> Handle(GetBranchesByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<Branch>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetBranchesByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetBranchesByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetBranchesByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
