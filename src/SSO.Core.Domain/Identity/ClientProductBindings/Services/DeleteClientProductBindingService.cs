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
	public sealed class DeleteClientProductBindingServiceRequest : DomainServiceRequest<ClientProductBinding>
	{
		public DeleteClientProductBindingServiceRequest(ClientProductBinding payload) : base(payload) { }
	}

	public sealed class DeleteClientProductBindingServiceRequestHandler
		: DomainServiceRequestHandler<ClientProductBinding, DeleteClientProductBindingServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteClientProductBindingServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<ClientProductBinding> localizer,
			ClientProductBindingValidator entityValidator,
			DeleteClientProductBindingSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<ClientProductBinding> Handle(DeleteClientProductBindingServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
