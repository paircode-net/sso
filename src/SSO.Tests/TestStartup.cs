using SSO.Infrastructures.Data.Identity;
using SSO.Middleware;
using SSO.Web.Api.Default;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Identity.Users.Entity;
using System.Reflection;

namespace SSO.Tests
{
	public class TestStartup
	{
		public TestStartup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			var assembly = typeof(TestStartup).GetTypeInfo().Assembly;

			services.AddMiddlewareTest(Configuration, assembly);

			services.AddControllersWithViews()
				.AddApplicationPart(typeof(SamplesController).Assembly);

			services.AddRazorPages()
				.AddApplicationPart(typeof(SamplesController).Assembly);

			services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/Account/Login";
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseHttpsRedirection();

			app.UseMiddlewareTest();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			using (var scope = app.ApplicationServices.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
				context.Database.EnsureCreated();
				IdentitySeed.EnsureAsync(scope.ServiceProvider).GetAwaiter().GetResult();
			}

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapRazorPages();
			});
		}
	}
}
