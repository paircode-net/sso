namespace SSO.Shared.Identity
{
	public static class SsoClaimTypes
	{
		public const string OrganizationId = "organization_id";
		public const string BranchId = "branch_id";
		public const string Permissions = "permissions";
		public const string PermissionVersion = "perm_ver";
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
	}
}
