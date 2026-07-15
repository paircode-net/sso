using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.RolePermissions.Queries
{
	public sealed class GetRolePermissionsByFilterQuery : ApplicationRequest<RolePermission, GetRolePermissionsByFilterQueryResponse>
	{
		public GetRolePermissionsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetRolePermissionsByFilterQueryResponse : ApplicationResponse<RolePermission>
	{
		public GetRolePermissionsByFilterQueryResponse(Tuple<int, int, WrapRequest<RolePermission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetRolePermissionsByFilterQueryResponse(int statusCode, WrapRequest<RolePermission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetRolePermissionsByFilterQueryHandler : ApplicationRequestHandler<RolePermission, GetRolePermissionsByFilterQuery, GetRolePermissionsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetRolePermissionsByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<RolePermission> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetRolePermissionsByFilterQueryResponse> Handle(GetRolePermissionsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<RolePermission>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetRolePermissionsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetRolePermissionsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetRolePermissionsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
