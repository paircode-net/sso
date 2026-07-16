using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Middleware.AddServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelWrapper.Middleware;
using SSO.Shared.Identity;
using System;
using System.Reflection;

namespace SSO.Middleware
{
	public static class Configurations
	{
		public static IServiceCollection AddMiddleware(
			this IServiceCollection services,
			IConfiguration configuration,
			Assembly presentationAssembly,
			IHostEnvironment environment = null)
		{
			services.AddLocalization();

			services.AddDbContexts(configuration);
			services.AddSpecifications();
			services.AddEntityValidations();
			services.AddDomainValidations();
			services.AddDomainServices();

			var assemblyApplication = AppDomain.CurrentDomain.Load("SSO.Core.Application");
			var assemblyDomain = AppDomain.CurrentDomain.Load("SSO.Core.Domain");
			var assemblyInfrastructuresServices = AppDomain.CurrentDomain.Load("SSO.Infrastructures.Services");
			services.AddMediatR(options => options.RegisterServicesFromAssemblies(assemblyApplication, assemblyDomain, assemblyInfrastructuresServices));

			services.AddModelWrapper()
				.AddDefaultReturnedCollectionSize(10)
				.AddMinimumReturnedCollectionSize(1)
				.AddMaximumReturnedCollectionSize(500)
				.AddQueryTermsMinimumSize(1)
				.AddByDefaultLoadComplexProperties(true)
				.AddEntityBaseType(typeof(DomainEntityBase))
				.AddSuppressedTerms(new string[] { "the" })
				.AddByDefaultInStringSeparator("|");

			services.AddInheritStringLocalizerFactory();

			services.AddIdentityFoundation(configuration, environment);
			if (environment != null)
			{
				services.AddSsoHardening(configuration, environment);
			}

			return services;
		}

		public static IApplicationBuilder UseMiddleware(this IApplicationBuilder app)
		{
			var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
			var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
			var hardening = app.ApplicationServices.GetService<SsoHardeningOptions>() ?? new SsoHardeningOptions();

			app.UseMigrations(configuration, environment);
			app.UseSsoHardening(hardening);

			var supportedCultures = new string[] { "pt-BR" };

			var localizationOptions = new RequestLocalizationOptions()
				.SetDefaultCulture(supportedCultures[0])
				.AddSupportedCultures(supportedCultures)
				.AddSupportedUICultures(supportedCultures);

			app.UseRequestLocalization(localizationOptions);

			app.UseAuthentication();
			app.UseAuthorization();

			return app;
		}

		#region TESTS
		public static IServiceCollection AddMiddlewareTest(this IServiceCollection services, IConfiguration configuration, Assembly presentationAssembly)
		{
			services.AddLocalization();

			services.AddDbContextsTest(configuration);
			services.AddSpecifications();
			services.AddEntityValidations();
			services.AddDomainValidations();
			services.AddDomainServices();

			var assemblyApplication = AppDomain.CurrentDomain.Load("SSO.Core.Application");
			var assemblyDomain = AppDomain.CurrentDomain.Load("SSO.Core.Domain");
			var assemblyInfrastructuresServices = AppDomain.CurrentDomain.Load("SSO.Infrastructures.Services");
			services.AddMediatR(options => options.RegisterServicesFromAssemblies(assemblyApplication, assemblyDomain, assemblyInfrastructuresServices));

			services.AddModelWrapper()
				.AddDefaultReturnedCollectionSize(10)
				.AddMinimumReturnedCollectionSize(1)
				.AddMaximumReturnedCollectionSize(100)
				.AddQueryTermsMinimumSize(3)
				.AddSuppressedTerms(new string[] { "the" });

			services.AddInheritStringLocalizerFactory();

			services.AddIdentityFoundation(configuration, environment: null, disableTransportSecurityRequirement: true);

			// Integration tests authenticate admin APIs via X-Test-Permissions (never enable in Production).
			services.Configure<SsoTestingOptions>(options => options.EnableTestAuth = true);

			var hardening = configuration.GetSection(SsoHardeningOptions.SectionName).Get<SsoHardeningOptions>()
				?? new SsoHardeningOptions
				{
					RateLimit = { Enabled = true, PermitLimit = 1000, WindowSeconds = 60 },
					Cors = { Enabled = true },
					Signing = { UseDevelopmentCertificates = true }
				};
			services.AddSingleton(hardening);
			if (hardening.Cors.Enabled)
			{
				services.AddCors(cors => cors.AddPolicy("SsoCors", p =>
					p.WithOrigins(hardening.Cors.AllowedOrigins ?? Array.Empty<string>())
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials()));
			}

			if (hardening.RateLimit.Enabled)
			{
				services.AddRateLimiter(_ => { });
			}

			return services;
		}

		public static IApplicationBuilder UseMiddlewareTest(this IApplicationBuilder app)
		{
			var supportedCultures = new string[] { "en-US", "pt-BR" };

			var localizationOptions = new RequestLocalizationOptions()
				.SetDefaultCulture(supportedCultures[0])
				.AddSupportedCultures(supportedCultures)
				.AddSupportedUICultures(supportedCultures);

			app.UseRequestLocalization(localizationOptions);

			return app;
		}
		#endregion
	}
}
