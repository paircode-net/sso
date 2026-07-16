using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/sessions")]
	[IgnoreAntiforgeryToken]
	public sealed class SessionsController : ControllerBase
	{
		private readonly IUserSessionService _sessionService;
		private readonly UserManager<User> _userManager;

		public SessionsController(IUserSessionService sessionService, UserManager<User> userManager)
		{
			_sessionService = sessionService;
			_userManager = userManager;
		}

		/// <summary>Lightweight revoke status for product SDK hot-check (SLA ≤ 60s).</summary>
		[HttpGet("{sessionId:guid}/status")]
		[AllowAnonymous]
		public async Task<IActionResult> GetStatus(Guid sessionId, CancellationToken cancellationToken)
		{
			var revoked = await _sessionService.IsSessionRevokedAsync(sessionId, cancellationToken);
			return Ok(new { session_id = sessionId, revoked });
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> ListMine(
			[FromQuery] bool includeRevoked = false,
			CancellationToken cancellationToken = default)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Unauthorized();
			}

			var sessions = await _sessionService.ListForUserAsync(user.Id, includeRevoked, cancellationToken);
			return Ok(sessions.Select(Map));
		}

		[HttpPost("me/{sessionId:guid}/revoke")]
		[Authorize]
		public async Task<IActionResult> RevokeMine(Guid sessionId, CancellationToken cancellationToken)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Unauthorized();
			}

			var sessions = await _sessionService.ListForUserAsync(user.Id, includeRevoked: true, cancellationToken);
			if (sessions.All(x => x.Id != sessionId))
			{
				return NotFound();
			}

			await _sessionService.RevokeSessionAsync(sessionId, "self.revoke", cancellationToken);
			return Ok(new { revoked = true, session_id = sessionId });
		}

		[HttpGet("user/{userId:guid}")]
		[RequiresPermission(SsoAdminPermissions.SessionsRevoke)]
		public async Task<IActionResult> ListForUser(
			Guid userId,
			[FromQuery] bool includeRevoked = true,
			CancellationToken cancellationToken = default)
		{
			var sessions = await _sessionService.ListForUserAsync(userId, includeRevoked, cancellationToken);
			return Ok(sessions.Select(Map));
		}

		[HttpPost("{sessionId:guid}/revoke")]
		[RequiresPermission(SsoAdminPermissions.SessionsRevoke)]
		public async Task<IActionResult> RevokeOne(Guid sessionId, CancellationToken cancellationToken)
		{
			var ok = await _sessionService.RevokeSessionAsync(sessionId, "admin.revoke", cancellationToken);
			if (!ok)
			{
				return NotFound();
			}

			return Ok(new { revoked = true, session_id = sessionId });
		}

		private static object Map(Core.Domain.Identity.UserSessions.Entity.UserSession s) => new
		{
			id = s.Id,
			user_id = s.UserId,
			client_id = s.ClientId,
			organization_id = s.OrganizationId,
			branch_id = s.BranchId,
			created_at = s.CreatedAt,
			last_seen_at = s.LastSeenAt,
			revoked_at = s.RevokedAt,
			revoke_reason = s.RevokeReason,
			active = s.IsActive
		};
	}
}
