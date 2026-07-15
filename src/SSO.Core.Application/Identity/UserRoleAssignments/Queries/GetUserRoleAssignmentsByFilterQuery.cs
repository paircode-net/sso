using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.UserRoleAssignments.Queries
{
	public sealed class GetUserRoleAssignmentsByFilterQuery : ApplicationRequest<UserRoleAssignment, GetUserRoleAssignmentsByFilterQueryResponse>
	{
		public GetUserRoleAssignmentsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetUserRoleAssignmentsByFilterQueryResponse : ApplicationResponse<UserRoleAssignment>
	{
		public GetUserRoleAssignmentsByFilterQueryResponse(Tuple<int, int, WrapRequest<UserRoleAssignment>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetUserRoleAssignmentsByFilterQueryResponse(int statusCode, WrapRequest<UserRoleAssignment> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetUserRoleAssignmentsByFilterQueryHandler : ApplicationRequestHandler<UserRoleAssignment, GetUserRoleAssignmentsByFilterQuery, GetUserRoleAssignmentsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetUserRoleAssignmentsByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<UserRoleAssignment> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetUserRoleAssignmentsByFilterQueryResponse> Handle(GetUserRoleAssignmentsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<UserRoleAssignment>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetUserRoleAssignmentsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetUserRoleAssignmentsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetUserRoleAssignmentsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
