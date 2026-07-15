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
	public sealed class PutPermissionCommand : ApplicationRequest<Permission, PutPermissionCommandResponse>
	{
		public PutPermissionCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class PutPermissionCommandResponse : ApplicationResponse<Permission>
	{
		public PutPermissionCommandResponse(Tuple<int, int, WrapRequest<Permission>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PutPermissionCommandResponse(int statusCode, WrapRequest<Permission> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PutPermissionCommandHandler : ApplicationRequestHandler<Permission, PutPermissionCommand, PutPermissionCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PutPermissionCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Permission> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PutPermissionCommandResponse> Handle(PutPermissionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<Permission>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<Permission>(Localizer);
				request.Put(data);
				await Mediator.Send(new UpdatePermissionServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PutPermissionNotification(data));
				return new PutPermissionCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PutPermissionCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PutPermissionCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
