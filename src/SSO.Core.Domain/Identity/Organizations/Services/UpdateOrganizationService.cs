using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Organizations.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Organizations.Validations.EntityValidations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Organizations.Services
{
	public sealed class UpdateOrganizationServiceRequest : DomainServiceRequest<Organization>
	{
		public UpdateOrganizationServiceRequest(Organization payload) : base(payload) { }
	}

	public sealed class UpdateOrganizationServiceRequestHandler
		: DomainServiceRequestHandler<Organization, UpdateOrganizationServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateOrganizationServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Organization> localizer,
			OrganizationValidator entityValidator,
			UpdateOrganizationSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Organization> Handle(UpdateOrganizationServiceRequest request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.Payload.BranchAuthzInheritance))
			{
				request.Payload.BranchAuthzInheritance = SSO.Shared.Identity.BranchAuthzInheritancePolicies.Off;
			}
			else if (!SSO.Shared.Identity.BranchAuthzInheritancePolicies.IsValid(request.Payload.BranchAuthzInheritance))
			{
				throw new InvalidOperationException("invalid_branch_authz_inheritance");
			}

			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
