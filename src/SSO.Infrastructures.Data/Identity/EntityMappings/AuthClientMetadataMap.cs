using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.AuthClientMetadata.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class AuthClientMetadataMap : IEntityTypeConfiguration<AuthClientMetadataEntity>
	{
		public void Configure(EntityTypeBuilder<AuthClientMetadataEntity> builder)
		{
			builder.ToTable("AuthClientMetadata");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.ClientId).HasColumnType("nvarchar(128)").IsRequired(true);
			builder.Property(e => e.DisplayName).HasColumnType("nvarchar(256)").IsRequired(false);
			builder.Property(e => e.IsSystem).HasColumnType("bit").IsRequired(true);
			builder.Property(e => e.IsFirstParty).HasColumnType("bit").IsRequired(true);
			builder.Property(e => e.IsEnabled).HasColumnType("bit").IsRequired(true);
			builder.Property(e => e.RequireConsent).HasColumnType("nvarchar(32)").IsRequired(true);
			builder.Property(e => e.ConsentRememberDays).HasColumnType("int").IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.ClientId).IsUnique().HasFilter("[IsDeleted] = 0");
		}
	}
}
