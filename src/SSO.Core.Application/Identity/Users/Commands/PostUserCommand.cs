using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.Users.Notifications;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity.Users.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Users.Commands
{
	public sealed class PostUserCommand : ApplicationRequest<User, PostUserCommandResponse>
	{
		public PostUserCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			ConfigSuppressedProperties(x => x.PasswordHash);
			ConfigSuppressedProperties(x => x.SecurityStamp);
			ConfigSuppressedProperties(x => x.ConcurrencyStamp);
			ConfigSuppressedResponseProperties(x => x.Password);
			ConfigSuppressedResponseProperties(x => x.PasswordHash);
			ConfigSuppressedResponseProperties(x => x.SecurityStamp);
			ConfigSuppressedResponseProperties(x => x.ConcurrencyStamp);
		}
	}

	public sealed class PostUserCommandResponse : ApplicationResponse<User>
	{
		public PostUserCommandResponse(Tuple<int, int, WrapRequest<User>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple)
			: base(tuple)
		{
		}

		public PostUserCommandResponse(int statusCode, WrapRequest<User> request, object data, string message = "Successful operation!", long? resultCount = null)
			: base(statusCode, request, data, message, resultCount)
		{
		}
	}

	public sealed class PostUserCommandHandler : ApplicationRequestHandler<User, PostUserCommand, PostUserCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }

		public PostUserCommandHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<User> localizer)
		{
			Logger = logger;
			Mediator = mediator;
			Localizer = localizer;
		}

		public override async Task<PostUserCommandResponse> Handle(PostUserCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);

				var data = request.Post();

				await Mediator.Send(new CreateUserServiceRequest(data), cancellationToken);

				await Mediator.Publish(new PostUserNotification(data), cancellationToken);

				var responseData = new User
				{
					Id = data.Id,
					Email = data.Email,
					UserName = data.UserName,
					EmailConfirmed = data.EmailConfirmed,
					PhoneNumber = data.PhoneNumber,
					CreatedAt = data.CreatedAt,
					UpdatedAt = data.UpdatedAt,
					DeletedAt = data.DeletedAt,
					IsDeleted = data.IsDeleted
				};

				return new PostUserCommandResponse((int)HttpStatusCode.Created, request, responseData, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostUserCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostUserCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
