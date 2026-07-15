using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SSO.Infrastructures.Data.Identity;

namespace SSO.Tests.Helpers.Data.Identity
{
	internal static class IdentityDbContextExtensions
	{
		public static IdentityDbContext GetInMemoryIdentityDbContext(string databaseName = "IDENTITY_INMEM")
		{
			var options = new DbContextOptionsBuilder<IdentityDbContext>()
				.UseInMemoryDatabase(databaseName)
				.ConfigureWarnings(cw => cw.Ignore(InMemoryEventId.TransactionIgnoredWarning))
				.Options;

			var context = new IdentityDbContext(options);
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();
			return context;
		}

		public static IdentityDbContextReader GetDbContextReader(this IdentityDbContext context)
			=> new IdentityDbContextReader(context);

		public static IdentityDbContextWriter GetDbContextWriter(this IdentityDbContext context)
			=> new IdentityDbContextWriter(context);
	}
}
