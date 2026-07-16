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

		public static string? GetClaimVersion(this ClaimsPrincipal user)
			=> user.FindFirst(SsoClaimTypes.ClaimVersion)?.Value
				?? user.FindFirst("claim_ver")?.Value;

		/// <summary>
		/// Reads a typed claim by catalog code (JWT type is sso_c_{code}).
		/// Prefer permissions for route gates — typed claims are attributes.
		/// </summary>
		public static string? GetTypedClaim(this ClaimsPrincipal user, string code)
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				return null;
			}

			var jwtType = TypedClaimNames.ToJwtType(code);
			return user.FindFirst(jwtType)?.Value;
		}

		public static T? GetTypedClaim<T>(this ClaimsPrincipal user, string code)
		{
			var raw = user.GetTypedClaim(code);
			if (raw is null)
			{
				return default;
			}

			var target = typeof(T);
			if (target == typeof(string))
			{
				return (T)(object)raw;
			}

			if (target == typeof(bool) || target == typeof(bool?))
			{
				if (bool.TryParse(raw, out var b))
				{
					return (T)(object)b;
				}

				return default;
			}

			if (target == typeof(int) || target == typeof(int?))
			{
				if (int.TryParse(raw, out var i))
				{
					return (T)(object)i;
				}

				return default;
			}

			throw new NotSupportedException($"GetTypedClaim<{target.Name}> is not supported; use string/int/bool.");
		}

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
