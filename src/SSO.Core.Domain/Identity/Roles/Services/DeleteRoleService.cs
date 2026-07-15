using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Core.Domain.Identity.Roles.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Roles.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Roles.Services
{
	public sealed class DeleteRoleServiceRequest : DomainServiceRequest<Role>
	{
		public DeleteRoleServiceRequest(Role payload) : base(payload) { }
	}

	public sealed class DeleteRoleServiceRequestHandler
		: DomainServiceRequestHandler<Role, DeleteRoleServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteRoleServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Role> localizer,
			RoleValidator entityValidator,
			DeleteRoleSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Role> Handle(DeleteRoleServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
