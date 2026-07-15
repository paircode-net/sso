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
using SSO.Core.Domain.Identity.Roles.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Roles.Queries
{
	public sealed class GetRoleByIdQuery : ApplicationRequest<Role, GetRoleByIdQueryResponse>
	{
		public GetRoleByIdQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class GetRoleByIdQueryResponse : ApplicationResponse<Role>
	{
		public GetRoleByIdQueryResponse(Tuple<int, int, WrapRequest<Role>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetRoleByIdQueryResponse(int statusCode, WrapRequest<Role> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetRoleByIdQueryHandler : ApplicationRequestHandler<Role, GetRoleByIdQuery, GetRoleByIdQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetRoleByIdQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Role> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetRoleByIdQueryResponse> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var id = request.Project(x => x.Id);
				var data = await Reader.Query<Role>().Where(x => x.Id == id && !x.IsDeleted).Select(request).SingleOrDefaultAsync();
				if (data == null) throw new EntityNotFoundException<Role>(Localizer);
				return new GetRoleByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetRoleByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetRoleByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
