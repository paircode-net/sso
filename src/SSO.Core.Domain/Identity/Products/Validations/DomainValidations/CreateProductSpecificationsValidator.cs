using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Products.Specifications;
using SSO.Core.Domain.Identity.Products.Entity;

namespace SSO.Core.Domain.Identity.Products.Validations.DomainValidations
{
	public sealed class CreateProductSpecificationsValidator : DomainValidator<Product>
	{
		public CreateProductSpecificationsValidator(ProductCodeAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<Product>(spec.Not(), spec.ToString()));
		}
	}
}
