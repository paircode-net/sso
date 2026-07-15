using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Core.Domain.Identity.Branches.Entity;

namespace SSO.Infrastructures.Data.Identity.EntityMappings
{
	public sealed class BranchMap : IEntityTypeConfiguration<Branch>
	{
		public void Configure(EntityTypeBuilder<Branch> builder)
		{
			builder.ToTable("Branches");

			builder.Property(p => p.Id)
				.HasColumnName("Id")
				.HasColumnType("UNIQUEIDENTIFIER")
				.ValueGeneratedOnAdd()
				.IsRequired(true);
			builder.HasKey(e => e.Id);

			builder.Property(e => e.OrganizationId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("OrganizationId")
				.IsRequired(true);
			builder.Property(e => e.ParentBranchId)
				.HasColumnType("UNIQUEIDENTIFIER")
				.HasColumnName("ParentBranchId")
				.IsRequired(false);
			builder.Property(e => e.Name)
				.HasColumnType("NVARCHAR(128)")
				.HasColumnName("Name")
				.IsRequired(true);
			builder.Property(e => e.Code)
				.HasColumnType("NVARCHAR(64)")
				.HasColumnName("Code")
				.IsRequired(true);

			builder.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired(true);
			builder.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.DeletedAt).HasColumnType("datetime2").IsRequired(false);
			builder.Property(e => e.IsDeleted).HasColumnType("bit").IsRequired(true);

			builder.HasIndex(e => new { e.OrganizationId, e.Code })
				.IsUnique()
				.HasFilter("[IsDeleted] = 0");
		}
	}
}
