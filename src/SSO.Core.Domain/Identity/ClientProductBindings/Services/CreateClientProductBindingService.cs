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
	public sealed class CreateClientProductBindingServiceRequest : DomainServiceRequest<ClientProductBinding>
	{
		public CreateClientProductBindingServiceRequest(ClientProductBinding payload) : base(payload) { }
	}

	public sealed class CreateClientProductBindingServiceRequestHandler
		: DomainServiceRequestHandler<ClientProductBinding, CreateClientProductBindingServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateClientProductBindingServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<ClientProductBinding> localizer,
			ClientProductBindingValidator entityValidator,
			CreateClientProductBindingSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<ClientProductBinding> Handle(CreateClientProductBindingServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
