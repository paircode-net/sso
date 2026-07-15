using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Organizations.Services;
using SSO.Core.Domain.Identity.Organizations.Specifications;
using SSO.Core.Domain.Identity.Organizations.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Organizations.Validations.EntityValidations;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Identity;

namespace SSO.Tests.UnitTests.Core.Domain.Identity.Organizations.Services
{
	[TestClass]
	public class CreateOrganizationServiceScenarios
	{
		[TestMethod]
		public async Task CreateOrganization_Should_Persist_Entity()
		{
			using var context = IdentityDbContextExtensions.GetInMemoryIdentityDbContext(nameof(CreateOrganization_Should_Persist_Entity));
			var reader = context.GetDbContextReader();
			var writer = context.GetDbContextWriter();

			var handler = new CreateOrganizationServiceRequestHandler(
				writer,
				GenericHelper.CreateLocalizer<Organization>(),
				new OrganizationValidator(),
				new CreateOrganizationSpecificationsValidator(
					new OrganizationCodeAlreadyExistsSpecification(reader)));

			var organization = new Organization { Name = "Unit Org", Code = "unit-org" };
			organization.MarkCreated();

			var result = await handler.Handle(new CreateOrganizationServiceRequest(organization), default);
			await writer.CommitAsync();

			Assert.AreEqual("unit-org", result.Code);
			Assert.AreEqual(1, context.Organizations.Count());
		}
	}
}
