using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using SSO.Core.Application.Identity.OrganizationInvites.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationInvites.Commands
{
	public sealed class PatchResendOrganizationInviteCommand : ApplicationRequest<OrganizationInvite, PatchResendOrganizationInviteCommandResponse>
	{
		public PatchResendOrganizationInviteCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class PatchResendOrganizationInviteCommandResponse : ApplicationResponse<OrganizationInvite>
	{
		public PatchResendOrganizationInviteCommandResponse(Tuple<int, int, WrapRequest<OrganizationInvite>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PatchResendOrganizationInviteCommandResponse(int statusCode, WrapRequest<OrganizationInvite> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PatchResendOrganizationInviteCommandHandler
		: ApplicationRequestHandler<OrganizationInvite, PatchResendOrganizationInviteCommand, PatchResendOrganizationInviteCommandResponse>
	{
		private readonly ILoggerFactory _logger;
		private readonly IMediator _mediator;
		private readonly IStringLocalizer _localizer;
		private readonly IIdentityDbContextWriter _writer;
		private readonly IMailService _mailService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public PatchResendOrganizationInviteCommandHandler(
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

		public override async Task<PatchResendOrganizationInviteCommandResponse> Handle(
			PatchResendOrganizationInviteCommand request,
			CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(_localizer, true);
				var id = request.Project(x => x.Id);
				var data = await _writer.Query<OrganizationInvite>()
					.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
				if (data is null)
				{
					throw new EntityNotFoundException<OrganizationInvite>(_localizer);
				}

				var serviceRequest = new ResendOrganizationInviteServiceRequest(data);
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

				await _mediator.Publish(new PatchResendOrganizationInviteNotification(data), cancellationToken);
				return new PatchResendOrganizationInviteCommandResponse((int)HttpStatusCode.OK, request, data, _localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				_logger.CreateLogger<PatchResendOrganizationInviteCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PatchResendOrganizationInviteCommandResponse(ExceptionResponseHelper.CreateTuple(_localizer, request, exception));
			}
		}
	}
}
