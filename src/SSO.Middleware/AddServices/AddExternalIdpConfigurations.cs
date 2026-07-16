using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Middleware.AddServices
{
	public static class AddExternalIdpConfigurations
	{
		public static IServiceCollection AddExternalIdentityProviders(
			this IServiceCollection services,
			SsoHardeningOptions options)
		{
			var entra = options.ExternalAuth?.Entra ?? new EntraOptions();
			var google = options.ExternalAuth?.Google ?? new GoogleOptions();
			var ldap = options.ExternalAuth?.Ldap ?? new LdapOptions();

			services.AddScoped<FederatedAccountService>();
			services.AddScoped<LdapGroupRoleSyncService>();

			if (entra.Enabled || google.Enabled)
			{
				var authBuilder = services.AddAuthentication();

				if (entra.Enabled)
				{
					if (string.IsNullOrWhiteSpace(entra.ClientId) || string.IsNullOrWhiteSpace(entra.ClientSecret))
					{
						throw new InvalidOperationException(
							"Sso:ExternalAuth:Entra is Enabled but ClientId/ClientSecret are missing. Use User Secrets / env vars.");
					}

					authBuilder.AddOpenIdConnect(AuthenticationSchemes.Entra, "Microsoft Entra ID", oidc =>
					{
						oidc.Authority = $"https://login.microsoftonline.com/{entra.TenantId}/v2.0";
						oidc.ClientId = entra.ClientId;
						oidc.ClientSecret = entra.ClientSecret;
						oidc.CallbackPath = entra.CallbackPath;
						oidc.ResponseType = OpenIdConnectResponseType.Code;
						oidc.SaveTokens = true;
						oidc.GetClaimsFromUserInfoEndpoint = true;
						oidc.Scope.Clear();
						oidc.Scope.Add("openid");
						oidc.Scope.Add("profile");
						oidc.Scope.Add("email");
						oidc.MapInboundClaims = false;
						oidc.TokenValidationParameters.NameClaimType = "name";
					});
				}

				if (google.Enabled)
				{
					if (string.IsNullOrWhiteSpace(google.ClientId) || string.IsNullOrWhiteSpace(google.ClientSecret))
					{
						throw new InvalidOperationException(
							"Sso:ExternalAuth:Google is Enabled but ClientId/ClientSecret are missing.");
					}

					authBuilder.AddOpenIdConnect(AuthenticationSchemes.Google, "Google", oidc =>
					{
						oidc.Authority = "https://accounts.google.com";
						oidc.ClientId = google.ClientId;
						oidc.ClientSecret = google.ClientSecret;
						oidc.CallbackPath = google.CallbackPath;
						oidc.ResponseType = OpenIdConnectResponseType.Code;
						oidc.SaveTokens = true;
						oidc.GetClaimsFromUserInfoEndpoint = true;
						oidc.Scope.Clear();
						oidc.Scope.Add("openid");
						oidc.Scope.Add("profile");
						oidc.Scope.Add("email");
						oidc.MapInboundClaims = false;
						oidc.TokenValidationParameters.NameClaimType = "name";
					});
				}
			}

			if (ldap.Enabled)
			{
				services.AddSingleton<ILdapAuthenticationService, LdapAuthenticationService>();
			}
			else
			{
				services.AddSingleton<ILdapAuthenticationService, DisabledLdapAuthenticationService>();
			}

			return services;
		}
	}

	public sealed class DisabledLdapAuthenticationService : ILdapAuthenticationService
	{
		public Task<LdapAuthResult> AuthenticateAsync(
			string username,
			string password,
			CancellationToken cancellationToken = default)
			=> Task.FromResult(LdapAuthResult.Fail("ldap_disabled"));
	}
}
