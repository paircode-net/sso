namespace SSO.Shared.Identity
{
	public static class SsoClaimTypes
	{
		public const string OrganizationId = "organization_id";
		public const string BranchId = "branch_id";
		public const string Permissions = "permissions";
		public const string PermissionVersion = "perm_ver";
		/// <summary>Opaque etag for typed claim catalog/assignments (feature 00008 / F00008-D2).</summary>
		public const string ClaimVersion = "claim_ver";
		/// <summary>Stable session id for hot revocation (feature 00005).</summary>
		public const string SessionId = "sid";
	}

	public static class SsoGrantTypes
	{
		public const string SwitchContext = "urn:sso:params:oauth:grant-type:switch_context";
	}

	public static class SsoClients
	{
		public const string DevSpaClientId = "dev-product-spa";
		public const string DevServiceClientId = "dev-product-service";
		public const string DevServiceClientSecret = "dev-service-secret-change-me";
		public const string AdminApiClientId = "sso-admin-api";
		public const string AdminApiClientSecret = "sso-admin-secret-change-me";
	}
}
