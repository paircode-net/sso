using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Validations.DomainValidations;
using SSO.Core.Domain.Identity.ClientProductBindings.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.ClientProductBindings.Services
{
	public sealed class UpdateClientProductBindingServiceRequest : DomainServiceRequest<ClientProductBinding>
	{
		public UpdateClientProductBindingServiceRequest(ClientProductBinding payload) : base(payload) { }
	}

	public sealed class UpdateClientProductBindingServiceRequestHandler
		: DomainServiceRequestHandler<ClientProductBinding, UpdateClientProductBindingServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateClientProductBindingServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<ClientProductBinding> localizer,
			ClientProductBindingValidator entityValidator,
			UpdateClientProductBindingSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<ClientProductBinding> Handle(UpdateClientProductBindingServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
