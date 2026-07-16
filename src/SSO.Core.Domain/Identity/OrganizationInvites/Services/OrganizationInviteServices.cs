using System;
using System.Security.Cryptography;
using System.Text;
using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Validations.DomainValidations;
using SSO.Core.Domain.Identity.OrganizationInvites.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.OrganizationInvites.Services
{
	public sealed class CreateOrganizationInviteServiceRequest : DomainServiceRequest<OrganizationInvite>
	{
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
			if (request.Payload.Status != OrganizationInviteStatuses.Pending)
			{
				throw new InvalidOperationException("Only pending invites can be cancelled.");
			}

			request.Payload.Status = OrganizationInviteStatuses.Cancelled;
			request.Payload.RespondedAt = DateTime.UtcNow;
			request.Payload.TouchUpdated();
			ValidateEntity(request.Payload);
			return Task.FromResult(request.Payload);
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
