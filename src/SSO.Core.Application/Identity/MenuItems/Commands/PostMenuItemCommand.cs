using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
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
	public sealed class PostMenuItemCommand : ApplicationRequest<MenuItem, PostMenuItemCommandResponse>
	{
		public PostMenuItemCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostMenuItemCommandResponse : ApplicationResponse<MenuItem>
	{
		public PostMenuItemCommandResponse(Tuple<int, int, WrapRequest<MenuItem>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostMenuItemCommandResponse(int statusCode, WrapRequest<MenuItem> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostMenuItemCommandHandler : ApplicationRequestHandler<MenuItem, PostMenuItemCommand, PostMenuItemCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostMenuItemCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<MenuItem> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostMenuItemCommandResponse> Handle(PostMenuItemCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreateMenuItemServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PostMenuItemNotification(data));
				return new PostMenuItemCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostMenuItemCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostMenuItemCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
