using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.ClientProductBindings.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ClientProductBindings.Services;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ClientProductBindings.Commands
{
	public sealed class PostClientProductBindingCommand : ApplicationRequest<ClientProductBinding, PostClientProductBindingCommandResponse>
	{
		public PostClientProductBindingCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostClientProductBindingCommandResponse : ApplicationResponse<ClientProductBinding>
	{
		public PostClientProductBindingCommandResponse(Tuple<int, int, WrapRequest<ClientProductBinding>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostClientProductBindingCommandResponse(int statusCode, WrapRequest<ClientProductBinding> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostClientProductBindingCommandHandler : ApplicationRequestHandler<ClientProductBinding, PostClientProductBindingCommand, PostClientProductBindingCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostClientProductBindingCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<ClientProductBinding> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostClientProductBindingCommandResponse> Handle(PostClientProductBindingCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreateClientProductBindingServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PostClientProductBindingNotification(data));
				return new PostClientProductBindingCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostClientProductBindingCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostClientProductBindingCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
