using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Validations.DomainValidations;
using SSO.Core.Domain.Identity.RolePermissions.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.RolePermissions.Services
{
	public sealed class DeleteRolePermissionServiceRequest : DomainServiceRequest<RolePermission>
	{
		public DeleteRolePermissionServiceRequest(RolePermission payload) : base(payload) { }
	}

	public sealed class DeleteRolePermissionServiceRequestHandler
		: DomainServiceRequestHandler<RolePermission, DeleteRolePermissionServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteRolePermissionServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<RolePermission> localizer,
			RolePermissionValidator entityValidator,
			DeleteRolePermissionSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<RolePermission> Handle(DeleteRolePermissionServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
