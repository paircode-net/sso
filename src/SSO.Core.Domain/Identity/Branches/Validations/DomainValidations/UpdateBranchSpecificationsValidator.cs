using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Branches.Specifications;

namespace SSO.Core.Domain.Identity.Branches.Validations.DomainValidations
{
	public sealed class UpdateBranchSpecificationsValidator : DomainValidator<Branch>
	{
		public UpdateBranchSpecificationsValidator(BranchCodeAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<Branch>(spec.Not(), spec.ToString()));
		}
	}
}
