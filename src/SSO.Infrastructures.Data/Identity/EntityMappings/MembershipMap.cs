using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.Memberships.Entity;
using System;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class MembershipMap : IEntityTypeConfiguration<Membership>
	{
		public void Configure(EntityTypeBuilder<Membership> builder)
		{
			builder.ToTable("Memberships");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.UserId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("UserId")
				.IsRequired(true);
			builder.Property(e => e.OrganizationId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("OrganizationId")
				.IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.UserId, e.OrganizationId })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}
	}
}
