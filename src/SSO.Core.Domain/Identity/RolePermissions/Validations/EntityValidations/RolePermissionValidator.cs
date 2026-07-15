using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.RolePermissions.Entity;

namespace SSO.Core.Domain.Identity.RolePermissions.Validations.EntityValidations
{
	public sealed class RolePermissionValidator : EntityValidator<RolePermission>
	{
		public RolePermissionValidator()
		{
			RuleFor(x => x.RoleId).NotEmpty().WithMessage("'{PropertyName}' is required!");
			RuleFor(x => x.PermissionId).NotEmpty().WithMessage("'{PropertyName}' is required!");
		}
	}
}
