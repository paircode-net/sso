using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SSO.Core.Domain.Identity.ClientProductBindings.Entity;

using System;



namespace SSO.Infrastructures.Data.Identity.EntityMappings

{

	public sealed class ClientProductBindingMap : IEntityTypeConfiguration<ClientProductBinding>

	{

		public void Configure(EntityTypeBuilder<ClientProductBinding> builder)

		{

			builder.ToTable("ClientProductBindings");



			builder.Property(p => p.Id)

				.HasColumnName("Id")

				.HasColumnType("UNIQUEIDENTIFIER")

				.ValueGeneratedOnAdd()

				.IsRequired(true);

			builder.HasKey(e => e.Id);



			builder.Property(e => e.ClientId)

				.HasColumnType("NVARCHAR(128)")

				.HasColumnName("ClientId")

				.IsRequired(true);

			builder.Property(e => e.ProductId)

				.HasColumnType("UNIQUEIDENTIFIER")

				.HasColumnName("ProductId")

				.IsRequired(true);



			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);

			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);

			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);

			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);



			builder.HasIndex(e => e.ClientId)

				.IsUnique()

				.HasFilter("[IsDeleted] = 0");

		}

	}

}

