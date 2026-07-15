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
	public sealed class UpdateMembershipServiceRequest : DomainServiceRequest<Membership>
	{
		public UpdateMembershipServiceRequest(Membership payload) : base(payload) { }
	}

	public sealed class UpdateMembershipServiceRequestHandler
		: DomainServiceRequestHandler<Membership, UpdateMembershipServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateMembershipServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Membership> localizer,
			MembershipValidator entityValidator,
			UpdateMembershipSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Membership> Handle(UpdateMembershipServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
