using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Web.Api.Pages.Account
{
	public sealed class ConfirmEmailModel : PageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IAuthAuditService _auditService;

		public ConfirmEmailModel(UserManager<User> userManager, IAuthAuditService auditService)
		{
			_userManager = userManager;
			_auditService = auditService;
		}

		public bool Succeeded { get; set; }
		public string? ErrorMessage { get; set; }

		public async Task<IActionResult> OnGetAsync(Guid userId, string code)
		{
			if (userId == Guid.Empty || string.IsNullOrWhiteSpace(code))
			{
				ErrorMessage = "Invalid confirmation link.";
				return Page();
			}

			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user is null)
			{
				ErrorMessage = "User not found.";
				return Page();
			}

			var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (!result.Succeeded)
			{
				ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
				return Page();
			}

			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.EmailConfirmed,
				AuthAuditOutcomes.Success,
				userId: user.Id,
				email: user.Email));

			Succeeded = true;
			return Page();
		}
	}
}
