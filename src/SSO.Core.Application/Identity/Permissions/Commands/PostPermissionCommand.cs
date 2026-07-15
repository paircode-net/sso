using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.Permissions.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Permissions.Services;
using SSO.Core.Domain.Identity.Permissions.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Permissions.Commands
{
	public sealed class PostPermissionCommand : ApplicationRequest<Permission, PostPermissionCommandResponse>
	{
		public PostPermissionCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostPermissionCommandResponse : ApplicationResponse<Permission>
	{
		public PostPermissionCommandResponse(Tuple<int, int, WrapRequest<Permission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostPermissionCommandResponse(int statusCode, WrapRequest<Permission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostPermissionCommandHandler : ApplicationRequestHandler<Permission, PostPermissionCommand, PostPermissionCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostPermissionCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Permission> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostPermissionCommandResponse> Handle(PostPermissionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreatePermissionServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PostPermissionNotification(data));
				return new PostPermissionCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostPermissionCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostPermissionCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
