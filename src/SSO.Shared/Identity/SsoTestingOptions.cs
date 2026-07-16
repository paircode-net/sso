namespace SSO.Shared.Identity
{
	public sealed class SsoTestingOptions
	{
		public const string SectionName = "Sso:Testing";

		/// <summary>
		/// When true, authentication scheme <see cref="SsoTestingAuthDefaults.SchemeName"/> accepts
		/// <c>X-Test-Permissions</c> / <c>X-Test-Organization-Id</c> headers (tests only).
		/// </summary>
		public bool EnableTestAuth { get; set; }
	}

	public static class SsoTestingAuthDefaults
	{
		public const string SchemeName = "TestPermissions";
		public const string PermissionsHeader = "X-Test-Permissions";
		public const string OrganizationIdHeader = "X-Test-Organization-Id";
		public const string UserIdHeader = "X-Test-User-Id";
	}
}
