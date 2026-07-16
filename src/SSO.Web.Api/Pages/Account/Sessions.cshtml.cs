using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Web.Api.Pages.Account
{
	[Authorize]
	public sealed class SessionsModel : PageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IUserSessionService _sessionService;

		public SessionsModel(UserManager<User> userManager, IUserSessionService sessionService)
		{
			_userManager = userManager;
			_sessionService = sessionService;
		}

		public IReadOnlyList<UserSession> Sessions { get; private set; } = Array.Empty<UserSession>();
		public string? Message { get; private set; }

		public async Task OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return;
			}

			Sessions = await _sessionService.ListForUserAsync(user.Id, includeRevoked: true);
		}

		public async Task<IActionResult> OnPostRevokeAsync(Guid sessionId)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			var mine = await _sessionService.ListForUserAsync(user.Id, includeRevoked: true);
			if (mine.Any(x => x.Id == sessionId))
			{
				await _sessionService.RevokeSessionAsync(sessionId, "account.sessions.ui");
				Message = "Sessão revogada.";
			}

			Sessions = await _sessionService.ListForUserAsync(user.Id, includeRevoked: true);
			return Page();
		}
	}
}
