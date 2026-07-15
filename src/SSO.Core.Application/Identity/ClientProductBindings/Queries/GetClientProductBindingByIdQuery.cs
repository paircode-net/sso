using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Select;
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
	public sealed class GetClientProductBindingByIdQuery : ApplicationRequest<ClientProductBinding, GetClientProductBindingByIdQueryResponse>
	{
		public GetClientProductBindingByIdQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class GetClientProductBindingByIdQueryResponse : ApplicationResponse<ClientProductBinding>
	{
		public GetClientProductBindingByIdQueryResponse(Tuple<int, int, WrapRequest<ClientProductBinding>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetClientProductBindingByIdQueryResponse(int statusCode, WrapRequest<ClientProductBinding> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetClientProductBindingByIdQueryHandler : ApplicationRequestHandler<ClientProductBinding, GetClientProductBindingByIdQuery, GetClientProductBindingByIdQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetClientProductBindingByIdQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<ClientProductBinding> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetClientProductBindingByIdQueryResponse> Handle(GetClientProductBindingByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var id = request.Project(x => x.Id);
				var data = await Reader.Query<ClientProductBinding>().Where(x => x.Id == id && !x.IsDeleted).Select(request).SingleOrDefaultAsync();
				if (data == null) throw new EntityNotFoundException<ClientProductBinding>(Localizer);
				return new GetClientProductBindingByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetClientProductBindingByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetClientProductBindingByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
