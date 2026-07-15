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
using SSO.Core.Domain.Identity.Permissions.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Permissions.Queries
{
	public sealed class GetPermissionByIdQuery : ApplicationRequest<Permission, GetPermissionByIdQueryResponse>
	{
		public GetPermissionByIdQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class GetPermissionByIdQueryResponse : ApplicationResponse<Permission>
	{
		public GetPermissionByIdQueryResponse(Tuple<int, int, WrapRequest<Permission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetPermissionByIdQueryResponse(int statusCode, WrapRequest<Permission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetPermissionByIdQueryHandler : ApplicationRequestHandler<Permission, GetPermissionByIdQuery, GetPermissionByIdQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetPermissionByIdQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Permission> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetPermissionByIdQueryResponse> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var id = request.Project(x => x.Id);
				var data = await Reader.Query<Permission>().Where(x => x.Id == id && !x.IsDeleted).Select(request).SingleOrDefaultAsync();
				if (data == null) throw new EntityNotFoundException<Permission>(Localizer);
				return new GetPermissionByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetPermissionByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetPermissionByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
