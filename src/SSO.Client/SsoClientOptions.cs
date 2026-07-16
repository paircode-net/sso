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
	}
}
