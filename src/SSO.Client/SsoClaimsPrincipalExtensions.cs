using System.Security.Claims;
using SSO.Shared.Identity;

namespace SSO.Client
{
	public static class SsoClaimsPrincipalExtensions
	{
		public static Guid? GetSessionId(this ClaimsPrincipal user)
		{
			var value = user.FindFirst(SsoClaimTypes.SessionId)?.Value
				?? user.FindFirst("sid")?.Value;
			return Guid.TryParse(value, out var id) ? id : null;
		}

		public static Guid? GetOrganizationId(this ClaimsPrincipal user)
		{
			var value = user.FindFirst(SsoClaimTypes.OrganizationId)?.Value
				?? user.FindFirst("organization_id")?.Value;
			return Guid.TryParse(value, out var id) ? id : null;
		}

		public static Guid? GetBranchId(this ClaimsPrincipal user)
		{
			var value = user.FindFirst(SsoClaimTypes.BranchId)?.Value
				?? user.FindFirst("branch_id")?.Value;
			return Guid.TryParse(value, out var id) ? id : null;
		}

		public static string? GetPermissionVersion(this ClaimsPrincipal user)
			=> user.FindFirst(SsoClaimTypes.PermissionVersion)?.Value
				?? user.FindFirst("perm_ver")?.Value;

		public static IReadOnlyList<string> GetPermissions(this ClaimsPrincipal user)
			=> user.FindAll(SsoClaimTypes.Permissions)
				.Concat(user.FindAll("permissions"))
				.Select(c => c.Value)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(x => x)
				.ToList();

		public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
			=> user.GetPermissions()
				.Any(p => string.Equals(p, permissionCode, StringComparison.OrdinalIgnoreCase));

		public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissionCodes)
		{
			if (permissionCodes is null || permissionCodes.Length == 0)
			{
				return false;
			}

			var granted = user.GetPermissions().ToHashSet(StringComparer.OrdinalIgnoreCase);
			return permissionCodes.Any(p => granted.Contains(p));
		}

		public static Guid? GetSubjectId(this ClaimsPrincipal user)
		{
			var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
				?? user.FindFirst("sub")?.Value;
			return Guid.TryParse(value, out var id) ? id : null;
		}
	}
}
