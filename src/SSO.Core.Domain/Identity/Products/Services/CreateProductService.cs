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
	public sealed class CreateProductServiceRequest : DomainServiceRequest<Product>
	{
		public CreateProductServiceRequest(Product payload) : base(payload) { }
	}

	public sealed class CreateProductServiceRequestHandler
		: DomainServiceRequestHandler<Product, CreateProductServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateProductServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<Product> localizer,
			ProductValidator entityValidator,
			CreateProductSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<Product> Handle(CreateProductServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
