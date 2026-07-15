using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;

namespace SSO.Web.Api.Pages.Account
{
	[IgnoreAntiforgeryToken]
	public sealed class ForgotPasswordModel : PageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IMailService _mailService;
		private readonly IAuthAuditService _auditService;

		public ForgotPasswordModel(
			UserManager<User> userManager,
			IMailService mailService,
			IAuthAuditService auditService)
		{
			_userManager = userManager;
			_mailService = mailService;
			_auditService = auditService;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public bool Sent { get; set; }

		public sealed class InputModel
		{
			[Required]
			[EmailAddress]
			public string Email { get; set; } = string.Empty;
		}

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.FindByEmailAsync(Input.Email);
			if (user is not null && !user.IsDeleted)
			{
				var token = await _userManager.GeneratePasswordResetTokenAsync(user);
				var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
				var callback = Url.Page(
					"/Account/ResetPassword",
					pageHandler: null,
					values: new { userId = user.Id, code = encoded },
					protocol: Request.Scheme)!;

				await _mailService.SendAsync(
					user.Email!,
					"Reset your password",
					$"ResetToken={encoded};UserId={user.Id:D};Link={callback}");

				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.PasswordResetRequested,
					AuthAuditOutcomes.Success,
					userId: user.Id,
					email: user.Email));
			}

			Sent = true;
			return Page();
		}
	}
}
