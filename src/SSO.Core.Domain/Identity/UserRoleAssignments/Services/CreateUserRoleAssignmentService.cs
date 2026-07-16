using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Validations.DomainValidations;
using SSO.Core.Domain.Identity.UserRoleAssignments.Validations.EntityValidations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.UserRoleAssignments.Services
{
	public sealed class CreateUserRoleAssignmentServiceRequest : DomainServiceRequest<UserRoleAssignment>
	{
		public CreateUserRoleAssignmentServiceRequest(UserRoleAssignment payload) : base(payload) { }
	}

	public sealed class CreateUserRoleAssignmentServiceRequestHandler
		: DomainServiceRequestHandler<UserRoleAssignment, CreateUserRoleAssignmentServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		private ICurrentAdminContext AdminContext { get; set; }

		public CreateUserRoleAssignmentServiceRequestHandler(
			IIdentityDbContextWriter writer,
			ICurrentAdminContext adminContext,
			IStringLocalizer<UserRoleAssignment> localizer,
			UserRoleAssignmentValidator entityValidator,
			CreateUserRoleAssignmentSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
			AdminContext = adminContext;
		}

		override public async Task<UserRoleAssignment> Handle(CreateUserRoleAssignmentServiceRequest request, CancellationToken cancellationToken)
		{
			if (request.Payload.OrganizationId is Guid organizationId)
			{
				AdminContext.EnsureCanAccessOrganization(organizationId);
			}
			else if (!AdminContext.IsPlatformAdmin)
			{
				throw new UnauthorizedAccessException("Only platform admins can create platform-scoped role assignments.");
			}

			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
