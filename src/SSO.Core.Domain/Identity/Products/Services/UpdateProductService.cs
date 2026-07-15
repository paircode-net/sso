using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Products.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Products.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Products.Services
{
	public sealed class UpdateProductServiceRequest : DomainServiceRequest<Product>
	{
		public UpdateProductServiceRequest(Product payload) : base(payload) { }
	}

	public sealed class UpdateProductServiceRequestHandler
		: DomainServiceRequestHandler<Product, UpdateProductServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateProductServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Product> localizer,
			ProductValidator entityValidator,
			UpdateProductSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Product> Handle(UpdateProductServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
