using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SSO.Client.Authorization;
using SSO.Shared.Identity;

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

			options.RevocationCheck ??= new SsoRevocationCheckOptions();

			services.Configure<SsoClientOptions>(_ =>
			{
				_.Authority = options.Authority;
				_.Audience = options.Audience;
				_.ClientId = options.ClientId;
				_.ClientSecret = options.ClientSecret;
				_.RequireHttpsMetadata = options.RequireHttpsMetadata;
				_.MapInboundClaims = options.MapInboundClaims;
				_.RevocationCheck = options.RevocationCheck;
			});

			services.AddSingleton<SsoSessionRevocationCache>();
			services.AddHttpClient("sso-revocation", client =>
			{
				client.BaseAddress = new Uri(options.Authority.TrimEnd('/') + "/");
				client.Timeout = TimeSpan.FromSeconds(5);
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

					if (options.RevocationCheck.Enabled)
					{
						jwt.Events = new JwtBearerEvents
						{
							OnTokenValidated = async context =>
							{
								var opts = context.HttpContext.RequestServices
									.GetRequiredService<IOptions<SsoClientOptions>>().Value;
								if (!opts.RevocationCheck.Enabled)
								{
									return;
								}

								var sid = context.Principal?.FindFirst(SsoClaimTypes.SessionId)?.Value
									?? context.Principal?.FindFirst("sid")?.Value;
								if (string.IsNullOrWhiteSpace(sid) || !Guid.TryParse(sid, out var sessionId))
								{
									return;
								}

								var cache = context.HttpContext.RequestServices
									.GetRequiredService<SsoSessionRevocationCache>();
								var factory = context.HttpContext.RequestServices
									.GetRequiredService<IHttpClientFactory>();

								var revoked = await cache.IsRevokedAsync(
									sessionId,
									opts,
									factory,
									context.HttpContext.RequestAborted);

								if (revoked == true)
								{
									context.Fail("Session has been revoked.");
								}
								else if (revoked is null && opts.RevocationCheck.FailClosed)
								{
									context.Fail("Session revocation status unavailable.");
								}
							}
						};
					}
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

	/// <summary>Caches sid → revoked for CacheSeconds (default 30s → SLA ≤ 60s).</summary>
	public sealed class SsoSessionRevocationCache
	{
		private readonly ConcurrentDictionary<Guid, CacheEntry> _entries = new();

		private sealed record CacheEntry(bool Revoked, DateTime ExpiresAt);

		public async Task<bool?> IsRevokedAsync(
			Guid sessionId,
			SsoClientOptions options,
			IHttpClientFactory httpClientFactory,
			CancellationToken cancellationToken)
		{
			var now = DateTime.UtcNow;
			if (_entries.TryGetValue(sessionId, out var cached) && cached.ExpiresAt > now)
			{
				return cached.Revoked;
			}

			try
			{
				var client = httpClientFactory.CreateClient("sso-revocation");
				var path = options.RevocationCheck.StatusPath.Replace(
					"{sid}",
					sessionId.ToString("D"),
					StringComparison.OrdinalIgnoreCase);
				var response = await client.GetAsync(path, cancellationToken);
				if (!response.IsSuccessStatusCode)
				{
					return null;
				}

				var body = await response.Content.ReadFromJsonAsync<StatusDto>(cancellationToken: cancellationToken);
				var revoked = body?.Revoked ?? false;
				var ttl = TimeSpan.FromSeconds(Math.Clamp(options.RevocationCheck.CacheSeconds, 1, 60));
				_entries[sessionId] = new CacheEntry(revoked, now.Add(ttl));
				return revoked;
			}
			catch
			{
				return null;
			}
		}

		private sealed class StatusDto
		{
			public bool Revoked { get; set; }
		}
	}
}
