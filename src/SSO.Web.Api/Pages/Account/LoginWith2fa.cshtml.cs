using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Web.Api.Pages.Account
{
	[IgnoreAntiforgeryToken]
	public sealed class LoginWith2faModel : PageModel
	{
		private readonly SignInManager<User> _signInManager;
		private readonly IAuthAuditService _auditService;

		public LoginWith2faModel(SignInManager<User> signInManager, IAuthAuditService auditService)
		{
			_signInManager = signInManager;
			_auditService = auditService;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		[BindProperty]
		public string? ReturnUrl { get; set; }

		public string? ErrorMessage { get; set; }

		public sealed class InputModel
		{
			[Required]
			[StringLength(8, MinimumLength = 6)]
			[DataType(DataType.Text)]
			public string TwoFactorCode { get; set; } = string.Empty;
		}

		public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
		{
			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user is null)
			{
				return RedirectToPage("./Login");
			}

			ReturnUrl = returnUrl ?? Url.Content("~/");
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			ReturnUrl ??= Url.Content("~/");
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
			if (user is null)
			{
				return RedirectToPage("./Login");
			}

			var code = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
			var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent: false, rememberClient: false);
			if (result.Succeeded)
			{
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.TwoFactorSucceeded,
					AuthAuditOutcomes.Success,
					userId: user.Id,
					email: user.Email));
				return LocalRedirect(ReturnUrl);
			}

			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.TwoFactorFailed,
				AuthAuditOutcomes.Failure,
				userId: user.Id,
				email: user.Email));
			ErrorMessage = "Invalid authenticator code.";
			return Page();
		}
	}
}
