using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.MenuItems.Queries
{
	public sealed class GetMenuItemsByFilterQuery : ApplicationRequest<MenuItem, GetMenuItemsByFilterQueryResponse>
	{
		public GetMenuItemsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetMenuItemsByFilterQueryResponse : ApplicationResponse<MenuItem>
	{
		public GetMenuItemsByFilterQueryResponse(Tuple<int, int, WrapRequest<MenuItem>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetMenuItemsByFilterQueryResponse(int statusCode, WrapRequest<MenuItem> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetMenuItemsByFilterQueryHandler : ApplicationRequestHandler<MenuItem, GetMenuItemsByFilterQuery, GetMenuItemsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetMenuItemsByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<MenuItem> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetMenuItemsByFilterQueryResponse> Handle(GetMenuItemsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<MenuItem>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetMenuItemsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetMenuItemsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetMenuItemsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
