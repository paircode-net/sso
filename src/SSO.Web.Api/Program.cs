using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using SSO.Middleware;
using SSO.Middleware.AddServices;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
	.Enrich.FromLogContext()
	.Enrich.WithEnvironmentName()
	.Enrich.WithMachineName()
	.Enrich.WithProperty("Application", "SSO")
	.WriteTo.Console()
	.CreateBootstrapLogger();

try
{
	var builder = WebApplication.CreateBuilder(args);

	builder.Host.UseSerilog((context, services, configuration) =>
	{
		configuration
			.ReadFrom.Configuration(context.Configuration)
			.ReadFrom.Services(services)
			.Enrich.FromLogContext()
			.Enrich.WithEnvironmentName()
			.Enrich.WithMachineName()
			.Enrich.WithProperty("Application", "SSO")
			.WriteTo.Console();

		var obs = context.Configuration.GetSection(SsoObservabilityOptions.SectionName)
			.Get<SsoObservabilityOptions>();
		if (obs is not null
			&& string.Equals(obs.Exporter, SsoObservabilityExporters.Otlp, StringComparison.OrdinalIgnoreCase)
			&& !string.IsNullOrWhiteSpace(obs.OtlpEndpoint))
		{
			configuration.WriteTo.OpenTelemetry(options =>
			{
				options.Endpoint = obs.OtlpEndpoint;
				options.ResourceAttributes = new Dictionary<string, object>
				{
					["service.name"] = "sso"
				};
			});
		}
	});

	var keyVaultUri = builder.Configuration["Sso:Signing:KeyVaultUri"]
		?? builder.Configuration["Sso:KeyVault:Uri"];
	if (!string.IsNullOrWhiteSpace(keyVaultUri) && !builder.Environment.IsDevelopment())
	{
		builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
		Log.Information("Configuration loaded from Azure Key Vault {Uri}", keyVaultUri);
	}

	builder.Services.AddMiddleware(builder.Configuration, typeof(Program).Assembly, builder.Environment);
	builder.Services.AddControllersWithViews();
	builder.Services.AddRazorPages(options =>
	{
		options.Conventions.AddAreaFolderApplicationModelConvention(
			"Admin",
			"/",
			model => model.Filters.Add(new ServiceFilterAttribute(typeof(AdminPortalPageFilter))));
	});
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen();

	builder.Services.ConfigureApplicationCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.LogoutPath = "/Account/Login";
		options.AccessDeniedPath = "/Account/Login";
	});

	var app = builder.Build();

	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI();
	}

	app.UseHttpsRedirection();
	app.UseStaticFiles();

	var obsOptions = app.Services.GetService<SsoObservabilityOptions>() ?? new SsoObservabilityOptions();
	if (obsOptions.SerilogRequestLogging)
	{
		app.UseSerilogRequestLogging(options =>
		{
			options.MessageTemplate =
				"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
			options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
			{
				diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
				diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
				// Never enrich with Authorization / cookies / raw query secrets.
				foreach (var header in httpContext.Request.Headers)
				{
					if (LogRedaction.IsSensitiveHeader(header.Key))
					{
						continue;
					}
				}
			};
		});
	}

	app.UseMiddleware();

	app.MapControllers();
	app.MapRazorPages();
	app.MapSsoHealthEndpoints();

	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "SSO host terminated unexpectedly");
	throw;
}
finally
{
	Log.CloseAndFlush();
}
