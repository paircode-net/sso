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
	public sealed class DeleteUserRoleAssignmentServiceRequest : DomainServiceRequest<UserRoleAssignment>
	{
		public DeleteUserRoleAssignmentServiceRequest(UserRoleAssignment payload) : base(payload) { }
	}

	public sealed class DeleteUserRoleAssignmentServiceRequestHandler
		: DomainServiceRequestHandler<UserRoleAssignment, DeleteUserRoleAssignmentServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteUserRoleAssignmentServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<UserRoleAssignment> localizer,
			UserRoleAssignmentValidator entityValidator,
			DeleteUserRoleAssignmentSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<UserRoleAssignment> Handle(DeleteUserRoleAssignmentServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
