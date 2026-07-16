using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.ClientWebhooks.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class ClientWebhookEndpointMap : IEntityTypeConfiguration<ClientWebhookEndpoint>
	{
		public void Configure(EntityTypeBuilder<ClientWebhookEndpoint> builder)
		{
			builder.ToTable("ClientWebhookEndpoints");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.ClientId).HasColumnType("nvarchar(128)").IsRequired(true);
			builder.Property(e => e.Url).HasColumnType("nvarchar(1024)").IsRequired(true);
			builder.Property(e => e.HmacSecret).HasColumnType("nvarchar(256)").IsRequired(true);
			builder.Property(e => e.IsEnabled).HasColumnType("bit").IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.ClientId).IsUnique().HasFilter("[IsDeleted] = 0");
		}
	}
}
