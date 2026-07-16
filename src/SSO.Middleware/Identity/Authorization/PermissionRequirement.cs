using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SSO.Shared.Identity;

namespace SSO.Middleware.Identity.Authorization
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

		/// <summary>Caller must have at least one of these permission codes (OR).</summary>
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

			var granted = context.User
				.FindAll(SsoClaimTypes.Permissions)
				.Select(c => c.Value)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			if (requirement.Permissions.Any(p => granted.Contains(p)))
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}
}
