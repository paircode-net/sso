using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Memberships.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Memberships.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Memberships.Services
{
	public sealed class DeleteMembershipServiceRequest : DomainServiceRequest<Membership>
	{
		public DeleteMembershipServiceRequest(Membership payload) : base(payload) { }
	}

	public sealed class DeleteMembershipServiceRequestHandler
		: DomainServiceRequestHandler<Membership, DeleteMembershipServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteMembershipServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Membership> localizer,
			MembershipValidator entityValidator,
			DeleteMembershipSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Membership> Handle(DeleteMembershipServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
