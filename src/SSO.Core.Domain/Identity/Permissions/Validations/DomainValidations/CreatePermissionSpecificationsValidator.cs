using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Permissions.Specifications;
using SSO.Core.Domain.Identity.Permissions.Entity;

namespace SSO.Core.Domain.Identity.Permissions.Validations.DomainValidations
{
	public sealed class CreatePermissionSpecificationsValidator : DomainValidator<Permission>
	{
		public CreatePermissionSpecificationsValidator(PermissionCodeAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<Permission>(spec.Not(), spec.ToString()));
		}
	}
}
