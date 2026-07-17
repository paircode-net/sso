using System;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Server;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Middleware.AddServices
{
	public static class AddHardeningConfigurations
	{
		public static IServiceCollection AddSsoHardening(
			this IServiceCollection services,
			IConfiguration configuration,
			IHostEnvironment environment)
		{
			var options = configuration.GetSection(SsoHardeningOptions.SectionName).Get<SsoHardeningOptions>()
				?? new SsoHardeningOptions();

			if (environment.IsProduction())
			{
				// P-004 / D9 safer defaults when Production section omitted
				options.Database.AutoMigrate = configuration.GetValue("Sso:Database:AutoMigrate", false);
				if (!configuration.GetSection("Sso:Signing:UseDevelopmentCertificates").Exists())
				{
					options.Signing.UseDevelopmentCertificates = false;
				}
			}

			services.Configure<SsoHardeningOptions>(configuration.GetSection(SsoHardeningOptions.SectionName));
			services.AddSingleton(options);

			services.Configure<IdentityOptions>(identity =>
			{
				identity.Lockout.AllowedForNewUsers = options.Lockout.AllowedForNewUsers;
				identity.Lockout.MaxFailedAccessAttempts = options.Lockout.MaxFailedAccessAttempts;
				identity.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(options.Lockout.DefaultLockoutMinutes);
			});

			if (options.Cors.Enabled)
			{
				services.AddCors(cors =>
				{
					cors.AddPolicy("SsoCors", policy =>
					{
						policy
							.WithOrigins(options.Cors.AllowedOrigins ?? Array.Empty<string>())
							.AllowAnyHeader()
							.AllowAnyMethod()
							.AllowCredentials();
					});
				});
			}

			if (options.RateLimit.Enabled)
			{
				services.AddRateLimiter(rate =>
				{
					rate.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
					rate.OnRejected = (context, _) =>
					{
						SsoAuthMetrics.RecordRateLimited(context.HttpContext.Request.Path.Value);
						return ValueTask.CompletedTask;
					};
					rate.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
					{
						var path = httpContext.Request.Path.Value ?? string.Empty;
						var isAuthSurface =
							path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase)
							|| path.StartsWith("/connect/token", StringComparison.OrdinalIgnoreCase)
							|| path.StartsWith("/connect/authorize", StringComparison.OrdinalIgnoreCase);

						if (!isAuthSurface)
						{
							return RateLimitPartition.GetNoLimiter("open");
						}

						var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
						return RateLimitPartition.GetFixedWindowLimiter(
							key,
							_ => new FixedWindowRateLimiterOptions
							{
								PermitLimit = options.RateLimit.PermitLimit,
								Window = TimeSpan.FromSeconds(Math.Max(1, options.RateLimit.WindowSeconds)),
								QueueLimit = 0
							});
					});
				});
			}

			services.AddExternalIdentityProviders(options);

			return services;
		}

		public static IApplicationBuilder UseSsoHardening(this IApplicationBuilder app, SsoHardeningOptions options)
		{
			if (options.Cors.Enabled)
			{
				app.UseCors("SsoCors");
			}

			if (options.RateLimit.Enabled)
			{
				app.UseRateLimiter();
			}

			return app;
		}

		public static void ConfigureOpenIddictSigning(
			OpenIddictServerBuilder options,
			SsoHardeningOptions hardening,
			IHostEnvironment environment,
			ILogger logger)
		{
			var useDevCerts = hardening.Signing.UseDevelopmentCertificates || environment.IsDevelopment();
			if (useDevCerts)
			{
				options.AddDevelopmentEncryptionCertificate();
				options.AddDevelopmentSigningCertificate();
				logger.LogInformation("OpenIddict using development signing/encryption certificates.");
				return;
			}

			if (!SigningCertificateResolver.TryResolveProductionCertificate(
				hardening.Signing,
				logger,
				out var cert,
				out var source)
				|| cert is null)
			{
				throw new InvalidOperationException(
					"Signing:UseDevelopmentCertificates is false but no certificate is configured. " +
					"Set Signing:KeyVaultUri + Signing:KeyVaultCertificateName (F00010-D5) or Signing:CertificatePath.");
			}

			options.AddEncryptionCertificate(cert);
			options.AddSigningCertificate(cert);
			logger.LogInformation("OpenIddict using certificate from {Source}.", source);
		}
	}
}
