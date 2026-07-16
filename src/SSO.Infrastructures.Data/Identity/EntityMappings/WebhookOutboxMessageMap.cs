using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.WebhookOutbox.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class WebhookOutboxMessageMap : IEntityTypeConfiguration<WebhookOutboxMessage>
	{
		public void Configure(EntityTypeBuilder<WebhookOutboxMessage> builder)
		{
			builder.ToTable("WebhookOutbox");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.EventType).HasColumnType("nvarchar(64)").IsRequired(true);
			builder.Property(e => e.ClientId).HasColumnType("nvarchar(128)").IsRequired(true);
			builder.Property(e => e.PayloadJson).HasColumnType("nvarchar(max)").IsRequired(true);
			builder.Property(e => e.Status).HasColumnType("nvarchar(32)").IsRequired(true);
			builder.Property(e => e.AttemptCount).HasColumnType("int").IsRequired(true);
			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.NextAttemptAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeliveredAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.LastError).HasColumnType("nvarchar(1024)").IsRequired(false);

			builder.HasIndex(e => new { e.Status, e.NextAttemptAt });
		}
	}
}
