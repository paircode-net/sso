using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Default.Samples.Specifications;
using SSO.Core.Domain.Default.Samples.Validations.DomainValidations;
using SSO.Core.Domain.Default.Samples.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Memberships.Specifications;
using SSO.Core.Domain.Identity.Memberships.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Memberships.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Organizations.Specifications;
using SSO.Core.Domain.Identity.Organizations.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Organizations.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Products.Specifications;
using SSO.Core.Domain.Identity.Products.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Products.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Users.Specifications;
using SSO.Core.Domain.Identity.Users.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Users.Validations.EntityValidations;

namespace SSO.Middleware.AddServices
{
	public static class AddValidationsConfigurations
	{
		public static IServiceCollection AddSpecifications(this IServiceCollection services)
		{
			services.AddTransient<SampleDescriptionAlreadyExistsSpecification>();

			services.AddTransient<OrganizationCodeAlreadyExistsSpecification>();
			services.AddTransient<ProductCodeAlreadyExistsSpecification>();
			services.AddTransient<MembershipUserOrganizationAlreadyExistsSpecification>();
			services.AddTransient<UserEmailAlreadyExistsSpecification>();

			return services;
		}

		public static IServiceCollection AddEntityValidations(this IServiceCollection services)
		{
			services.AddTransient<SampleValidator>();

			services.AddTransient<OrganizationValidator>();
			services.AddTransient<ProductValidator>();
			services.AddTransient<MembershipValidator>();
			services.AddTransient<UserValidator>();

			return services;
		}

		public static IServiceCollection AddDomainValidations(this IServiceCollection services)
		{
			services.AddTransient<UpdateSampleSpecificationsValidator>();
			services.AddTransient<CreateSampleSpecificationsValidator>();
			services.AddTransient<DeleteSampleSpecificationsValidator>();

			services.AddTransient<CreateOrganizationSpecificationsValidator>();
			services.AddTransient<UpdateOrganizationSpecificationsValidator>();
			services.AddTransient<DeleteOrganizationSpecificationsValidator>();

			services.AddTransient<CreateProductSpecificationsValidator>();
			services.AddTransient<UpdateProductSpecificationsValidator>();
			services.AddTransient<DeleteProductSpecificationsValidator>();

			services.AddTransient<CreateMembershipSpecificationsValidator>();
			services.AddTransient<UpdateMembershipSpecificationsValidator>();
			services.AddTransient<DeleteMembershipSpecificationsValidator>();

			services.AddTransient<CreateUserSpecificationsValidator>();

			return services;
		}
	}
}
