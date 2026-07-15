using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;

namespace SSO.Core.Domain.Identity.UserRoleAssignments.Validations.EntityValidations
{
	public sealed class UserRoleAssignmentValidator : EntityValidator<UserRoleAssignment>
	{
		public UserRoleAssignmentValidator()
		{
			RuleFor(x => x.UserId).NotEmpty().WithMessage("'{PropertyName}' is required!");
			RuleFor(x => x.RoleId).NotEmpty().WithMessage("'{PropertyName}' is required!");
			RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("'{PropertyName}' is required!");
			RuleFor(x => x.ProductId).NotEmpty().WithMessage("'{PropertyName}' is required!");
		}
	}
}
