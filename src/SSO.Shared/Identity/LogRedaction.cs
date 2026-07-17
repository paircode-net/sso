using System.Text.RegularExpressions;

namespace SSO.Shared.Identity
{
	/// <summary>
	/// Strips secrets from structured log payloads (Authorization, bearer tokens, passwords).
	/// </summary>
	public static partial class LogRedaction
	{
		public const string Redacted = "[REDACTED]";

		private static readonly string[] SensitiveHeaderNames =
		[
			"Authorization",
			"Cookie",
			"Set-Cookie",
			"X-Api-Key",
			"Proxy-Authorization"
		];

		private static readonly string[] SensitiveFieldNames =
		[
			"password",
			"passwd",
			"secret",
			"client_secret",
			"refresh_token",
			"access_token",
			"id_token",
			"token",
			"authorization"
		];

		public static bool IsSensitiveHeader(string name) =>
			SensitiveHeaderNames.Any(h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));

		public static bool IsSensitiveField(string name) =>
			SensitiveFieldNames.Any(f => string.Equals(f, name, StringComparison.OrdinalIgnoreCase));

		public static string RedactHeaderValue(string name, string? value) =>
			IsSensitiveHeader(name) ? Redacted : (value ?? string.Empty);

		public static string RedactMessage(string? message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return string.Empty;
			}

			var result = BearerTokenRegex().Replace(message, $"Authorization: {Redacted}");
			result = PasswordFieldRegex().Replace(result, $"\"$1\":\"{Redacted}\"");
			return result;
		}

		[GeneratedRegex(@"Authorization\s*:\s*Bearer\s+\S+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
		private static partial Regex BearerTokenRegex();

		[GeneratedRegex("\"(password|passwd|secret|client_secret|refresh_token|access_token|id_token)\"\\s*:\\s*\"[^\"]*\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
		private static partial Regex PasswordFieldRegex();
	}
}
