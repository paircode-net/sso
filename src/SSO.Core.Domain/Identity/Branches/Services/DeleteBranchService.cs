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
	public sealed class DeleteBranchServiceRequest : DomainServiceRequest<Branch>
	{
		public DeleteBranchServiceRequest(Branch payload) : base(payload) { }
	}

	public sealed class DeleteBranchServiceRequestHandler
		: DomainServiceRequestHandler<Branch, DeleteBranchServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteBranchServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Branch> localizer,
			BranchValidator entityValidator,
			DeleteBranchSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Branch> Handle(DeleteBranchServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
