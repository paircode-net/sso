using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationInvites.Queries
{
	public sealed class GetOrganizationInvitesByFilterQuery
		: ApplicationRequest<OrganizationInvite, GetOrganizationInvitesByFilterQueryResponse>
	{
		public GetOrganizationInvitesByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			ConfigSuppressedProperties(x => x.TokenHash);
		}
	}

	public sealed class GetOrganizationInvitesByFilterQueryResponse : ApplicationResponse<OrganizationInvite>
	{
		public GetOrganizationInvitesByFilterQueryResponse(Tuple<int, int, WrapRequest<OrganizationInvite>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetOrganizationInvitesByFilterQueryResponse(int statusCode, WrapRequest<OrganizationInvite> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetOrganizationInvitesByFilterQueryHandler
		: ApplicationRequestHandler<OrganizationInvite, GetOrganizationInvitesByFilterQuery, GetOrganizationInvitesByFilterQueryResponse>
	{
		private readonly ILoggerFactory _logger;
		private readonly IStringLocalizer _localizer;
		private readonly IIdentityDbContextReader _reader;

		public GetOrganizationInvitesByFilterQueryHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<OrganizationInvite> localizer,
			IIdentityDbContextReader reader)
		{
			_logger = logger;
			_localizer = localizer;
			_reader = reader;
		}

		public override async Task<GetOrganizationInvitesByFilterQueryResponse> Handle(
			GetOrganizationInvitesByFilterQuery request,
			CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await _reader.Query<OrganizationInvite>()
					.AsNoTracking()
					.Where(x => !x.IsDeleted)
					.FullSearch(request, out resultCount)
					.ToListAsync(cancellationToken);
				return new GetOrganizationInvitesByFilterQueryResponse(
					(int)HttpStatusCode.OK,
					request,
					data,
					_localizer["Successful operation!"],
					resultCount);
			}
			catch (Exception exception)
			{
				_logger.CreateLogger<GetOrganizationInvitesByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetOrganizationInvitesByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(_localizer, request, exception));
			}
		}
	}
}
