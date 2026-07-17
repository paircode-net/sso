using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Logging;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity
{
	/// <summary>
	/// Resolves OpenIddict signing/encryption certificates (dev, PFX path, or Key Vault — F00010-D5 / D9).
	/// </summary>
	public static class SigningCertificateResolver
	{
		public static bool TryResolveProductionCertificate(
			SigningOptions signing,
			ILogger logger,
			out X509Certificate2? certificate,
			out string source)
		{
			certificate = null;
			source = string.Empty;

			if (!string.IsNullOrWhiteSpace(signing.KeyVaultUri)
				&& !string.IsNullOrWhiteSpace(signing.KeyVaultCertificateName))
			{
				try
				{
					var client = new CertificateClient(
						new Uri(signing.KeyVaultUri.TrimEnd('/')),
						new DefaultAzureCredential());
					certificate = client
						.DownloadCertificateAsync(signing.KeyVaultCertificateName)
						.GetAwaiter()
						.GetResult();
					source = $"KeyVault:{signing.KeyVaultUri}/{signing.KeyVaultCertificateName}";
					logger.LogInformation(
						"Loaded OpenIddict signing certificate '{Name}' from Key Vault.",
						signing.KeyVaultCertificateName);
					return certificate is not null;
				}
				catch (Exception ex)
				{
					logger.LogError(
						ex,
						"Failed to download signing certificate '{Name}' from Key Vault {Uri}.",
						signing.KeyVaultCertificateName,
						signing.KeyVaultUri);
					throw;
				}
			}

			if (!string.IsNullOrWhiteSpace(signing.CertificatePath))
			{
				certificate = string.IsNullOrEmpty(signing.CertificatePassword)
					? X509CertificateLoader.LoadCertificateFromFile(signing.CertificatePath)
					: X509CertificateLoader.LoadPkcs12FromFile(
						signing.CertificatePath,
						signing.CertificatePassword);
				source = $"File:{signing.CertificatePath}";
				logger.LogInformation(
					"Loaded OpenIddict signing certificate from path {Path}.",
					signing.CertificatePath);
				return true;
			}

			return false;
		}

		public static bool IsSigningConfigured(SigningOptions signing, bool useDevelopmentCertificates) =>
			useDevelopmentCertificates
			|| (!string.IsNullOrWhiteSpace(signing.KeyVaultUri)
				&& !string.IsNullOrWhiteSpace(signing.KeyVaultCertificateName))
			|| !string.IsNullOrWhiteSpace(signing.CertificatePath);
	}
}
