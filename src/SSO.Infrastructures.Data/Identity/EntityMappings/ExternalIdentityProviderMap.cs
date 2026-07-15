using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class ExternalIdentityProviderMap : IEntityTypeConfiguration<ExternalIdentityProvider>
	{
		public void Configure(EntityTypeBuilder<ExternalIdentityProvider> builder)
		{
			builder.ToTable("ExternalIdentityProviders");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.OrganizationId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.ProviderType).HasColumnType("NVARCHAR(32)").IsRequired(true);
			builder.Property(e => e.Code).HasColumnType("NVARCHAR(64)").IsRequired(true);
			builder.Property(e => e.DisplayName).HasColumnType("NVARCHAR(128)").IsRequired(true);
			builder.Property(e => e.IsEnabled).HasColumnType("bit").IsRequired(true);
			builder.Property(e => e.Authority).HasColumnType("NVARCHAR(512)").IsRequired(false);
			builder.Property(e => e.ClientId).HasColumnType("NVARCHAR(128)").IsRequired(false);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.Code).IsUnique().HasFilter("[IsDeleted] = 0");
		}
	}
}
