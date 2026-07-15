using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Organizations.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Organizations.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Organizations.Services
{
	public sealed class DeleteOrganizationServiceRequest : DomainServiceRequest<Organization>
	{
		public DeleteOrganizationServiceRequest(Organization payload) : base(payload) { }
	}

	public sealed class DeleteOrganizationServiceRequestHandler
		: DomainServiceRequestHandler<Organization, DeleteOrganizationServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteOrganizationServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Organization> localizer,
			OrganizationValidator entityValidator,
			DeleteOrganizationSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Organization> Handle(DeleteOrganizationServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
