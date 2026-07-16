using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.AuthClientMetadata.Entity;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.ClientWebhooks.Entity;
using SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Core.Domain.Identity.RevokedSessions.Entity;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity.WebhookOutbox.Entity;
using SSO.Infrastructures.Data.Identity.EntityMappings;

namespace SSO.Infrastructures.Data.Identity
{
	public sealed class IdentityDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<User, IdentityRole<Guid>, Guid>
	{
		public static string Schema => "IdentityDb";

		public DbSet<Organization> Organizations { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Membership> Memberships { get; set; }
		public DbSet<Branch> Branches { get; set; }
		public DbSet<Permission> Permissions { get; set; }
		public DbSet<Role> AuthRoles { get; set; }
		public DbSet<RolePermission> RolePermissions { get; set; }
		public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }
		public DbSet<ClientProductBinding> ClientProductBindings { get; set; }
		public DbSet<AuthAuditEvent> AuthAuditEvents { get; set; }
		public DbSet<MenuItem> MenuItems { get; set; }
		public DbSet<ExternalIdentityProvider> ExternalIdentityProviders { get; set; }
		public DbSet<OrganizationInvite> OrganizationInvites { get; set; }
		public DbSet<UserSession> UserSessions { get; set; }
		public DbSet<RevokedSession> RevokedSessions { get; set; }
		public DbSet<WebhookOutboxMessage> WebhookOutbox { get; set; }
		public DbSet<ClientWebhookEndpoint> ClientWebhookEndpoints { get; set; }
		public DbSet<LdapGroupRoleMap> LdapGroupRoleMaps { get; set; }
		public DbSet<AuthClientMetadataEntity> AuthClientMetadata { get; set; }
		public DbSet<ClaimDefinition> ClaimDefinitions { get; set; }
		public DbSet<UserClaimAssignment> UserClaimAssignments { get; set; }
		public DbSet<RoleClaim> AuthRoleClaims { get; set; }

		public IdentityDbContext()
		{
		}

		public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.HasDefaultSchema(Schema);

			builder.Entity<User>(entity =>
			{
				entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
				entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
				entity.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
				entity.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);
				entity.Ignore(e => e.Password);
			});

			builder.ApplyConfiguration(new OrganizationMap());
			builder.ApplyConfiguration(new ProductMap());
			builder.ApplyConfiguration(new MembershipMap());
			builder.ApplyConfiguration(new BranchMap());
			builder.ApplyConfiguration(new PermissionMap());
			builder.ApplyConfiguration(new RoleMap());
			builder.ApplyConfiguration(new RolePermissionMap());
			builder.ApplyConfiguration(new UserRoleAssignmentMap());
			builder.ApplyConfiguration(new ClientProductBindingMap());
			builder.ApplyConfiguration(new AuthAuditEventMap());
			builder.ApplyConfiguration(new MenuItemMap());
			builder.ApplyConfiguration(new ExternalIdentityProviderMap());
			builder.ApplyConfiguration(new OrganizationInviteMap());
			builder.ApplyConfiguration(new UserSessionMap());
			builder.ApplyConfiguration(new RevokedSessionMap());
			builder.ApplyConfiguration(new WebhookOutboxMessageMap());
			builder.ApplyConfiguration(new ClientWebhookEndpointMap());
			builder.ApplyConfiguration(new LdapGroupRoleMapMap());
			builder.ApplyConfiguration(new AuthClientMetadataMap());
			builder.ApplyConfiguration(new ClaimDefinitionMap());
			builder.ApplyConfiguration(new UserClaimAssignmentMap());
			builder.ApplyConfiguration(new RoleClaimMap());
		}
	}
}
