using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.RoleClaims.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class RoleClaimMap : IEntityTypeConfiguration<RoleClaim>
	{
		public void Configure(EntityTypeBuilder<RoleClaim> builder)
		{
			builder.ToTable("AuthRoleClaims");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.RoleId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.ClaimDefinitionId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.Value).HasColumnType("nvarchar(512)").IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.RoleId, e.ClaimDefinitionId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}
	}
}
