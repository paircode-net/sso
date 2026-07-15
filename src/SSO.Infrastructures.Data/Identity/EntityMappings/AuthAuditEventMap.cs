using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class AuthAuditEventMap : IEntityTypeConfiguration<AuthAuditEvent>
	{
		public void Configure(EntityTypeBuilder<AuthAuditEvent> builder)
		{
			builder.ToTable("AuthAuditEvents");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.EventType).HasColumnType("NVARCHAR(64)").IsRequired(true);
			builder.Property(e => e.Outcome).HasColumnType("NVARCHAR(32)").IsRequired(true);
			builder.Property(e => e.UserId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.Email).HasColumnType("NVARCHAR(256)").IsRequired(false);
			builder.Property(e => e.ClientId).HasColumnType("NVARCHAR(128)").IsRequired(false);
			builder.Property(e => e.IpAddress).HasColumnType("NVARCHAR(64)").IsRequired(false);
			builder.Property(e => e.Detail).HasColumnType("NVARCHAR(1024)").IsRequired(false);

			builder.HasIndex(e => e.CreatedAt);
			builder.HasIndex(e => e.UserId);
			builder.HasIndex(e => e.EventType);
		}
	}
}
