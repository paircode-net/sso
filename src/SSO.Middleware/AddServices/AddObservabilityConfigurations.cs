using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Middleware.AddServices
{
	public static class AddObservabilityConfigurations
	{
		public const string LiveTag = "live";
		public const string ReadyTag = "ready";

		public static IServiceCollection AddSsoObservability(
			this IServiceCollection services,
			IConfiguration configuration,
			IHostEnvironment environment)
		{
			var options = configuration.GetSection(SsoObservabilityOptions.SectionName).Get<SsoObservabilityOptions>()
				?? configuration.GetSection(SsoHardeningOptions.SectionName).Get<SsoHardeningOptions>()?.Observability
				?? new SsoObservabilityOptions();

			if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(options.Exporter))
			{
				options.Exporter = SsoObservabilityExporters.Console;
			}

			services.Configure<SsoObservabilityOptions>(opt =>
			{
				opt.Enabled = options.Enabled;
				opt.Exporter = options.Exporter;
				opt.OtlpEndpoint = options.OtlpEndpoint;
				opt.AzureMonitorConnectionString = options.AzureMonitorConnectionString
					?? configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
				opt.TracingEnabled = options.TracingEnabled;
				opt.MetricsEnabled = options.MetricsEnabled;
				opt.TracingSampleRatio = options.TracingSampleRatio;
				opt.SerilogRequestLogging = options.SerilogRequestLogging;
			});
			services.AddSingleton(options);

			services.AddHealthChecks()
				.AddDbContextCheck<IdentityDbContext>("identity_db", tags: new[] { ReadyTag })
				.AddCheck<SigningCertificateHealthCheck>("signing_cert", tags: new[] { ReadyTag });

			services.AddSingleton<SigningCertificateHealthCheck>();

			if (!options.Enabled)
			{
				return services;
			}

			var resource = ResourceBuilder.CreateDefault()
				.AddService(
					serviceName: "sso",
					serviceVersion: typeof(AddObservabilityConfigurations).Assembly.GetName().Version?.ToString() ?? "1.0.0")
				.AddAttributes(new[]
				{
					new KeyValuePair<string, object>("deployment.environment", environment.EnvironmentName)
				});

			services.AddOpenTelemetry()
				.ConfigureResource(r => r.AddService("sso"))
				.WithTracing(tracing =>
				{
					if (!options.TracingEnabled)
					{
						return;
					}

					tracing
						.SetResourceBuilder(resource)
						.AddAspNetCoreInstrumentation(instr =>
						{
							instr.Filter = ctx =>
							{
								var path = ctx.Request.Path.Value ?? string.Empty;
								return path.StartsWith("/connect", StringComparison.OrdinalIgnoreCase)
									|| path.StartsWith("/api/identity", StringComparison.OrdinalIgnoreCase)
									|| path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase)
									|| path.StartsWith("/health", StringComparison.OrdinalIgnoreCase);
							};
						})
						.AddHttpClientInstrumentation()
						.SetSampler(new TraceIdRatioBasedSampler(
							Math.Clamp(options.TracingSampleRatio, 0, 1)));

					AddTraceExporter(tracing, options, configuration);
				})
				.WithMetrics(metrics =>
				{
					if (!options.MetricsEnabled)
					{
						return;
					}

					metrics
						.SetResourceBuilder(resource)
						.AddAspNetCoreInstrumentation()
						.AddRuntimeInstrumentation()
						.AddMeter(SsoAuthMetrics.MeterName);

					AddMetricExporter(metrics, options, configuration);
				});

			return services;
		}

		public static IEndpointRouteBuilder MapSsoHealthEndpoints(this IEndpointRouteBuilder endpoints)
		{
			endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
			{
				Predicate = _ => false,
				ResponseWriter = WriteMinimalAsync
			});

			endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
			{
				Predicate = check => check.Tags.Contains(ReadyTag),
				ResponseWriter = WriteMinimalAsync
			});

			return endpoints;
		}

		public static IApplicationBuilder UseSsoObservabilityEnrichment(this IApplicationBuilder app)
		{
			app.Use(async (context, next) =>
			{
				Activity.Current?.SetTag("sso.request_id", context.TraceIdentifier);
				var logger = context.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory))
					as Microsoft.Extensions.Logging.ILoggerFactory;
				var scoped = logger?.CreateLogger("SSO.Request");
				using (scoped?.BeginScope(new Dictionary<string, object?>
				{
					["RequestId"] = context.TraceIdentifier,
					["TraceId"] = Activity.Current?.TraceId.ToString() ?? string.Empty
				}))
				{
					await next();
				}
			});

			return app;
		}

		private static void AddTraceExporter(
			TracerProviderBuilder tracing,
			SsoObservabilityOptions options,
			IConfiguration configuration)
		{
			var exporter = options.Exporter ?? SsoObservabilityExporters.Console;
			if (string.Equals(exporter, SsoObservabilityExporters.None, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (string.Equals(exporter, SsoObservabilityExporters.Otlp, StringComparison.OrdinalIgnoreCase))
			{
				tracing.AddOtlpExporter(otlp =>
				{
					if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
					{
						otlp.Endpoint = new Uri(options.OtlpEndpoint);
					}
				});
				return;
			}

			if (string.Equals(exporter, SsoObservabilityExporters.AzureMonitor, StringComparison.OrdinalIgnoreCase))
			{
				var cs = options.AzureMonitorConnectionString
					?? configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
				if (!string.IsNullOrWhiteSpace(cs))
				{
					tracing.AddAzureMonitorTraceExporter(o => o.ConnectionString = cs);
				}

				return;
			}

			tracing.AddConsoleExporter();
		}

		private static void AddMetricExporter(
			MeterProviderBuilder metrics,
			SsoObservabilityOptions options,
			IConfiguration configuration)
		{
			var exporter = options.Exporter ?? SsoObservabilityExporters.Console;
			if (string.Equals(exporter, SsoObservabilityExporters.None, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (string.Equals(exporter, SsoObservabilityExporters.Otlp, StringComparison.OrdinalIgnoreCase))
			{
				metrics.AddOtlpExporter(otlp =>
				{
					if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
					{
						otlp.Endpoint = new Uri(options.OtlpEndpoint);
					}
				});
				return;
			}

			if (string.Equals(exporter, SsoObservabilityExporters.AzureMonitor, StringComparison.OrdinalIgnoreCase))
			{
				var cs = options.AzureMonitorConnectionString
					?? configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
				if (!string.IsNullOrWhiteSpace(cs))
				{
					metrics.AddAzureMonitorMetricExporter(o => o.ConnectionString = cs);
				}

				return;
			}

			metrics.AddConsoleExporter();
		}

		private static async Task WriteMinimalAsync(HttpContext context, HealthReport report)
		{
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsJsonAsync(new
			{
				status = report.Status.ToString(),
				checks = report.Entries.Select(e => new
				{
					name = e.Key,
					status = e.Value.Status.ToString(),
					description = e.Value.Description
				})
			});
		}
	}
}
