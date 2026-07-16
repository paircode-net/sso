namespace SSO.Shared.Identity
{
	/// <summary>Coarse admin permission codes for <c>api/identity/*</c> (feature 00002).</summary>
	public static class SsoAdminPermissions
	{
		public const string Platform = "sso.admin.platform";
		public const string Org = "sso.admin.org";
		public const string AuditRead = "sso.admin.audit.read";
		public const string SessionsRevoke = "sso.admin.sessions.revoke";
		public const string Menus = "sso.admin.menus";
	}
}
