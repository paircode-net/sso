using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.UserSessions.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class UserSessionMap : IEntityTypeConfiguration<UserSession>
	{
		public void Configure(EntityTypeBuilder<UserSession> builder)
		{
			builder.ToTable("UserSessions");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.UserId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.ClientId).HasColumnType("nvarchar(128)").IsRequired(true);
			builder.Property(e => e.OrganizationId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.BranchId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);
			builder.Property(e => e.LastSeenAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.RevokedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.RevokeReason).HasColumnType("nvarchar(256)").IsRequired(false);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.UserId);
			builder.HasIndex(e => new { e.UserId, e.RevokedAt });
		}
	}
}
