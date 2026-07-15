using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Permissions.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Permissions.Queries
{
	public sealed class GetPermissionsByFilterQuery : ApplicationRequest<Permission, GetPermissionsByFilterQueryResponse>
	{
		public GetPermissionsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetPermissionsByFilterQueryResponse : ApplicationResponse<Permission>
	{
		public GetPermissionsByFilterQueryResponse(Tuple<int, int, WrapRequest<Permission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetPermissionsByFilterQueryResponse(int statusCode, WrapRequest<Permission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetPermissionsByFilterQueryHandler : ApplicationRequestHandler<Permission, GetPermissionsByFilterQuery, GetPermissionsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetPermissionsByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Permission> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetPermissionsByFilterQueryResponse> Handle(GetPermissionsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<Permission>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetPermissionsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetPermissionsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetPermissionsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
