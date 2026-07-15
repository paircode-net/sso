using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Web.Api.Pages.Account
{
	[IgnoreAntiforgeryToken]
	public sealed class LoginModel : PageModel
	{
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;

		public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public string? ReturnUrl { get; set; }

		public string? ErrorMessage { get; set; }

		public sealed class InputModel
		{
			[Required]
			[EmailAddress]
			public string Email { get; set; } = string.Empty;

			[Required]
			[DataType(DataType.Password)]
			public string Password { get; set; } = string.Empty;
		}

		public void OnGet(string? returnUrl = null)
		{
			ReturnUrl = returnUrl ?? Url.Content("~/");
		}

		public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
		{
			ReturnUrl = returnUrl ?? Url.Content("~/");

			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.FindByEmailAsync(Input.Email);
			if (user is null || user.IsDeleted)
			{
				ErrorMessage = "Invalid login attempt.";
				return Page();
			}

			var result = await _signInManager.PasswordSignInAsync(user, Input.Password, isPersistent: false, lockoutOnFailure: true);
			if (result.Succeeded)
			{
				return LocalRedirect(ReturnUrl);
			}

			ErrorMessage = "Invalid login attempt.";
			return Page();
		}
	}
}
