using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
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
	public sealed class PostRolePermissionCommand : ApplicationRequest<RolePermission, PostRolePermissionCommandResponse>
	{
		public PostRolePermissionCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostRolePermissionCommandResponse : ApplicationResponse<RolePermission>
	{
		public PostRolePermissionCommandResponse(Tuple<int, int, WrapRequest<RolePermission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostRolePermissionCommandResponse(int statusCode, WrapRequest<RolePermission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostRolePermissionCommandHandler : ApplicationRequestHandler<RolePermission, PostRolePermissionCommand, PostRolePermissionCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostRolePermissionCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<RolePermission> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostRolePermissionCommandResponse> Handle(PostRolePermissionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreateRolePermissionServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PostRolePermissionNotification(data));
				return new PostRolePermissionCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostRolePermissionCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostRolePermissionCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
