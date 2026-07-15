using System.Collections.Generic;

namespace SSO.Shared.Identity
{
	public sealed class SsoHardeningOptions
	{
		public const string SectionName = "Sso";

		public DatabaseOptions Database { get; set; } = new();
		public CorsOptions Cors { get; set; } = new();
		public RateLimitOptions RateLimit { get; set; } = new();
		public LockoutOptions Lockout { get; set; } = new();
		public SigningOptions Signing { get; set; } = new();
		public ExternalAuthOptions ExternalAuth { get; set; } = new();
	}

	public sealed class DatabaseOptions
	{
		/// <summary>P-004: when false, startup does not call Migrate() (use pipeline).</summary>
		public bool AutoMigrate { get; set; } = true;
		public bool SeedOnStartup { get; set; } = true;
	}

	public sealed class CorsOptions
	{
		public bool Enabled { get; set; } = true;
		public string[] AllowedOrigins { get; set; } = new[] { "https://localhost:5001", "https://localhost:3000" };
	}

	public sealed class RateLimitOptions
	{
		public bool Enabled { get; set; } = true;
		public int PermitLimit { get; set; } = 60;
		public int WindowSeconds { get; set; } = 60;
	}

	public sealed class LockoutOptions
	{
		public bool AllowedForNewUsers { get; set; } = true;
		public int MaxFailedAccessAttempts { get; set; } = 5;
		public int DefaultLockoutMinutes { get; set; } = 5;
	}

	public sealed class SigningOptions
	{
		/// <summary>When true, use OpenIddict development certificates (never for real production).</summary>
		public bool UseDevelopmentCertificates { get; set; } = true;
		/// <summary>Optional cert path (PFX) for production signing — prefer Key Vault in real ops.</summary>
		public string CertificatePath { get; set; }
		public string CertificatePassword { get; set; }
	}

	public sealed class ExternalAuthOptions
	{
		public EntraOptions Entra { get; set; } = new();
		public GoogleOptions Google { get; set; } = new();
		public LdapOptions Ldap { get; set; } = new();
	}

	public sealed class EntraOptions
	{
		public bool Enabled { get; set; }
		public string TenantId { get; set; } = "common";
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string CallbackPath { get; set; } = "/signin-entra";
	}

	public sealed class GoogleOptions
	{
		public bool Enabled { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string CallbackPath { get; set; } = "/signin-google";
	}

	public sealed class LdapOptions
	{
		public bool Enabled { get; set; }
		public string Host { get; set; }
		public int Port { get; set; } = 389;
		public string BaseDn { get; set; }
	}
}
