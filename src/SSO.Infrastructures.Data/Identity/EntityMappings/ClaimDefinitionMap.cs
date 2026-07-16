using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class ClaimDefinitionMap : IEntityTypeConfiguration<ClaimDefinition>
	{
		public void Configure(EntityTypeBuilder<ClaimDefinition> builder)
		{
			builder.ToTable("ClaimDefinitions");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Code).HasColumnType("nvarchar(128)").IsRequired(true);
			builder.Property(e => e.Name).HasColumnType("nvarchar(256)").IsRequired(true);
			builder.Property(e => e.Description).HasColumnType("nvarchar(512)").IsRequired(false);
			builder.Property(e => e.ValueType).HasColumnType("nvarchar(32)").IsRequired(true);
			builder.Property(e => e.ProductId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.IsEnabled).HasColumnType("bit").IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.Code).IsUnique().HasFilter("[IsDeleted] = 0");
		}
	}
}
