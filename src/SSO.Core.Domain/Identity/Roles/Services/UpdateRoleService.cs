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
	public sealed class UpdateRoleServiceRequest : DomainServiceRequest<Role>
	{
		public UpdateRoleServiceRequest(Role payload) : base(payload) { }
	}

	public sealed class UpdateRoleServiceRequestHandler
		: DomainServiceRequestHandler<Role, UpdateRoleServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateRoleServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Role> localizer,
			RoleValidator entityValidator,
			UpdateRoleSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Role> Handle(UpdateRoleServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
