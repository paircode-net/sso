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
	public sealed class CreateRoleServiceRequest : DomainServiceRequest<Role>
	{
		public CreateRoleServiceRequest(Role payload) : base(payload) { }
	}

	public sealed class CreateRoleServiceRequestHandler
		: DomainServiceRequestHandler<Role, CreateRoleServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateRoleServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Role> localizer,
			RoleValidator entityValidator,
			CreateRoleSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Role> Handle(CreateRoleServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
