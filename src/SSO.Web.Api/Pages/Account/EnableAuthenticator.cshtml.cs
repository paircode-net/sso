using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Web.Api.Pages.Account
{
	[Authorize]
	[IgnoreAntiforgeryToken]
	public sealed class EnableAuthenticatorModel : PageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IAuthAuditService _auditService;
		private readonly UrlEncoder _urlEncoder;

		public EnableAuthenticatorModel(
			UserManager<User> userManager,
			IAuthAuditService auditService,
			UrlEncoder urlEncoder)
		{
			_userManager = userManager;
			_auditService = auditService;
			_urlEncoder = urlEncoder;
		}

		public string? SharedKey { get; set; }
		public string? AuthenticatorUri { get; set; }
		public bool Enabled { get; set; }
		public string? ErrorMessage { get; set; }

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public sealed class InputModel
		{
			[Required]
			[StringLength(8, MinimumLength = 6)]
			public string Code { get; set; } = string.Empty;
		}

		public async Task<IActionResult> OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			await LoadSharedKeyAsync(user);
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			if (!ModelState.IsValid)
			{
				await LoadSharedKeyAsync(user);
				return Page();
			}

			var code = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
			var isValid = await _userManager.VerifyTwoFactorTokenAsync(
				user,
				_userManager.Options.Tokens.AuthenticatorTokenProvider,
				code);

			if (!isValid)
			{
				ErrorMessage = "Verification code is invalid.";
				await LoadSharedKeyAsync(user);
				return Page();
			}

			await _userManager.SetTwoFactorEnabledAsync(user, true);
			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.TwoFactorEnabled,
				AuthAuditOutcomes.Success,
				userId: user.Id,
				email: user.Email));

			Enabled = true;
			return Page();
		}

		private async Task LoadSharedKeyAsync(User user)
		{
			var key = await _userManager.GetAuthenticatorKeyAsync(user);
			if (string.IsNullOrEmpty(key))
			{
				await _userManager.ResetAuthenticatorKeyAsync(user);
				key = await _userManager.GetAuthenticatorKeyAsync(user);
			}

			SharedKey = FormatKey(key!);
			AuthenticatorUri =
				$"otpauth://totp/{_urlEncoder.Encode("SSO")}:{_urlEncoder.Encode(user.Email!)}?secret={key}&issuer={_urlEncoder.Encode("SSO")}";
		}

		private static string FormatKey(string unformattedKey)
		{
			var result = new StringBuilder();
			var currentPosition = 0;
			while (currentPosition + 4 < unformattedKey.Length)
			{
				result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
				currentPosition += 4;
			}

			if (currentPosition < unformattedKey.Length)
			{
				result.Append(unformattedKey.AsSpan(currentPosition));
			}

			return result.ToString().ToLowerInvariant();
		}
	}
}
