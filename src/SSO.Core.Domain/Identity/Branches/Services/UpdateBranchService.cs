using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Branches.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Branches.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Branches.Services
{
	public sealed class UpdateBranchServiceRequest : DomainServiceRequest<Branch>
	{
		public UpdateBranchServiceRequest(Branch payload) : base(payload) { }
	}

	public sealed class UpdateBranchServiceRequestHandler
		: DomainServiceRequestHandler<Branch, UpdateBranchServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateBranchServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Branch> localizer,
			BranchValidator entityValidator,
			UpdateBranchSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Branch> Handle(UpdateBranchServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
