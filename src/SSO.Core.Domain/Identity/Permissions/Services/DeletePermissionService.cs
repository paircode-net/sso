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
	public sealed class DeletePermissionServiceRequest : DomainServiceRequest<Permission>
	{
		public DeletePermissionServiceRequest(Permission payload) : base(payload) { }
	}

	public sealed class DeletePermissionServiceRequestHandler
		: DomainServiceRequestHandler<Permission, DeletePermissionServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeletePermissionServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Permission> localizer,
			PermissionValidator entityValidator,
			DeletePermissionSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Permission> Handle(DeletePermissionServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
