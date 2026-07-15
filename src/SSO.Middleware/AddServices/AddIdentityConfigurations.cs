using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Infrastructures.Services.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Middleware.AddServices
{
	public static class AddIdentityConfigurations
	{
		public static IServiceCollection AddIdentityFoundation(
			this IServiceCollection services,
			bool disableTransportSecurityRequirement = false)
		{
			services
				.AddIdentity<User, IdentityRole<Guid>>(options =>
				{
					options.Password.RequiredLength = 8;
					options.User.RequireUniqueEmail = true;
					options.SignIn.RequireConfirmedAccount = false;
				})
				.AddEntityFrameworkStores<IdentityDbContext>()
				.AddDefaultTokenProviders();

			services.AddScoped<IEffectivePermissionsResolver, EffectivePermissionsResolver>();
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

					options
						.AddDevelopmentEncryptionCertificate()
						.AddDevelopmentSigningCertificate();

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
	}
}
