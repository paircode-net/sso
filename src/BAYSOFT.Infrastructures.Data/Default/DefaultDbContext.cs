
using BAYSOFT.Abstractions.Core.Domain.Interfaces.Infrastructures.Data;
using BAYSOFT.Core.Domain.Default.Samples.Entity;
using BAYSOFT.Infrastructures.Data.Default.EntityMappings;
using Microsoft.EntityFrameworkCore;

namespace BAYSOFT.Infrastructures.Data.Default
{
    public sealed class DefaultDbContext : DbContext
    {
        public static string Schema => "DefaultDb";

        public DbSet<Sample> Samples { get; set; }
        public DefaultDbContext() { }
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options){ }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.ApplyConfiguration(new SampleMap());
        }
    }
}