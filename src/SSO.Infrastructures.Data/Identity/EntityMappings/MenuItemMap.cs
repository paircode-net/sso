using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.MenuItems.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class MenuItemMap : IEntityTypeConfiguration<MenuItem>
	{
		public void Configure(EntityTypeBuilder<MenuItem> builder)
		{
			builder.ToTable("MenuItems");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.ProductId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("ProductId")
				.IsRequired(true);
			builder.Property(e => e.Code)
				.HasColumnType("NVARCHAR(64)")
				.HasColumnName("Code")
				.IsRequired(true);
			builder.Property(e => e.Title)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("Title")
				.IsRequired(true);
			builder.Property(e => e.Route)
				.HasColumnType("NVARCHAR(256)")
				.HasColumnName("Route")
				.IsRequired(true);
			builder.Property(e => e.PermissionCode)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("PermissionCode")
				.IsRequired(true);
			builder.Property(e => e.SortOrder)
				.HasColumnType("int")
				.HasColumnName("SortOrder")
				.IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.ProductId, e.Code })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}
	}
}
