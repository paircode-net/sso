using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class OrganizationInviteMap : IEntityTypeConfiguration<OrganizationInvite>
	{
		public void Configure(EntityTypeBuilder<OrganizationInvite> builder)
		{
			builder.ToTable("OrganizationInvites");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.OrganizationId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.Email).HasColumnType("nvarchar(256)").IsRequired(true);
			builder.Property(e => e.TokenHash).HasColumnType("nvarchar(128)").IsRequired(true);
			builder.Property(e => e.Status).HasColumnType("nvarchar(32)").IsRequired(true);
			builder.Property(e => e.ExpiresAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.InvitedByUserId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(true);
			builder.Property(e => e.RespondedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.AcceptedUserId).HasColumnType("UNIQUEIDENTIFIER").IsRequired(false);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.TokenHash).IsUnique().HasFilter("[IsDeleted] = 0");
			builder.HasIndex(e => new { e.OrganizationId, e.Email, e.Status });
		}
	}
}
