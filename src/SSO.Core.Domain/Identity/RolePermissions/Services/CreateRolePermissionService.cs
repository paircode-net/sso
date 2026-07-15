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
	public sealed class CreateRolePermissionServiceRequest : DomainServiceRequest<RolePermission>
	{
		public CreateRolePermissionServiceRequest(RolePermission payload) : base(payload) { }
	}

	public sealed class CreateRolePermissionServiceRequestHandler
		: DomainServiceRequestHandler<RolePermission, CreateRolePermissionServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateRolePermissionServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<RolePermission> localizer,
			RolePermissionValidator entityValidator,
			CreateRolePermissionSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<RolePermission> Handle(CreateRolePermissionServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
