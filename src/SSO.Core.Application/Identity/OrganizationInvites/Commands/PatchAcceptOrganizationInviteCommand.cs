using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;

namespace SSO.Core.Application.Identity.OrganizationInvites.Commands
{
	public sealed class PatchAcceptOrganizationInviteCommand : IRequest<PatchOrganizationInviteResult>
	{
		public string RawToken { get; set; } = string.Empty;
		public Guid AcceptingUserId { get; set; }
	}

	public sealed class PatchDeclineOrganizationInviteCommand : IRequest<PatchOrganizationInviteResult>
	{
		public string RawToken { get; set; } = string.Empty;
		public Guid? ActingUserId { get; set; }
	}

	public sealed class PatchOrganizationInviteResult
	{
		public bool Succeeded { get; init; }
		public string? Error { get; init; }
		public Guid? OrganizationId { get; init; }
		public Guid? MembershipId { get; init; }
	}

	public sealed class PatchAcceptOrganizationInviteCommandHandler
		: IRequestHandler<PatchAcceptOrganizationInviteCommand, PatchOrganizationInviteResult>
	{
		private readonly IIdentityDbContextWriter _writer;
		private readonly IMediator _mediator;

		public PatchAcceptOrganizationInviteCommandHandler(
			IIdentityDbContextWriter writer,
			IMediator mediator)
		{
			_writer = writer;
			_mediator = mediator;
		}

		public async Task<PatchOrganizationInviteResult> Handle(
			PatchAcceptOrganizationInviteCommand request,
			CancellationToken cancellationToken)
		{
			var hash = OrganizationInviteToken.Hash(request.RawToken);
			var invite = await _writer.Query<OrganizationInvite>()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.TokenHash == hash, cancellationToken);

			if (invite is null)
			{
				return Fail("Convite inválido.");
			}

			try
			{
				var serviceRequest = new AcceptOrganizationInviteServiceRequest(invite, request.AcceptingUserId);
				await _mediator.Send(serviceRequest, cancellationToken);
				await _writer.CommitAsync(cancellationToken);

				return new PatchOrganizationInviteResult
				{
					Succeeded = true,
					OrganizationId = invite.OrganizationId,
					MembershipId = serviceRequest.MembershipId
				};
			}
			catch (Exception ex)
			{
				if (invite.Status == OrganizationInviteStatuses.Expired)
				{
					await _writer.CommitAsync(cancellationToken);
				}

				return Fail(ex.Message);
			}
		}

		private static PatchOrganizationInviteResult Fail(string error)
			=> new() { Succeeded = false, Error = error };
	}

	public sealed class PatchDeclineOrganizationInviteCommandHandler
		: IRequestHandler<PatchDeclineOrganizationInviteCommand, PatchOrganizationInviteResult>
	{
		private readonly IIdentityDbContextWriter _writer;
		private readonly IMediator _mediator;

		public PatchDeclineOrganizationInviteCommandHandler(
			IIdentityDbContextWriter writer,
			IMediator mediator)
		{
			_writer = writer;
			_mediator = mediator;
		}

		public async Task<PatchOrganizationInviteResult> Handle(
			PatchDeclineOrganizationInviteCommand request,
			CancellationToken cancellationToken)
		{
			var hash = OrganizationInviteToken.Hash(request.RawToken);
			var invite = await _writer.Query<OrganizationInvite>()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.TokenHash == hash, cancellationToken);

			if (invite is null)
			{
				return new PatchOrganizationInviteResult { Succeeded = false, Error = "Convite inválido." };
			}

			try
			{
				await _mediator.Send(
					new DeclineOrganizationInviteServiceRequest(invite, request.ActingUserId),
					cancellationToken);
				await _writer.CommitAsync(cancellationToken);

				return new PatchOrganizationInviteResult
				{
					Succeeded = true,
					OrganizationId = invite.OrganizationId
				};
			}
			catch (Exception ex)
			{
				return new PatchOrganizationInviteResult { Succeeded = false, Error = ex.Message };
			}
		}
	}
}
