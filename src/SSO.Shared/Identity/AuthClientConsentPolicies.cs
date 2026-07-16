namespace SSO.Shared.Identity
{
	/// <summary>Consent policy for AuthClients (feature 00007).</summary>
	public static class AuthClientConsentPolicies
	{
		public const string Always = "Always";
		public const string First = "First";
		public const string Never = "Never";

		public static bool IsValid(string? value)
			=> value is Always or First or Never;
	}
}
