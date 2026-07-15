using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
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
	public sealed class DeleteRolePermissionCommand : ApplicationRequest<RolePermission, DeleteRolePermissionCommandResponse>
	{
		public DeleteRolePermissionCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class DeleteRolePermissionCommandResponse : ApplicationResponse<RolePermission>
	{
		public DeleteRolePermissionCommandResponse(Tuple<int, int, WrapRequest<RolePermission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public DeleteRolePermissionCommandResponse(int statusCode, WrapRequest<RolePermission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class DeleteRolePermissionCommandHandler : ApplicationRequestHandler<RolePermission, DeleteRolePermissionCommand, DeleteRolePermissionCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public DeleteRolePermissionCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<RolePermission> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<DeleteRolePermissionCommandResponse> Handle(DeleteRolePermissionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<RolePermission>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<RolePermission>(Localizer);
				await Mediator.Send(new DeleteRolePermissionServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new DeleteRolePermissionNotification(data));
				return new DeleteRolePermissionCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<DeleteRolePermissionCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new DeleteRolePermissionCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
