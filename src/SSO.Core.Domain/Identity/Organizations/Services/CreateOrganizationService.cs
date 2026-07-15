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
	public sealed class CreateOrganizationServiceRequest : DomainServiceRequest<Organization>
	{
		public CreateOrganizationServiceRequest(Organization payload) : base(payload) { }
	}

	public sealed class CreateOrganizationServiceRequestHandler
		: DomainServiceRequestHandler<Organization, CreateOrganizationServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateOrganizationServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Organization> localizer,
			OrganizationValidator entityValidator,
			CreateOrganizationSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Organization> Handle(CreateOrganizationServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
