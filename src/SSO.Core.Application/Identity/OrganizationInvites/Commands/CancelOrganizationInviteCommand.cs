using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using SSO.Core.Application.Identity.OrganizationInvites.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationInvites.Commands
{
	public sealed class CancelOrganizationInviteCommand : ApplicationRequest<OrganizationInvite, CancelOrganizationInviteCommandResponse>
	{
		public CancelOrganizationInviteCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class CancelOrganizationInviteCommandResponse : ApplicationResponse<OrganizationInvite>
	{
		public CancelOrganizationInviteCommandResponse(Tuple<int, int, WrapRequest<OrganizationInvite>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public CancelOrganizationInviteCommandResponse(int statusCode, WrapRequest<OrganizationInvite> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class CancelOrganizationInviteCommandHandler
		: ApplicationRequestHandler<OrganizationInvite, CancelOrganizationInviteCommand, CancelOrganizationInviteCommandResponse>
	{
		private readonly ILoggerFactory _logger;
		private readonly IMediator _mediator;
		private readonly IStringLocalizer _localizer;
		private readonly IIdentityDbContextWriter _writer;

		public CancelOrganizationInviteCommandHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<OrganizationInvite> localizer,
			IIdentityDbContextWriter writer)
		{
			_logger = logger;
			_mediator = mediator;
			_localizer = localizer;
			_writer = writer;
		}

		public override async Task<CancelOrganizationInviteCommandResponse> Handle(
			CancelOrganizationInviteCommand request,
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

				await _mediator.Send(new CancelOrganizationInviteServiceRequest(data), cancellationToken);
				await _writer.CommitAsync(cancellationToken);
				await _mediator.Publish(new CancelOrganizationInviteNotification(data), cancellationToken);
				return new CancelOrganizationInviteCommandResponse((int)HttpStatusCode.OK, request, data, _localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				_logger.CreateLogger<CancelOrganizationInviteCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new CancelOrganizationInviteCommandResponse(ExceptionResponseHelper.CreateTuple(_localizer, request, exception));
			}
		}
	}
}
