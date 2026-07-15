using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Memberships.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Memberships.Queries
{
	public sealed class GetMembershipsByFilterQuery : ApplicationRequest<Membership, GetMembershipsByFilterQueryResponse>
	{
		public GetMembershipsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetMembershipsByFilterQueryResponse : ApplicationResponse<Membership>
	{
		public GetMembershipsByFilterQueryResponse(Tuple<int, int, WrapRequest<Membership>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetMembershipsByFilterQueryResponse(int statusCode, WrapRequest<Membership> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetMembershipsByFilterQueryHandler : ApplicationRequestHandler<Membership, GetMembershipsByFilterQuery, GetMembershipsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetMembershipsByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Membership> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetMembershipsByFilterQueryResponse> Handle(GetMembershipsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<Membership>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetMembershipsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetMembershipsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetMembershipsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
