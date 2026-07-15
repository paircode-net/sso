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
	public sealed class UpdateRolePermissionServiceRequest : DomainServiceRequest<RolePermission>
	{
		public UpdateRolePermissionServiceRequest(RolePermission payload) : base(payload) { }
	}

	public sealed class UpdateRolePermissionServiceRequestHandler
		: DomainServiceRequestHandler<RolePermission, UpdateRolePermissionServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateRolePermissionServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<RolePermission> localizer,
			RolePermissionValidator entityValidator,
			UpdateRolePermissionSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<RolePermission> Handle(UpdateRolePermissionServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
