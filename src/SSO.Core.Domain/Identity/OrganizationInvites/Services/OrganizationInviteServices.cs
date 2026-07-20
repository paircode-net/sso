using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Validations.DomainValidations;
using SSO.Core.Domain.Identity.OrganizationInvites.Validations.EntityValidations;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Services
{
	public sealed class CreateOrganizationInviteServiceRequest : DomainServiceRequest<OrganizationInvite>
	{
		public string? IssuedRawToken { get; set; }

		public CreateOrganizationInviteServiceRequest(OrganizationInvite payload) : base(payload) { }
	}

	public sealed class CreateOrganizationInviteServiceRequestHandler
		: DomainServiceRequestHandler<OrganizationInvite, CreateOrganizationInviteServiceRequest>
	{
		private readonly IIdentityDbContextWriter _writer;
		private readonly ICurrentAdminContext _adminContext;

		public CreateOrganizationInviteServiceRequestHandler(
			IIdentityDbContextWriter writer,
			ICurrentAdminContext adminContext,
			IStringLocalizer<OrganizationInvite> localizer,
			OrganizationInviteValidator entityValidator,
			CreateOrganizationInviteSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			_writer = writer;
			_adminContext = adminContext;
		}

		public override async Task<OrganizationInvite> Handle(
			CreateOrganizationInviteServiceRequest request,
			CancellationToken cancellationToken)
		{
			_adminContext.EnsureCanAccessOrganization(request.Payload.OrganizationId);

			var rawToken = OrganizationInviteToken.CreateRawToken();
			request.IssuedRawToken = rawToken;

			request.Payload.TokenHash = OrganizationInviteToken.Hash(rawToken);
			request.Payload.Status = OrganizationInviteStatuses.Pending;
			request.Payload.ExpiresAt = DateTime.UtcNow.AddDays(7);
			request.Payload.Email = request.Payload.Email.Trim().ToLowerInvariant();
			request.Payload.MarkCreated();

			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await _writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}

	public sealed class CancelOrganizationInviteServiceRequest : DomainServiceRequest<OrganizationInvite>
	{
		public CancelOrganizationInviteServiceRequest(OrganizationInvite payload) : base(payload) { }
	}

	public sealed class CancelOrganizationInviteServiceRequestHandler
		: DomainServiceRequestHandler<OrganizationInvite, CancelOrganizationInviteServiceRequest>
	{
		private readonly ICurrentAdminContext _adminContext;

		public CancelOrganizationInviteServiceRequestHandler(
			ICurrentAdminContext adminContext,
			IStringLocalizer<OrganizationInvite> localizer,
			OrganizationInviteValidator entityValidator,
			CancelOrganizationInviteSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			_adminContext = adminContext;
		}

		public override Task<OrganizationInvite> Handle(
			CancelOrganizationInviteServiceRequest request,
			CancellationToken cancellationToken)
		{
			_adminContext.EnsureCanAccessOrganization(request.Payload.OrganizationId);
			ValidateDomain(request.Payload);

			request.Payload.Status = OrganizationInviteStatuses.Cancelled;
			request.Payload.RespondedAt = DateTime.UtcNow;
			request.Payload.TouchUpdated();
			ValidateEntity(request.Payload);
			return Task.FromResult(request.Payload);
		}
	}

	public sealed class ResendOrganizationInviteServiceRequest : DomainServiceRequest<OrganizationInvite>
	{
		public string? IssuedRawToken { get; set; }

		public ResendOrganizationInviteServiceRequest(OrganizationInvite payload) : base(payload) { }
	}

	public sealed class ResendOrganizationInviteServiceRequestHandler
		: DomainServiceRequestHandler<OrganizationInvite, ResendOrganizationInviteServiceRequest>
	{
		private readonly ICurrentAdminContext _adminContext;

		public ResendOrganizationInviteServiceRequestHandler(
			ICurrentAdminContext adminContext,
			IStringLocalizer<OrganizationInvite> localizer,
			OrganizationInviteValidator entityValidator,
			ResendOrganizationInviteSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			_adminContext = adminContext;
		}

		public override Task<OrganizationInvite> Handle(
			ResendOrganizationInviteServiceRequest request,
			CancellationToken cancellationToken)
		{
			_adminContext.EnsureCanAccessOrganization(request.Payload.OrganizationId);
			ValidateDomain(request.Payload);

			var rawToken = OrganizationInviteToken.CreateRawToken();
			request.IssuedRawToken = rawToken;

			request.Payload.TokenHash = OrganizationInviteToken.Hash(rawToken);
			request.Payload.Status = OrganizationInviteStatuses.Pending;
			request.Payload.ExpiresAt = DateTime.UtcNow.AddDays(7);
			request.Payload.TouchUpdated();

			ValidateEntity(request.Payload);
			return Task.FromResult(request.Payload);
		}
	}

	public sealed class AcceptOrganizationInviteServiceRequest : DomainServiceRequest<OrganizationInvite>
	{
		public Guid AcceptingUserId { get; }
		public Guid? MembershipId { get; set; }

		public AcceptOrganizationInviteServiceRequest(OrganizationInvite payload, Guid acceptingUserId)
			: base(payload)
		{
			AcceptingUserId = acceptingUserId;
		}
	}

	public sealed class AcceptOrganizationInviteServiceRequestHandler
		: DomainServiceRequestHandler<OrganizationInvite, AcceptOrganizationInviteServiceRequest>
	{
		private readonly IIdentityDbContextWriter _writer;

		public AcceptOrganizationInviteServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<OrganizationInvite> localizer,
			OrganizationInviteValidator entityValidator,
			AcceptOrganizationInviteSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			_writer = writer;
		}

		public override async Task<OrganizationInvite> Handle(
			AcceptOrganizationInviteServiceRequest request,
			CancellationToken cancellationToken)
		{
			var invite = request.Payload;

			if (invite.Status == OrganizationInviteStatuses.Pending && invite.ExpiresAt <= DateTime.UtcNow)
			{
				invite.Status = OrganizationInviteStatuses.Expired;
				invite.TouchUpdated();
			}

			invite.AcceptedUserId = request.AcceptingUserId;
			ValidateDomain(invite);

			var membershipId = await EnsureMembershipAsync(invite, request.AcceptingUserId, cancellationToken);
			request.MembershipId = membershipId;

			invite.Status = OrganizationInviteStatuses.Accepted;
			invite.RespondedAt = DateTime.UtcNow;
			invite.AcceptedUserId = request.AcceptingUserId;
			invite.TouchUpdated();
			ValidateEntity(invite);
			return invite;
		}

		private async Task<Guid> EnsureMembershipAsync(
			OrganizationInvite invite,
			Guid userId,
			CancellationToken cancellationToken)
		{
			var existingId = _writer.Query<Membership>()
				.Where(x => !x.IsDeleted && x.UserId == userId && x.OrganizationId == invite.OrganizationId)
				.Select(x => (Guid?)x.Id)
				.FirstOrDefault();

			if (existingId is Guid id)
			{
				return id;
			}

			var membership = new Membership
			{
				UserId = userId,
				OrganizationId = invite.OrganizationId
			};
			membership.MarkCreated();
			await _writer.AddAsync(membership);
			return membership.Id;
		}
	}

	public sealed class DeclineOrganizationInviteServiceRequest : DomainServiceRequest<OrganizationInvite>
	{
		public Guid? ActingUserId { get; }

		public DeclineOrganizationInviteServiceRequest(OrganizationInvite payload, Guid? actingUserId)
			: base(payload)
		{
			ActingUserId = actingUserId;
		}
	}

	public sealed class DeclineOrganizationInviteServiceRequestHandler
		: DomainServiceRequestHandler<OrganizationInvite, DeclineOrganizationInviteServiceRequest>
	{
		public DeclineOrganizationInviteServiceRequestHandler(
			IStringLocalizer<OrganizationInvite> localizer,
			OrganizationInviteValidator entityValidator,
			DeclineOrganizationInviteSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
		}

		public override Task<OrganizationInvite> Handle(
			DeclineOrganizationInviteServiceRequest request,
			CancellationToken cancellationToken)
		{
			var invite = request.Payload;
			invite.AcceptedUserId = request.ActingUserId;
			ValidateDomain(invite);

			invite.Status = OrganizationInviteStatuses.Declined;
			invite.RespondedAt = DateTime.UtcNow;
			invite.AcceptedUserId = null;
			invite.TouchUpdated();
			ValidateEntity(invite);
			return Task.FromResult(invite);
		}
	}

	public static class OrganizationInviteToken
	{
		public static string CreateRawToken()
		{
			var bytes = RandomNumberGenerator.GetBytes(32);
			return Convert.ToBase64String(bytes)
				.TrimEnd('=')
				.Replace('+', '-')
				.Replace('/', '_');
		}

		public static string Hash(string rawToken)
		{
			var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
			return Convert.ToHexString(hash);
		}
	}
}
