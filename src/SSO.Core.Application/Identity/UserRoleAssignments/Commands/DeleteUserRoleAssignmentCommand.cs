using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
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
	public sealed class DeleteUserRoleAssignmentCommand : ApplicationRequest<UserRoleAssignment, DeleteUserRoleAssignmentCommandResponse>
	{
		public DeleteUserRoleAssignmentCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class DeleteUserRoleAssignmentCommandResponse : ApplicationResponse<UserRoleAssignment>
	{
		public DeleteUserRoleAssignmentCommandResponse(Tuple<int, int, WrapRequest<UserRoleAssignment>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public DeleteUserRoleAssignmentCommandResponse(int statusCode, WrapRequest<UserRoleAssignment> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class DeleteUserRoleAssignmentCommandHandler : ApplicationRequestHandler<UserRoleAssignment, DeleteUserRoleAssignmentCommand, DeleteUserRoleAssignmentCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public DeleteUserRoleAssignmentCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<UserRoleAssignment> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<DeleteUserRoleAssignmentCommandResponse> Handle(DeleteUserRoleAssignmentCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<UserRoleAssignment>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<UserRoleAssignment>(Localizer);
				await Mediator.Send(new DeleteUserRoleAssignmentServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new DeleteUserRoleAssignmentNotification(data));
				return new DeleteUserRoleAssignmentCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<DeleteUserRoleAssignmentCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new DeleteUserRoleAssignmentCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
