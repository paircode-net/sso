using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using SSO.Core.Application.Identity.MenuItems.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.MenuItems.Services;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.MenuItems.Commands
{
	public sealed class DeleteMenuItemCommand : ApplicationRequest<MenuItem, DeleteMenuItemCommandResponse>
	{
		public DeleteMenuItemCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class DeleteMenuItemCommandResponse : ApplicationResponse<MenuItem>
	{
		public DeleteMenuItemCommandResponse(Tuple<int, int, WrapRequest<MenuItem>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public DeleteMenuItemCommandResponse(int statusCode, WrapRequest<MenuItem> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class DeleteMenuItemCommandHandler : ApplicationRequestHandler<MenuItem, DeleteMenuItemCommand, DeleteMenuItemCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public DeleteMenuItemCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<MenuItem> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<DeleteMenuItemCommandResponse> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<MenuItem>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<MenuItem>(Localizer);
				await Mediator.Send(new DeleteMenuItemServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new DeleteMenuItemNotification(data));
				return new DeleteMenuItemCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<DeleteMenuItemCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new DeleteMenuItemCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
