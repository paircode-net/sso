using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Infrastructures.Services.Identity;
using SSO.Middleware.Identity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Middleware.AddServices
{
	public static class AddIdentityConfigurations
	{
		public static IServiceCollection AddIdentityFoundation(
			this IServiceCollection services,
			IConfiguration configuration = null,
			IHostEnvironment environment = null,
			bool disableTransportSecurityRequirement = false)
		{
			var hardening = configuration?.GetSection(SsoHardeningOptions.SectionName).Get<SsoHardeningOptions>()
				?? new SsoHardeningOptions();

			var testing = configuration?.GetSection(SsoTestingOptions.SectionName).Get<SsoTestingOptions>()
				?? new SsoTestingOptions();
			services.Configure<SsoTestingOptions>(options =>
			{
				options.EnableTestAuth = testing.EnableTestAuth;
			});

			services.AddHttpContextAccessor();
			services.AddScoped<ICurrentAdminContext, CurrentAdminContext>();
			services.AddScoped<IAdminPortalContextService, AdminPortalContextService>();
			services.AddScoped<AdminPortalPageFilter>();
			services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
			services.AddAuthorization();
			services.AddDistributedMemoryCache();
			services.AddSession(options =>
			{
				options.Cookie.Name = ".SSO.AdminSession";
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			services
				.AddIdentity<User, IdentityRole<Guid>>(options =>
				{
					options.Password.RequiredLength = 8;
					options.User.RequireUniqueEmail = true;
					options.SignIn.RequireConfirmedAccount = true;
					options.Lockout.AllowedForNewUsers = hardening.Lockout.AllowedForNewUsers;
					options.Lockout.MaxFailedAccessAttempts = hardening.Lockout.MaxFailedAccessAttempts;
					options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(hardening.Lockout.DefaultLockoutMinutes);
				})
				.AddEntityFrameworkStores<IdentityDbContext>()
				.AddDefaultTokenProviders();

			services.AddAuthentication()
				.AddScheme<AuthenticationSchemeOptions, TestPermissionsAuthHandler>(
					SsoTestingAuthDefaults.SchemeName,
					_ => { });

			services.AddScoped<IEffectivePermissionsResolver, EffectivePermissionsResolver>();
			services.AddScoped<IPermissionPolicyVersionProvider, PermissionPolicyVersionProvider>();
			services.AddScoped<IEffectiveMenusResolver, EffectiveMenusResolver>();
			services.AddScoped<TokenClaimsFactory>();

			services.AddOpenIddict()
				.AddCore(options =>
				{
					options.UseEntityFrameworkCore()
						.UseDbContext<IdentityDbContext>()
						.ReplaceDefaultEntities<Guid>();
				})
				.AddServer(options =>
				{
					options
						.SetAuthorizationEndpointUris("connect/authorize")
						.SetTokenEndpointUris("connect/token")
						.SetEndSessionEndpointUris("connect/logout")
						.SetUserInfoEndpointUris("connect/userinfo")
						.SetIntrospectionEndpointUris("connect/introspect")
						.SetRevocationEndpointUris("connect/revoke");

					options
						.AllowAuthorizationCodeFlow()
						.RequireProofKeyForCodeExchange()
						.AllowRefreshTokenFlow()
						.AllowClientCredentialsFlow()
						.AllowCustomFlow(SsoGrantTypes.SwitchContext);

					options.RegisterScopes(
						Scopes.OpenId,
						Scopes.Email,
						Scopes.Profile,
						Scopes.Roles,
						Scopes.OfflineAccess);

					options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
					options.SetIdentityTokenLifetime(TimeSpan.FromMinutes(15));
					options.SetRefreshTokenLifetime(TimeSpan.FromDays(14));

					options.DisableAccessTokenEncryption();

					using var loggerFactory = LoggerFactory.Create(_ => { });
					var logger = loggerFactory.CreateLogger("OpenIddict.Signing");
					var hostEnv = environment ?? new HostingEnvironmentStub();
					AddHardeningConfigurations.ConfigureOpenIddictSigning(options, hardening, hostEnv, logger);

					var aspNetCore = options
						.UseAspNetCore()
						.EnableAuthorizationEndpointPassthrough()
						.EnableTokenEndpointPassthrough()
						.EnableEndSessionEndpointPassthrough()
						.EnableUserInfoEndpointPassthrough();

					if (disableTransportSecurityRequirement)
					{
						aspNetCore.DisableTransportSecurityRequirement();
					}
				})
				.AddValidation(options =>
				{
					options.UseLocalServer();
					options.UseAspNetCore();
				});

			return services;
		}

		private sealed class HostingEnvironmentStub : IHostEnvironment
		{
			public string EnvironmentName { get; set; } = Environments.Development;
			public string ApplicationName { get; set; } = "SSO";
			public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
			public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
				= new Microsoft.Extensions.FileProviders.NullFileProvider();
		}
	}
}
