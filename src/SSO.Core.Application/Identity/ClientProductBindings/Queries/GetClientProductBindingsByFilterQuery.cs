using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ClientProductBindings.Queries
{
	public sealed class GetClientProductBindingsByFilterQuery : ApplicationRequest<ClientProductBinding, GetClientProductBindingsByFilterQueryResponse>
	{
		public GetClientProductBindingsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetClientProductBindingsByFilterQueryResponse : ApplicationResponse<ClientProductBinding>
	{
		public GetClientProductBindingsByFilterQueryResponse(Tuple<int, int, WrapRequest<ClientProductBinding>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetClientProductBindingsByFilterQueryResponse(int statusCode, WrapRequest<ClientProductBinding> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetClientProductBindingsByFilterQueryHandler : ApplicationRequestHandler<ClientProductBinding, GetClientProductBindingsByFilterQuery, GetClientProductBindingsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetClientProductBindingsByFilterQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<ClientProductBinding> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetClientProductBindingsByFilterQueryResponse> Handle(GetClientProductBindingsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var data = await Reader.Query<ClientProductBinding>().AsNoTracking().Where(x => !x.IsDeleted).FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetClientProductBindingsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetClientProductBindingsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetClientProductBindingsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
