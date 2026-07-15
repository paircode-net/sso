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
	public sealed class CreateMembershipServiceRequest : DomainServiceRequest<Membership>
	{
		public CreateMembershipServiceRequest(Membership payload) : base(payload) { }
	}

	public sealed class CreateMembershipServiceRequestHandler
		: DomainServiceRequestHandler<Membership, CreateMembershipServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateMembershipServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Membership> localizer,
			MembershipValidator entityValidator,
			CreateMembershipSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Membership> Handle(CreateMembershipServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
