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
	public sealed class PutRoleCommand : ApplicationRequest<Role, PutRoleCommandResponse>
	{
		public PutRoleCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class PutRoleCommandResponse : ApplicationResponse<Role>
	{
		public PutRoleCommandResponse(Tuple<int, int, WrapRequest<Role>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PutRoleCommandResponse(int statusCode, WrapRequest<Role> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PutRoleCommandHandler : ApplicationRequestHandler<Role, PutRoleCommand, PutRoleCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PutRoleCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Role> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PutRoleCommandResponse> Handle(PutRoleCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<Role>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<Role>(Localizer);
				request.Put(data);
				await Mediator.Send(new UpdateRoleServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PutRoleNotification(data));
				return new PutRoleCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PutRoleCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PutRoleCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
