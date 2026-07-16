using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Branches.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Branches.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Branches.Services
{
	public sealed class CreateBranchServiceRequest : DomainServiceRequest<Branch>
	{
		public CreateBranchServiceRequest(Branch payload) : base(payload) { }
	}

	public sealed class CreateBranchServiceRequestHandler
		: DomainServiceRequestHandler<Branch, CreateBranchServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		private ICurrentAdminContext AdminContext { get; set; }

		public CreateBranchServiceRequestHandler(
			IIdentityDbContextWriter writer,
			ICurrentAdminContext adminContext,
			IStringLocalizer<Branch> localizer,
			BranchValidator entityValidator,
			CreateBranchSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
			AdminContext = adminContext;
		}

		override public async Task<Branch> Handle(CreateBranchServiceRequest request, CancellationToken cancellationToken)
		{
			AdminContext.EnsureCanAccessOrganization(request.Payload.OrganizationId);
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
