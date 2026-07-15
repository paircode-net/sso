using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity.Users.Specifications;

namespace SSO.Core.Domain.Identity.Users.Validations.DomainValidations
{
	public sealed class CreateUserSpecificationsValidator : DomainValidator<User>
	{
		public CreateUserSpecificationsValidator(UserEmailAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<User>(spec.Not(), spec.ToString()));
		}
	}
}
