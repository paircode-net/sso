using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Core.Application.Identity.OrganizationInvites.Commands
{
	public sealed class AcceptOrganizationInviteCommand : IRequest<AcceptOrganizationInviteResult>
	{
		public string RawToken { get; set; } = string.Empty;
		public Guid AcceptingUserId { get; set; }
	}

	public sealed class AcceptOrganizationInviteResult
	{
		public bool Succeeded { get; init; }
		public string? Error { get; init; }
		public Guid? OrganizationId { get; init; }
		public Guid? MembershipId { get; init; }
	}

	public sealed class DeclineOrganizationInviteCommand : IRequest<AcceptOrganizationInviteResult>
	{
		public string RawToken { get; set; } = string.Empty;
		public Guid? ActingUserId { get; set; }
	}

	public sealed class AcceptOrganizationInviteCommandHandler
		: IRequestHandler<AcceptOrganizationInviteCommand, AcceptOrganizationInviteResult>
	{
		private readonly IIdentityDbContextWriter _writer;
		private readonly UserManager<User> _userManager;

		public AcceptOrganizationInviteCommandHandler(
			IIdentityDbContextWriter writer,
			UserManager<User> userManager)
		{
			_writer = writer;
			_userManager = userManager;
		}

		public async Task<AcceptOrganizationInviteResult> Handle(
			AcceptOrganizationInviteCommand request,
			CancellationToken cancellationToken)
		{
			var hash = OrganizationInviteToken.Hash(request.RawToken);
			var invite = await _writer.Query<OrganizationInvite>()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.TokenHash == hash, cancellationToken);

			if (invite is null)
			{
				return Fail("Convite inválido.");
			}

			if (invite.Status != OrganizationInviteStatuses.Pending || invite.ExpiresAt <= DateTime.UtcNow)
			{
				if (invite.Status == OrganizationInviteStatuses.Pending && invite.ExpiresAt <= DateTime.UtcNow)
				{
					invite.Status = OrganizationInviteStatuses.Expired;
					invite.TouchUpdated();
					await _writer.CommitAsync(cancellationToken);
				}

				return Fail("Convite expirado ou já utilizado.");
			}

			var user = await _userManager.FindByIdAsync(request.AcceptingUserId.ToString());
			if (user is null || user.IsDeleted)
			{
				return Fail("Usuário inválido.");
			}

			if (!string.Equals(user.Email, invite.Email, StringComparison.OrdinalIgnoreCase))
			{
				return Fail("O e-mail autenticado não corresponde ao convite.");
			}

			var exists = await _writer.Query<Membership>()
				.AnyAsync(
					x => !x.IsDeleted
						&& x.UserId == user.Id
						&& x.OrganizationId == invite.OrganizationId,
					cancellationToken);

			Guid membershipId;
			if (!exists)
			{
				var membership = new Membership
				{
					UserId = user.Id,
					OrganizationId = invite.OrganizationId
				};
				membership.MarkCreated();
				await _writer.AddAsync(membership);
				membershipId = membership.Id;
			}
			else
			{
				membershipId = await _writer.Query<Membership>()
					.Where(x => !x.IsDeleted && x.UserId == user.Id && x.OrganizationId == invite.OrganizationId)
					.Select(x => x.Id)
					.FirstAsync(cancellationToken);
			}

			invite.Status = OrganizationInviteStatuses.Accepted;
			invite.RespondedAt = DateTime.UtcNow;
			invite.AcceptedUserId = user.Id;
			invite.TouchUpdated();
			await _writer.CommitAsync(cancellationToken);

			return new AcceptOrganizationInviteResult
			{
				Succeeded = true,
				OrganizationId = invite.OrganizationId,
				MembershipId = membershipId
			};
		}

		private static AcceptOrganizationInviteResult Fail(string error)
			=> new() { Succeeded = false, Error = error };
	}

	public sealed class DeclineOrganizationInviteCommandHandler
		: IRequestHandler<DeclineOrganizationInviteCommand, AcceptOrganizationInviteResult>
	{
		private readonly IIdentityDbContextWriter _writer;
		private readonly UserManager<User> _userManager;

		public DeclineOrganizationInviteCommandHandler(
			IIdentityDbContextWriter writer,
			UserManager<User> userManager)
		{
			_writer = writer;
			_userManager = userManager;
		}

		public async Task<AcceptOrganizationInviteResult> Handle(
			DeclineOrganizationInviteCommand request,
			CancellationToken cancellationToken)
		{
			var hash = OrganizationInviteToken.Hash(request.RawToken);
			var invite = await _writer.Query<OrganizationInvite>()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.TokenHash == hash, cancellationToken);

			if (invite is null)
			{
				return new AcceptOrganizationInviteResult { Succeeded = false, Error = "Convite inválido." };
			}

			if (invite.Status != OrganizationInviteStatuses.Pending)
			{
				return new AcceptOrganizationInviteResult { Succeeded = false, Error = "Convite já respondido." };
			}

			if (request.ActingUserId is Guid userId)
			{
				var user = await _userManager.FindByIdAsync(userId.ToString());
				if (user is not null
					&& !string.Equals(user.Email, invite.Email, StringComparison.OrdinalIgnoreCase))
				{
					return new AcceptOrganizationInviteResult
					{
						Succeeded = false,
						Error = "O e-mail autenticado não corresponde ao convite."
					};
				}
			}

			invite.Status = OrganizationInviteStatuses.Declined;
			invite.RespondedAt = DateTime.UtcNow;
			invite.TouchUpdated();
			await _writer.CommitAsync(cancellationToken);

			return new AcceptOrganizationInviteResult
			{
				Succeeded = true,
				OrganizationId = invite.OrganizationId
			};
		}
	}
}
