using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Put;
using SSO.Core.Application.Identity.RolePermissions.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.RolePermissions.Services;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.RolePermissions.Commands
{
	public sealed class PutRolePermissionCommand : ApplicationRequest<RolePermission, PutRolePermissionCommandResponse>
	{
		public PutRolePermissionCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class PutRolePermissionCommandResponse : ApplicationResponse<RolePermission>
	{
		public PutRolePermissionCommandResponse(Tuple<int, int, WrapRequest<RolePermission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PutRolePermissionCommandResponse(int statusCode, WrapRequest<RolePermission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PutRolePermissionCommandHandler : ApplicationRequestHandler<RolePermission, PutRolePermissionCommand, PutRolePermissionCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PutRolePermissionCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<RolePermission> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PutRolePermissionCommandResponse> Handle(PutRolePermissionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<RolePermission>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<RolePermission>(Localizer);
				request.Put(data);
				await Mediator.Send(new UpdateRolePermissionServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PutRolePermissionNotification(data));
				return new PutRolePermissionCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PutRolePermissionCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PutRolePermissionCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
