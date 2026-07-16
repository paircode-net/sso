namespace SSO.Client
{
	public sealed class SsoClientOptions
	{
		public const string SectionName = "SsoClient";

		/// <summary>OIDC authority (e.g. https://localhost:5001).</summary>
		public string Authority { get; set; } = string.Empty;

		/// <summary>Expected audience / API resource. Often equals ClientId for user tokens.</summary>
		public string? Audience { get; set; }

		public string ClientId { get; set; } = string.Empty;

		/// <summary>Only for confidential clients (BFF / service). Never use in SPA.</summary>
		public string? ClientSecret { get; set; }

		public bool RequireHttpsMetadata { get; set; } = true;

		/// <summary>Map inbound JWT claim types (keeps short names like permissions).</summary>
		public bool MapInboundClaims { get; set; } = false;

		/// <summary>Hot revocation check against AS (feature 00005). On by default; opt-out disables SLA.</summary>
		public SsoRevocationCheckOptions RevocationCheck { get; set; } = new();
	}

	public sealed class SsoRevocationCheckOptions
	{
		/// <summary>When true (default), JwtBearer OnTokenValidated queries session status.</summary>
		public bool Enabled { get; set; } = true;

		/// <summary>Local cache TTL for sid status. Keep ≤ 60s to meet SLA (default 30s).</summary>
		public int CacheSeconds { get; set; } = 30;

		/// <summary>Relative path on the authority for status checks.</summary>
		public string StatusPath { get; set; } = "api/identity/sessions/{sid}/status";

		/// <summary>
		/// When the status endpoint is unreachable: true = fail closed (reject), false = fail open (allow).
		/// Default fail-open for availability; set true for high-security.
		/// </summary>
		public bool FailClosed { get; set; } = false;
	}
}
