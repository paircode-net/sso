using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/menus")]
	[RequiresPermission(SsoAdminPermissions.Menus)]
	public sealed class MenusController : ControllerBase
	{
		private readonly IEffectiveMenusResolver _menusResolver;
		private readonly IEffectivePermissionsResolver _permissionsResolver;
		private readonly IPermissionPolicyVersionProvider _policyVersionProvider;

		public MenusController(
			IEffectiveMenusResolver menusResolver,
			IEffectivePermissionsResolver permissionsResolver,
			IPermissionPolicyVersionProvider policyVersionProvider)
		{
			_menusResolver = menusResolver;
			_permissionsResolver = permissionsResolver;
			_policyVersionProvider = policyVersionProvider;
		}

		/// <summary>
		/// Diagnostic/admin: menus unlocked by effective permissions for a context.
		/// Products should prefer deriving UI from JWT <c>permissions</c> claims.
		/// </summary>
		[HttpGet("effective")]
		public async Task<IActionResult> GetEffectiveMenus(
			[FromQuery] Guid userId,
			[FromQuery] Guid organizationId,
			[FromQuery] Guid? branchId,
			[FromQuery] string clientId,
			CancellationToken cancellationToken)
		{
			if (userId == Guid.Empty || organizationId == Guid.Empty || string.IsNullOrWhiteSpace(clientId))
			{
				return BadRequest("userId, organizationId and clientId are required.");
			}

			var menus = await _menusResolver.ResolveAsync(
				userId,
				organizationId,
				branchId,
				clientId,
				cancellationToken);

			var permissions = await _permissionsResolver.ResolveAsync(
				userId,
				organizationId,
				branchId,
				clientId,
				cancellationToken);

			var permVer = await _policyVersionProvider.GetVersionAsync(cancellationToken);

			return Ok(new
			{
				perm_ver = permVer,
				permissions,
				menus
			});
		}
	}
}
