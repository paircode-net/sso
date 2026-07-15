using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using SSO.Core.Application.Identity.Roles.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Roles.Services;
using SSO.Core.Domain.Identity.Roles.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Roles.Commands
{
	public sealed class DeleteRoleCommand : ApplicationRequest<Role, DeleteRoleCommandResponse>
	{
		public DeleteRoleCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class DeleteRoleCommandResponse : ApplicationResponse<Role>
	{
		public DeleteRoleCommandResponse(Tuple<int, int, WrapRequest<Role>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public DeleteRoleCommandResponse(int statusCode, WrapRequest<Role> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class DeleteRoleCommandHandler : ApplicationRequestHandler<Role, DeleteRoleCommand, DeleteRoleCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public DeleteRoleCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Role> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<DeleteRoleCommandResponse> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<Role>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<Role>(Localizer);
				await Mediator.Send(new DeleteRoleServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new DeleteRoleNotification(data));
				return new DeleteRoleCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<DeleteRoleCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new DeleteRoleCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
