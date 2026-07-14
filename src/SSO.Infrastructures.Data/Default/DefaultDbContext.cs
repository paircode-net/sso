
using BAYSOFT.Abstractions.Core.Domain.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Default.Samples.Entity;
using SSO.Infrastructures.Data.Default.EntityMappings;
using Microsoft.EntityFrameworkCore;

namespace SSO.Infrastructures.Data.Default
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