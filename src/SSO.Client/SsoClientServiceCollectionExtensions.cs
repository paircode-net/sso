using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SSO.Client.Authorization;

namespace SSO.Client
{
	public static class SsoClientServiceCollectionExtensions
	{
		public static IServiceCollection AddSsoAuthentication(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<SsoClientOptions>(configuration.GetSection(SsoClientOptions.SectionName));
			var options = configuration.GetSection(SsoClientOptions.SectionName).Get<SsoClientOptions>()
				?? new SsoClientOptions();

			return services.AddSsoAuthentication(options);
		}

		public static IServiceCollection AddSsoAuthentication(
			this IServiceCollection services,
			SsoClientOptions options)
		{
			ArgumentNullException.ThrowIfNull(options);
			if (string.IsNullOrWhiteSpace(options.Authority))
			{
				throw new ArgumentException("SsoClient:Authority is required.", nameof(options));
			}

			services.Configure<SsoClientOptions>(_ =>
			{
				_.Authority = options.Authority;
				_.Audience = options.Audience;
				_.ClientId = options.ClientId;
				_.ClientSecret = options.ClientSecret;
				_.RequireHttpsMetadata = options.RequireHttpsMetadata;
				_.MapInboundClaims = options.MapInboundClaims;
			});

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(jwt =>
				{
					jwt.Authority = options.Authority;
					jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
					jwt.MapInboundClaims = options.MapInboundClaims;
					if (!string.IsNullOrWhiteSpace(options.Audience))
					{
						jwt.Audience = options.Audience;
					}

					jwt.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = !string.IsNullOrWhiteSpace(options.Audience),
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						NameClaimType = "name",
						RoleClaimType = "role"
					};
				});

			services.AddAuthorization();
			services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
			services.AddHttpClient<SsoTokenClient>(client =>
			{
				client.BaseAddress = new Uri(options.Authority.TrimEnd('/') + "/");
			});

			return services;
		}
	}
}
