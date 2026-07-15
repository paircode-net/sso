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
	public sealed class CreatePermissionServiceRequest : DomainServiceRequest<Permission>
	{
		public CreatePermissionServiceRequest(Permission payload) : base(payload) { }
	}

	public sealed class CreatePermissionServiceRequestHandler
		: DomainServiceRequestHandler<Permission, CreatePermissionServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreatePermissionServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Permission> localizer,
			PermissionValidator entityValidator,
			CreatePermissionSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Permission> Handle(CreatePermissionServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
