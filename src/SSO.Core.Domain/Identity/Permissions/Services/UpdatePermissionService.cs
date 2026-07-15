using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.Permissions.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Permissions.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Permissions.Services
{
	public sealed class UpdatePermissionServiceRequest : DomainServiceRequest<Permission>
	{
		public UpdatePermissionServiceRequest(Permission payload) : base(payload) { }
	}

	public sealed class UpdatePermissionServiceRequestHandler
		: DomainServiceRequestHandler<Permission, UpdatePermissionServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdatePermissionServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Permission> localizer,
			PermissionValidator entityValidator,
			UpdatePermissionSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Permission> Handle(UpdatePermissionServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
