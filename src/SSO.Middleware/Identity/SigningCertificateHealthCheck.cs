using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity
{
	/// <summary>
	/// Ready check: signing material is configured (dev certs, path, or Key Vault).
	/// </summary>
	public sealed class SigningCertificateHealthCheck : IHealthCheck
	{
		private readonly SsoHardeningOptions _options;
		private readonly IHostEnvironment _environment;

		public SigningCertificateHealthCheck(
			IOptions<SsoHardeningOptions> options,
			IHostEnvironment environment)
		{
			_options = options.Value;
			_environment = environment;
		}

		public Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default)
		{
			var useDev = _options.Signing.UseDevelopmentCertificates || _environment.IsDevelopment();
			if (SigningCertificateResolver.IsSigningConfigured(_options.Signing, useDev))
			{
				var detail = useDev
					? "development_certificates"
					: (!string.IsNullOrWhiteSpace(_options.Signing.KeyVaultUri)
						? "key_vault"
						: "certificate_path");
				return Task.FromResult(HealthCheckResult.Healthy($"Signing configured ({detail})."));
			}

			return Task.FromResult(HealthCheckResult.Unhealthy(
				"Signing:UseDevelopmentCertificates is false and no CertificatePath or Key Vault certificate is configured."));
		}
	}
}
