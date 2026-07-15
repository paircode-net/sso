using System;
using System.Linq;
using System.Security.Claims;
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
	public sealed class ExternalLoginModel : PageModel
	{
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;
		private readonly IAuthAuditService _auditService;

		public ExternalLoginModel(
			SignInManager<User> signInManager,
			UserManager<User> userManager,
			IAuthAuditService auditService)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_auditService = auditService;
		}

		public string? StatusMessage { get; set; }

		public IActionResult OnPost(string provider, string? returnUrl = null)
		{
			var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
		{
			returnUrl ??= Url.Content("~/");

			if (!string.IsNullOrEmpty(remoteError))
			{
				StatusMessage = $"External provider error: {remoteError}";
				return Page();
			}

			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info is null)
			{
				StatusMessage = "Error loading external login information.";
				return Page();
			}

			var signInResult = await _signInManager.ExternalLoginSignInAsync(
				info.LoginProvider,
				info.ProviderKey,
				isPersistent: false,
				bypassTwoFactor: true);

			if (signInResult.Succeeded)
			{
				var existing = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginSucceeded,
					AuthAuditOutcomes.Success,
					userId: existing?.Id,
					email: existing?.Email,
					detail: $"external:{info.LoginProvider}"));
				return LocalRedirect(returnUrl);
			}

			var email = info.Principal.FindFirstValue(ClaimTypes.Email)
				?? info.Principal.FindFirstValue("email")
				?? info.Principal.FindFirstValue("preferred_username");

			if (string.IsNullOrWhiteSpace(email))
			{
				StatusMessage = "External provider did not return an email claim.";
				return Page();
			}

			var user = await _userManager.FindByEmailAsync(email);
			if (user is null)
			{
				user = new User
				{
					UserName = email,
					Email = email,
					EmailConfirmed = true
				};
				user.MarkCreated();
				var create = await _userManager.CreateAsync(user);
				if (!create.Succeeded)
				{
					StatusMessage = string.Join(" ", create.Errors.Select(e => e.Description));
					return Page();
				}
			}

			var addLogin = await _userManager.AddLoginAsync(user, info);
			if (!addLogin.Succeeded && addLogin.Errors.All(e => e.Code != "LoginAlreadyAssociated"))
			{
				StatusMessage = string.Join(" ", addLogin.Errors.Select(e => e.Description));
				return Page();
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.LoginSucceeded,
				AuthAuditOutcomes.Success,
				userId: user.Id,
				email: user.Email,
				detail: $"external:{info.LoginProvider}:linked"));

			return LocalRedirect(returnUrl);
		}
	}
}
