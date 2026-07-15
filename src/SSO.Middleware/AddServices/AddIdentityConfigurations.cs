using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Middleware.AddServices
{
	public static class AddIdentityConfigurations
	{
		public static IServiceCollection AddIdentityFoundation(this IServiceCollection services)
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
						.AllowClientCredentialsFlow();

					options.RegisterScopes(
						Scopes.OpenId,
						Scopes.Email,
						Scopes.Profile,
						Scopes.Roles,
						Scopes.OfflineAccess);

					options
						.AddDevelopmentEncryptionCertificate()
						.AddDevelopmentSigningCertificate();

					options
						.UseAspNetCore()
						.EnableAuthorizationEndpointPassthrough()
						.EnableTokenEndpointPassthrough()
						.EnableEndSessionEndpointPassthrough()
						.EnableUserInfoEndpointPassthrough();
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
