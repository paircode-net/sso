using SSO.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Infrastructures.Data.Default;
using SSO.Infrastructures.Data.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SSO.Shared.Identity;
using System;

namespace SSO.Middleware.AddServices
{
	public static class AddDbContextConfigurations
	{
		public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration)
		{
			#region Reader/Writer, DefaultDbContext and Connection
			var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

			services.AddTransient<IDefaultDbContextWriter, DefaultDbContextWriter>();
			services.AddTransient<IDefaultDbContextReader, DefaultDbContextReader>();

			services.AddDbContext<DefaultDbContext>(options =>
				options.UseSqlServer(
					defaultConnectionString,
					sql =>
					{
						sql.MigrationsAssembly(typeof(DefaultDbContext).Assembly.GetName().Name);
						sql.MigrationsHistoryTable("__EFMigrationsHistory", DefaultDbContext.Schema);
						sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
					}));
			#endregion

			#region Reader/Writer, IdentityDbContext and Connection
			var identityConnectionString = configuration.GetConnectionString("IdentityConnection")
				?? defaultConnectionString;

			services.AddTransient<IIdentityDbContextWriter, IdentityDbContextWriter>();
			services.AddTransient<IIdentityDbContextReader, IdentityDbContextReader>();

			services.AddDbContext<IdentityDbContext>(options =>
			{
				options.UseSqlServer(
					identityConnectionString,
					sql =>
					{
						sql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.GetName().Name);
						sql.MigrationsHistoryTable("__EFMigrationsHistory", IdentityDbContext.Schema);
						sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
					});

				options.UseOpenIddict<Guid>();
			});
			#endregion

			return services;
		}

		public static IServiceCollection AddDbContextsTest(this IServiceCollection services, IConfiguration configuration)
		{
			#region Reader/Writer, DefaultDbContext and InMemory
			services.AddTransient<IDefaultDbContextWriter, DefaultDbContextWriter>();
			services.AddTransient<IDefaultDbContextReader, DefaultDbContextReader>();

			services.AddDbContext<DefaultDbContext>(options =>
				options.UseInMemoryDatabase(nameof(DefaultDbContext), new InMemoryDatabaseRoot()),
				ServiceLifetime.Singleton);
			#endregion

			#region Reader/Writer, IdentityDbContext and InMemory
			services.AddTransient<IIdentityDbContextWriter, IdentityDbContextWriter>();
			services.AddTransient<IIdentityDbContextReader, IdentityDbContextReader>();

			services.AddDbContext<IdentityDbContext>(options =>
			{
				options.UseInMemoryDatabase(nameof(IdentityDbContext), new InMemoryDatabaseRoot());
				options.UseOpenIddict<Guid>();
			}, ServiceLifetime.Singleton);
			#endregion

			return services;
		}

		/// <summary>
		/// P-004: AutoMigrate defaults to true in Development; Production should set Sso:Database:AutoMigrate=false
		/// and apply migrations via pipeline (`dotnet ef database update`).
		/// </summary>
		public static IApplicationBuilder UseMigrations(
			this IApplicationBuilder app,
			IConfiguration configuration = null,
			IHostEnvironment environment = null)
		{
			configuration ??= app.ApplicationServices.GetRequiredService<IConfiguration>();
			environment ??= app.ApplicationServices.GetRequiredService<IHostEnvironment>();

			var hardening = configuration.GetSection(SsoHardeningOptions.SectionName).Get<SsoHardeningOptions>()
				?? new SsoHardeningOptions();

			var autoMigrate = configuration.GetValue<bool?>("Sso:Database:AutoMigrate")
				?? (environment.IsDevelopment() || hardening.Database.AutoMigrate);

			if (environment.IsProduction() && !configuration.GetSection("Sso:Database:AutoMigrate").Exists())
			{
				autoMigrate = false;
			}

			var seedOnStartup = configuration.GetValue("Sso:Database:SeedOnStartup", hardening.Database.SeedOnStartup);

			using (var scope = app.ApplicationServices.CreateScope())
			{
				var defaultDbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
				var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

				if (autoMigrate)
				{
					if (defaultDbContext.Database.IsRelational())
						defaultDbContext.Database.Migrate();

					if (identityDbContext.Database.IsRelational())
						identityDbContext.Database.Migrate();
				}

				if (seedOnStartup)
				{
					IdentitySeed.EnsureAsync(scope.ServiceProvider).GetAwaiter().GetResult();
				}
			}

			return app;
		}
	}
}
