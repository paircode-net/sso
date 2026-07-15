using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.UserRoleAssignments.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.UserRoleAssignments.Services;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.UserRoleAssignments.Commands
{
	public sealed class PostUserRoleAssignmentCommand : ApplicationRequest<UserRoleAssignment, PostUserRoleAssignmentCommandResponse>
	{
		public PostUserRoleAssignmentCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostUserRoleAssignmentCommandResponse : ApplicationResponse<UserRoleAssignment>
	{
		public PostUserRoleAssignmentCommandResponse(Tuple<int, int, WrapRequest<UserRoleAssignment>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostUserRoleAssignmentCommandResponse(int statusCode, WrapRequest<UserRoleAssignment> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostUserRoleAssignmentCommandHandler : ApplicationRequestHandler<UserRoleAssignment, PostUserRoleAssignmentCommand, PostUserRoleAssignmentCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostUserRoleAssignmentCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<UserRoleAssignment> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostUserRoleAssignmentCommandResponse> Handle(PostUserRoleAssignmentCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreateUserRoleAssignmentServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PostUserRoleAssignmentNotification(data));
				return new PostUserRoleAssignmentCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostUserRoleAssignmentCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostUserRoleAssignmentCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
