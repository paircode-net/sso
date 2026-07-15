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
	public sealed class PutUserRoleAssignmentCommand : ApplicationRequest<UserRoleAssignment, PutUserRoleAssignmentCommandResponse>
	{
		public PutUserRoleAssignmentCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class PutUserRoleAssignmentCommandResponse : ApplicationResponse<UserRoleAssignment>
	{
		public PutUserRoleAssignmentCommandResponse(Tuple<int, int, WrapRequest<UserRoleAssignment>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PutUserRoleAssignmentCommandResponse(int statusCode, WrapRequest<UserRoleAssignment> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PutUserRoleAssignmentCommandHandler : ApplicationRequestHandler<UserRoleAssignment, PutUserRoleAssignmentCommand, PutUserRoleAssignmentCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PutUserRoleAssignmentCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<UserRoleAssignment> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PutUserRoleAssignmentCommandResponse> Handle(PutUserRoleAssignmentCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<UserRoleAssignment>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<UserRoleAssignment>(Localizer);
				request.Put(data);
				await Mediator.Send(new UpdateUserRoleAssignmentServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PutUserRoleAssignmentNotification(data));
				return new PutUserRoleAssignmentCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PutUserRoleAssignmentCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PutUserRoleAssignmentCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
