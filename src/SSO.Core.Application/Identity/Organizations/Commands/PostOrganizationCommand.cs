using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.Organizations.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Organizations.Services;
using SSO.Core.Domain.Identity.Organizations.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Organizations.Commands
{
	public sealed class PostOrganizationCommand : ApplicationRequest<Organization, PostOrganizationCommandResponse>
	{
		public PostOrganizationCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostOrganizationCommandResponse : ApplicationResponse<Organization>
	{
		public PostOrganizationCommandResponse(Tuple<int, int, WrapRequest<Organization>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostOrganizationCommandResponse(int statusCode, WrapRequest<Organization> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostOrganizationCommandHandler : ApplicationRequestHandler<Organization, PostOrganizationCommand, PostOrganizationCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostOrganizationCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Organization> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostOrganizationCommandResponse> Handle(PostOrganizationCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreateOrganizationServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PostOrganizationNotification(data));
				return new PostOrganizationCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostOrganizationCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostOrganizationCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
