using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.RevokedSessions.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class RevokedSessionMap : IEntityTypeConfiguration<RevokedSession>
	{
		public void Configure(EntityTypeBuilder<RevokedSession> builder)
		{
			builder.ToTable("RevokedSessions");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.SessionId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.RevokedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.ExpiresAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.Reason).HasColumnType("nvarchar(256)").IsRequired(false);
			builder.Property(e => e.UserId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.ClientId).HasColumnType("nvarchar(128)").IsRequired(false);

			builder.HasIndex(e => e.SessionId).IsUnique();
			builder.HasIndex(e => e.ExpiresAt);
		}
	}
}
