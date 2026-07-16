using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity.Authorization
{
	/// <summary>
	/// Test-only authentication via headers. Disabled unless <see cref="SsoTestingOptions.EnableTestAuth"/>.
	/// </summary>
	public sealed class TestPermissionsAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		private readonly SsoTestingOptions _testing;

		public TestPermissionsAuthHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			IOptions<SsoTestingOptions> testing)
			: base(options, logger, encoder)
		{
			_testing = testing.Value ?? new SsoTestingOptions();
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (!_testing.EnableTestAuth)
			{
				return Task.FromResult(AuthenticateResult.NoResult());
			}

			if (!Request.Headers.TryGetValue(SsoTestingAuthDefaults.PermissionsHeader, out var permissionsHeader)
				|| string.IsNullOrWhiteSpace(permissionsHeader))
			{
				return Task.FromResult(AuthenticateResult.NoResult());
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, ResolveUserId()),
				new Claim("sub", ResolveUserId())
			};

			foreach (var permission in permissionsHeader.ToString()
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			{
				claims.Add(new Claim(SsoClaimTypes.Permissions, permission));
			}

			if (Request.Headers.TryGetValue(SsoTestingAuthDefaults.OrganizationIdHeader, out var orgHeader)
				&& Guid.TryParse(orgHeader.ToString(), out var orgId))
			{
				claims.Add(new Claim(SsoClaimTypes.OrganizationId, orgId.ToString("D")));
			}

			var identity = new ClaimsIdentity(claims, SsoTestingAuthDefaults.SchemeName);
			var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SsoTestingAuthDefaults.SchemeName);
			return Task.FromResult(AuthenticateResult.Success(ticket));
		}

		private string ResolveUserId()
		{
			if (Request.Headers.TryGetValue(SsoTestingAuthDefaults.UserIdHeader, out var userHeader)
				&& !string.IsNullOrWhiteSpace(userHeader))
			{
				return userHeader.ToString();
			}

			return Guid.Empty.ToString("D");
		}
	}
}
