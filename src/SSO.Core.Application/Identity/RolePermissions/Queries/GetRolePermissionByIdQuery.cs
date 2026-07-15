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
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.RolePermissions.Queries
{
	public sealed class GetRolePermissionByIdQuery : ApplicationRequest<RolePermission, GetRolePermissionByIdQueryResponse>
	{
		public GetRolePermissionByIdQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class GetRolePermissionByIdQueryResponse : ApplicationResponse<RolePermission>
	{
		public GetRolePermissionByIdQueryResponse(Tuple<int, int, WrapRequest<RolePermission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetRolePermissionByIdQueryResponse(int statusCode, WrapRequest<RolePermission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetRolePermissionByIdQueryHandler : ApplicationRequestHandler<RolePermission, GetRolePermissionByIdQuery, GetRolePermissionByIdQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }

		public GetRolePermissionByIdQueryHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<RolePermission> localizer, IIdentityDbContextReader reader)
		{
			Logger = logger; Localizer = localizer; Reader = reader;
		}

		override public async Task<GetRolePermissionByIdQueryResponse> Handle(GetRolePermissionByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var id = request.Project(x => x.Id);
				var data = await Reader.Query<RolePermission>().Where(x => x.Id == id && !x.IsDeleted).Select(request).SingleOrDefaultAsync();
				if (data == null) throw new EntityNotFoundException<RolePermission>(Localizer);
				return new GetRolePermissionByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetRolePermissionByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetRolePermissionByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
