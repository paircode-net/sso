using BAYSOFT.Core.Domain.Default.Samples.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BAYSOFT.Infrastructures.Data.Default.EntityMappings
{
    public sealed class SampleMap : IEntityTypeConfiguration<Sample>
    {
        public void Configure(EntityTypeBuilder<Sample> builder)
        {
            #region Map Table
            builder
                .ToTable("Samples");
            #endregion

            #region Map Primary Key
            builder
                .Property<Guid>(p => p.Id)
                .HasColumnName("Id")
                .HasColumnType("UNIQUEIDENTIFIER")
                .ValueGeneratedOnAdd()
                .IsRequired(true);

            builder
			    .HasKey(e => e.Id);
			#endregion

            #region Map Properties
            builder
                .Property<string>(e => e.Description)
                .HasColumnType("NVARCHAR(128)")
                .HasColumnName("Description")
                .IsRequired(true);
            #endregion

            #region Map Foreign Keys
            #endregion

            #region Map Relations
            #endregion
        }
    }
}