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
	public sealed class DeleteProductServiceRequest : DomainServiceRequest<Product>
	{
		public DeleteProductServiceRequest(Product payload) : base(payload) { }
	}

	public sealed class DeleteProductServiceRequestHandler
		: DomainServiceRequestHandler<Product, DeleteProductServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteProductServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Product> localizer,
			ProductValidator entityValidator,
			DeleteProductSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Product> Handle(DeleteProductServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			// soft-delete; entity remains tracked
			return request.Payload;
		}
	}
}
