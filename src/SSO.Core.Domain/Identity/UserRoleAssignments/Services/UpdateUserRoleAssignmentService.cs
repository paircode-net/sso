using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Validations.DomainValidations;
using SSO.Core.Domain.Identity.UserRoleAssignments.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.UserRoleAssignments.Services
{
	public sealed class UpdateUserRoleAssignmentServiceRequest : DomainServiceRequest<UserRoleAssignment>
	{
		public UpdateUserRoleAssignmentServiceRequest(UserRoleAssignment payload) : base(payload) { }
	}

	public sealed class UpdateUserRoleAssignmentServiceRequestHandler
		: DomainServiceRequestHandler<UserRoleAssignment, UpdateUserRoleAssignmentServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateUserRoleAssignmentServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<UserRoleAssignment> localizer,
			UserRoleAssignmentValidator entityValidator,
			UpdateUserRoleAssignmentSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<UserRoleAssignment> Handle(UpdateUserRoleAssignmentServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
