using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class LdapGroupRoleMapMap : IEntityTypeConfiguration<LdapGroupRoleMap>
	{
		public void Configure(EntityTypeBuilder<LdapGroupRoleMap> builder)
		{
			builder.ToTable("LdapGroupRoleMaps");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.OrganizationId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.GroupIdentifier).HasColumnType("nvarchar(512)").IsRequired(true);
			builder.Property(e => e.RoleId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.ProductId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.BranchId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.OrganizationId, e.GroupIdentifier, e.RoleId, e.ProductId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}
	}
}
