using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SSO.Core.Domain.Identity.Roles.Entity;

using System;



namespace SSO.Infrastructures.Data.Identity.EntityMappings

{

	public sealed class RoleMap : IEntityTypeConfiguration<Role>

	{

		public void Configure(EntityTypeBuilder<Role> builder)

		{

			builder.ToTable("AuthRoles");



			builder.Property(p => p.Id)

				.HasColumnName("Id")

				.HasColumnType("UNIQUEIDENTIFIER")

				.ValueGeneratedOnAdd()

				.IsRequired(true);

			builder.HasKey(e => e.Id);



			builder.Property(e => e.Code)

				.HasColumnType("NVARCHAR(128)")

				.HasColumnName("Code")

				.IsRequired(true);

			builder.Property(e => e.Name)

				.HasColumnType("NVARCHAR(256)")

				.HasColumnName("Name")

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

