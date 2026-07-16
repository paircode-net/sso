using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.Organizations.Entity;
using System;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class OrganizationMap : IEntityTypeConfiguration<Organization>
	{
		public void Configure(EntityTypeBuilder<Organization> builder)
		{
			builder.ToTable("Organizations");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Name)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("Name")
				.IsRequired(true);
			builder.Property(e => e.Code)
				.HasColumnType("NVARCHAR(64)")
				.HasColumnName("Code")
				.IsRequired(true);
			builder.Property(e => e.BranchAuthzInheritance)
				.HasColumnType("NVARCHAR(32)")
				.HasColumnName("BranchAuthzInheritance")
				.IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => e.Code)
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}
	}
}
