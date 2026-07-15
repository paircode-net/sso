using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.RolePermissions.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class RolePermissionMap : IEntityTypeConfiguration<RolePermission>
	{
		public void Configure(EntityTypeBuilder<RolePermission> builder)
		{
			builder.ToTable("RolePermissions");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.RoleId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("RoleId")
				.IsRequired(true);
			builder.Property(e => e.PermissionId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("PermissionId")
				.IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.RoleId, e.PermissionId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}
	}
}
