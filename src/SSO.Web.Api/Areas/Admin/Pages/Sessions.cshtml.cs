using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class SessionsModel : AdminPageModel
	{
		private readonly IUserSessionService _sessionService;

		public SessionsModel(IAdminPortalContextService portal, IUserSessionService sessionService) : base(portal)
		{
			_sessionService = sessionService;
		}

		[BindProperty(SupportsGet = true)]
		public Guid? UserId { get; set; }

		public IReadOnlyList<UserSession> Items { get; set; } = Array.Empty<UserSession>();

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.HasPermission(SsoAdminPermissions.SessionsRevoke))
			{
				return Forbid();
			}

			if (UserId is Guid id)
			{
				Items = await _sessionService.ListForUserAsync(id, includeRevoked: true);
			}

			return Page();
		}

		public async Task<IActionResult> OnPostRevokeAsync(Guid sessionId, Guid userId)
		{
			if (!Portal.HasPermission(SsoAdminPermissions.SessionsRevoke))
			{
				return Forbid();
			}

			var revoked = await _sessionService.RevokeSessionAsync(sessionId, "admin.portal.revoke");
			Message = revoked ? "Sessão revogada." : null;
			Error = revoked ? null : "Sessão não encontrada.";

			UserId = userId;
			Items = await _sessionService.ListForUserAsync(userId, includeRevoked: true);
			return Page();
		}
	}
}
