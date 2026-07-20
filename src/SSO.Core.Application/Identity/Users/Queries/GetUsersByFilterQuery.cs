using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Users.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Users.Queries
{
	public sealed class GetUsersByFilterQuery : ApplicationRequest<User, GetUsersByFilterQueryResponse>
	{
		public GetUsersByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			ConfigSuppressedProperties(x => x.PasswordHash);
			ConfigSuppressedProperties(x => x.SecurityStamp);
			ConfigSuppressedProperties(x => x.ConcurrencyStamp);
			ConfigSuppressedResponseProperties(x => x.Password);
			ConfigSuppressedResponseProperties(x => x.PasswordHash);
			ConfigSuppressedResponseProperties(x => x.SecurityStamp);
			ConfigSuppressedResponseProperties(x => x.ConcurrencyStamp);
		}
	}

	public sealed class GetUsersByFilterQueryResponse : ApplicationResponse<User>
	{
		public GetUsersByFilterQueryResponse(Tuple<int, int, WrapRequest<User>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple)
			: base(tuple)
		{
		}

		public GetUsersByFilterQueryResponse(int statusCode, WrapRequest<User> request, object data, string message = "Successful operation!", long? resultCount = null)
			: base(statusCode, request, data, message, resultCount)
		{
		}
	}

	public sealed class GetUsersByFilterQueryHandler : ApplicationRequestHandler<User, GetUsersByFilterQuery, GetUsersByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetUsersByFilterQueryHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<User> localizer,
			IIdentityDbContextReader reader)
		{
			Logger = logger;
			Localizer = localizer;
			Reader = reader;
		}

		public override async Task<GetUsersByFilterQueryResponse> Handle(
			GetUsersByFilterQuery request,
			CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<User>()
					.AsNoTracking()
					.Where(x => !x.IsDeleted)
					.FullSearch(request, out resultCount)
					.ToListAsync(cancellationToken);

				return new GetUsersByFilterQueryResponse(
					(int)HttpStatusCode.OK,
					request,
					data,
					Localizer["Successful operation!"],
					resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetUsersByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetUsersByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
