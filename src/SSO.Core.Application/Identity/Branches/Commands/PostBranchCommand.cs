using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.Branches.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Services;
using SSO.Core.Domain.Identity.Branches.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Branches.Commands
{
	public sealed class PostBranchCommand : ApplicationRequest<Branch, PostBranchCommandResponse>
	{
		public PostBranchCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostBranchCommandResponse : ApplicationResponse<Branch>
	{
		public PostBranchCommandResponse(Tuple<int, int, WrapRequest<Branch>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostBranchCommandResponse(int statusCode, WrapRequest<Branch> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostBranchCommandHandler : ApplicationRequestHandler<Branch, PostBranchCommand, PostBranchCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostBranchCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<Branch> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostBranchCommandResponse> Handle(PostBranchCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreateBranchServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				await Mediator.Publish(new PostBranchNotification(data));
				return new PostBranchCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostBranchCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostBranchCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
