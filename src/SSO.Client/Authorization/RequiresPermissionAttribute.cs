using Microsoft.AspNetCore.Authorization;

namespace SSO.Client.Authorization
{
	public sealed class PermissionRequirement : IAuthorizationRequirement
	{
		public PermissionRequirement(params string[] permissions)
		{
			if (permissions is null || permissions.Length == 0)
			{
				throw new ArgumentException("At least one permission is required.", nameof(permissions));
			}

			Permissions = permissions;
		}

		public IReadOnlyList<string> Permissions { get; }
	}

	public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
	{
		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			PermissionRequirement requirement)
		{
			if (context.User?.Identity?.IsAuthenticated != true)
			{
				return Task.CompletedTask;
			}

			var granted = context.User.GetPermissions().ToHashSet(StringComparer.OrdinalIgnoreCase);
			if (requirement.Permissions.Any(p => granted.Contains(p)))
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}

	/// <summary>
	/// Requires JwtBearer authentication and at least one of the listed permission claim values (OR).
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class RequiresPermissionAttribute : AuthorizeAttribute, IAuthorizationRequirementData
	{
		private readonly PermissionRequirement _requirement;

		public RequiresPermissionAttribute(params string[] permissions)
		{
			_requirement = new PermissionRequirement(permissions);
			AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
		}

		public IEnumerable<IAuthorizationRequirement> GetRequirements()
		{
			yield return _requirement;
		}
	}
}
