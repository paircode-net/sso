using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.OrganizationInvites.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationInvites.Commands
{
	public sealed class PostOrganizationInviteCommand : ApplicationRequest<OrganizationInvite, PostOrganizationInviteCommandResponse>
	{
		public PostOrganizationInviteCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			ConfigSuppressedProperties(x => x.TokenHash);
			ConfigSuppressedProperties(x => x.Status);
			ConfigSuppressedProperties(x => x.ExpiresAt);
			ConfigSuppressedProperties(x => x.InvitedByUserId);
			ConfigSuppressedProperties(x => x.RespondedAt);
			ConfigSuppressedProperties(x => x.AcceptedUserId);
			ConfigSuppressedProperties(x => x.CreatedAt);
			ConfigSuppressedProperties(x => x.UpdatedAt);
			ConfigSuppressedProperties(x => x.DeletedAt);
			ConfigSuppressedProperties(x => x.IsDeleted);
		}

		/// <summary>Plain token returned once after create (not persisted).</summary>
		public string? IssuedRawToken { get; set; }
	}

	public sealed class PostOrganizationInviteCommandResponse : ApplicationResponse<OrganizationInvite>
	{
		public PostOrganizationInviteCommandResponse(Tuple<int, int, WrapRequest<OrganizationInvite>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostOrganizationInviteCommandResponse(int statusCode, WrapRequest<OrganizationInvite> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostOrganizationInviteCommandHandler
		: ApplicationRequestHandler<OrganizationInvite, PostOrganizationInviteCommand, PostOrganizationInviteCommandResponse>
	{
		private readonly ILoggerFactory _logger;
		private readonly IMediator _mediator;
		private readonly IStringLocalizer _localizer;
		private readonly IIdentityDbContextWriter _writer;
		private readonly IMailService _mailService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public PostOrganizationInviteCommandHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<OrganizationInvite> localizer,
			IIdentityDbContextWriter writer,
			IMailService mailService,
			IHttpContextAccessor httpContextAccessor)
		{
			_logger = logger;
			_mediator = mediator;
			_localizer = localizer;
			_writer = writer;
			_mailService = mailService;
			_httpContextAccessor = httpContextAccessor;
		}

		public override async Task<PostOrganizationInviteCommandResponse> Handle(
			PostOrganizationInviteCommand request,
			CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(_localizer, true);
				var data = request.Post();

				var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
					?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
				if (!Guid.TryParse(userIdClaim, out var invitedBy))
				{
					throw new UnauthorizedAccessException("Authenticated user is required to send invites.");
				}

				data.InvitedByUserId = invitedBy;

				var serviceRequest = new CreateOrganizationInviteServiceRequest(data);
				await _mediator.Send(serviceRequest, cancellationToken);
				await _writer.CommitAsync(cancellationToken);

				var rawToken = serviceRequest.IssuedRawToken
					?? throw new InvalidOperationException("Invite token was not issued.");

				var http = _httpContextAccessor.HttpContext;
				var link = http is null
					? $"/Account/AcceptInvite?token={Uri.EscapeDataString(rawToken)}"
					: $"{http.Request.Scheme}://{http.Request.Host}/Account/AcceptInvite?token={Uri.EscapeDataString(rawToken)}";

				await _mailService.SendAsync(
					data.Email,
					"Convite para organização",
					$"InviteToken={rawToken};OrganizationId={data.OrganizationId:D};Link={link}",
					cancellationToken);

				request.IssuedRawToken = rawToken;
				await _mediator.Publish(new PostOrganizationInviteNotification(data), cancellationToken);
				return new PostOrganizationInviteCommandResponse(
					(int)HttpStatusCode.Created,
					request,
					data,
					_localizer["Successful operation!"],
					1);
			}
			catch (Exception exception)
			{
				_logger.CreateLogger<PostOrganizationInviteCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostOrganizationInviteCommandResponse(ExceptionResponseHelper.CreateTuple(_localizer, request, exception));
			}
		}
	}
}
