namespace SSO.Shared.Identity
{
	/// <summary>Consent prompt rules for AuthClients (feature 00007).</summary>
	public static class AuthClientConsentEvaluator
	{
		/// <summary>
		/// Returns true when the authorize pipeline should redirect to the Consent UI.
		/// </summary>
		public static bool ShouldPrompt(string? policy, bool hasValidAuthorization)
		{
			if (string.IsNullOrWhiteSpace(policy)
				|| string.Equals(policy, AuthClientConsentPolicies.Never, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			// Always | First — prompt until a valid remembered authorization exists.
			return !hasValidAuthorization;
		}

		/// <summary>
		/// Always+remember authorizations older than <paramref name="rememberDays"/> are treated as expired.
		/// </summary>
		public static bool IsRememberExpired(string? policy, DateTimeOffset? createdAtUtc, int rememberDays, DateTime utcNow)
		{
			if (!string.Equals(policy, AuthClientConsentPolicies.Always, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (createdAtUtc is null)
			{
				return false;
			}

			var days = rememberDays > 0 ? rememberDays : 180;
			return createdAtUtc.Value.UtcDateTime.AddDays(days) < utcNow;
		}
	}
}
