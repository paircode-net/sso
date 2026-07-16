using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Branches.Specifications;

namespace SSO.Core.Domain.Identity.Branches.Validations.DomainValidations
{
	public sealed class CreateBranchSpecificationsValidator : DomainValidator<Branch>
	{
		public CreateBranchSpecificationsValidator(
			BranchCodeAlreadyExistsSpecification codeSpec,
			BranchParentWouldCreateCycleSpecification cycleSpec)
		{
			Add(nameof(codeSpec), new DomainRule<Branch>(codeSpec.Not(), codeSpec.ToString()));
			Add(nameof(cycleSpec), new DomainRule<Branch>(cycleSpec.Not(), cycleSpec.ToString()));
		}
	}
}
