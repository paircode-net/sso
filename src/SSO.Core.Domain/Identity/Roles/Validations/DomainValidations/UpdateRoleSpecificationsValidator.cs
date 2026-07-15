using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Roles.Specifications;
using SSO.Core.Domain.Identity.Roles.Entity;

namespace SSO.Core.Domain.Identity.Roles.Validations.DomainValidations
{
	public sealed class UpdateRoleSpecificationsValidator : DomainValidator<Role>
	{
		public UpdateRoleSpecificationsValidator(RoleCodeAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<Role>(spec.Not(), spec.ToString()));
		}
	}
}
