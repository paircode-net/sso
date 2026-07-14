
using BAYSOFT.Core.Domain.Default._Context.Interfaces.Infrastructures.Data;
using BAYSOFT.Infrastructures.Data.Default;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BAYSOFT.Middleware.AddServices
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

			return services;
		}

		public static IApplicationBuilder UseMigrations(this IApplicationBuilder app)
		{
			using (var scope = app.ApplicationServices.CreateScope())
			{
                #region DefaultDbContext migrate
                var defaultDbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
                if (defaultDbContext.Database.IsRelational())
                    defaultDbContext.Database.Migrate();
                #endregion
			}

			return app;
		}
	}
}