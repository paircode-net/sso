using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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
						oidc.Scope.Clear();
						oidc.Scope.Add("openid");
						oidc.Scope.Add("profile");
						oidc.Scope.Add("email");
					});
				}
			}

			if (ldap.Enabled)
			{
				services.AddSingleton<ILdapAuthenticationStub, LdapAuthenticationStub>();
			}

			return services;
		}
	}

	public interface ILdapAuthenticationStub
	{
		Task<bool> ValidateCredentialsAsync(string username, string password);
	}

	/// <summary>Placeholder until real LDAP/AD bind is implemented (D7).</summary>
	public sealed class LdapAuthenticationStub : ILdapAuthenticationStub
	{
		private readonly ILogger<LdapAuthenticationStub> _logger;

		public LdapAuthenticationStub(ILogger<LdapAuthenticationStub> logger)
		{
			_logger = logger;
		}

		public Task<bool> ValidateCredentialsAsync(string username, string password)
		{
			_logger.LogWarning("LDAP IdP stub invoked for {User}; not implemented.", username);
			return Task.FromResult(false);
		}
	}
}
