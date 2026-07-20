using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Default.Samples.Specifications;
using SSO.Core.Domain.Default.Samples.Validations.DomainValidations;
using SSO.Core.Domain.Default.Samples.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Branches.Specifications;
using SSO.Core.Domain.Identity.Branches.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Branches.Validations.EntityValidations;
using SSO.Core.Domain.Identity.ClientProductBindings.Specifications;
using SSO.Core.Domain.Identity.ClientProductBindings.Validations.DomainValidations;
using SSO.Core.Domain.Identity.ClientProductBindings.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Memberships.Specifications;
using SSO.Core.Domain.Identity.Memberships.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Memberships.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Organizations.Specifications;
using SSO.Core.Domain.Identity.Organizations.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Organizations.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Permissions.Specifications;
using SSO.Core.Domain.Identity.Permissions.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Permissions.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Products.Specifications;
using SSO.Core.Domain.Identity.Products.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Products.Validations.EntityValidations;
using SSO.Core.Domain.Identity.RolePermissions.Validations.DomainValidations;
using SSO.Core.Domain.Identity.RolePermissions.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Roles.Specifications;
using SSO.Core.Domain.Identity.Roles.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Roles.Validations.EntityValidations;
using SSO.Core.Domain.Identity.MenuItems.Validations.DomainValidations;
using SSO.Core.Domain.Identity.MenuItems.Validations.EntityValidations;
using SSO.Core.Domain.Identity.OrganizationInvites.Specifications;
using SSO.Core.Domain.Identity.OrganizationInvites.Validations.DomainValidations;
using SSO.Core.Domain.Identity.OrganizationInvites.Validations.EntityValidations;
using SSO.Core.Domain.Identity.UserRoleAssignments.Validations.DomainValidations;
using SSO.Core.Domain.Identity.UserRoleAssignments.Validations.EntityValidations;
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
			services.AddTransient<BranchCodeAlreadyExistsSpecification>();
			services.AddTransient<BranchParentWouldCreateCycleSpecification>();
			services.AddTransient<PermissionCodeAlreadyExistsSpecification>();
			services.AddTransient<RoleCodeAlreadyExistsSpecification>();
			services.AddTransient<ClientProductBindingClientIdAlreadyExistsSpecification>();

			services.AddTransient<OrganizationInviteIsNotPendingSpecification>();
			services.AddTransient<OrganizationInviteIsExpiredSpecification>();
			services.AddTransient<OrganizationInviteAcceptingUserInvalidSpecification>();
			services.AddTransient<OrganizationInviteAcceptingUserEmailMismatchSpecification>();

			return services;
		}

		public static IServiceCollection AddEntityValidations(this IServiceCollection services)
		{
			services.AddTransient<SampleValidator>();

			services.AddTransient<OrganizationValidator>();
			services.AddTransient<ProductValidator>();
			services.AddTransient<MembershipValidator>();
			services.AddTransient<UserValidator>();
			services.AddTransient<BranchValidator>();
			services.AddTransient<PermissionValidator>();
			services.AddTransient<RoleValidator>();
			services.AddTransient<RolePermissionValidator>();
			services.AddTransient<UserRoleAssignmentValidator>();
			services.AddTransient<ClientProductBindingValidator>();
			services.AddTransient<MenuItemValidator>();
			services.AddTransient<OrganizationInviteValidator>();

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

			services.AddTransient<CreateBranchSpecificationsValidator>();
			services.AddTransient<UpdateBranchSpecificationsValidator>();
			services.AddTransient<DeleteBranchSpecificationsValidator>();

			services.AddTransient<CreatePermissionSpecificationsValidator>();
			services.AddTransient<UpdatePermissionSpecificationsValidator>();
			services.AddTransient<DeletePermissionSpecificationsValidator>();

			services.AddTransient<CreateRoleSpecificationsValidator>();
			services.AddTransient<UpdateRoleSpecificationsValidator>();
			services.AddTransient<DeleteRoleSpecificationsValidator>();

			services.AddTransient<CreateRolePermissionSpecificationsValidator>();
			services.AddTransient<UpdateRolePermissionSpecificationsValidator>();
			services.AddTransient<DeleteRolePermissionSpecificationsValidator>();

			services.AddTransient<CreateUserRoleAssignmentSpecificationsValidator>();
			services.AddTransient<UpdateUserRoleAssignmentSpecificationsValidator>();
			services.AddTransient<DeleteUserRoleAssignmentSpecificationsValidator>();

			services.AddTransient<CreateClientProductBindingSpecificationsValidator>();
			services.AddTransient<UpdateClientProductBindingSpecificationsValidator>();
			services.AddTransient<DeleteClientProductBindingSpecificationsValidator>();

			services.AddTransient<CreateMenuItemSpecificationsValidator>();
			services.AddTransient<UpdateMenuItemSpecificationsValidator>();
			services.AddTransient<DeleteMenuItemSpecificationsValidator>();

			services.AddTransient<CreateOrganizationInviteSpecificationsValidator>();
			services.AddTransient<CancelOrganizationInviteSpecificationsValidator>();
			services.AddTransient<ResendOrganizationInviteSpecificationsValidator>();
			services.AddTransient<AcceptOrganizationInviteSpecificationsValidator>();
			services.AddTransient<DeclineOrganizationInviteSpecificationsValidator>();

			return services;
		}
	}
}
