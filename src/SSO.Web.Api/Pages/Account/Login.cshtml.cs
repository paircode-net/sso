using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Pages.Account
{
	[IgnoreAntiforgeryToken]
	public sealed class LoginModel : PageModel
	{
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;
		private readonly IAuthAuditService _auditService;
		private readonly SsoHardeningOptions _options;
		private readonly IAdminPortalContextService _portal;

		public LoginModel(
			SignInManager<User> signInManager,
			UserManager<User> userManager,
			IAuthAuditService auditService,
			IOptions<SsoHardeningOptions> options,
			IAdminPortalContextService portal)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_auditService = auditService;
			_options = options.Value;
			_portal = portal;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		[BindProperty]
		public string? ReturnUrl { get; set; }

		public string? ErrorMessage { get; set; }

		public IList<AuthenticationScheme> ExternalProviders { get; set; } = new List<AuthenticationScheme>();

		public bool LdapEnabled => _options.ExternalAuth?.Ldap?.Enabled == true;

		public sealed class InputModel
		{
			[Required]
			[EmailAddress]
			public string Email { get; set; } = string.Empty;

			[Required]
			[DataType(DataType.Password)]
			public string Password { get; set; } = string.Empty;
		}

		public async Task OnGetAsync(string? returnUrl = null)
		{
			ReturnUrl = returnUrl ?? Url.Content("~/");
			ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			ReturnUrl ??= Url.Content("~/");
			ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.FindByEmailAsync(Input.Email);
			if (user is null || user.IsDeleted)
			{
				SsoAuthMetrics.RecordLoginFailure("user_not_found");
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginFailed,
					AuthAuditOutcomes.Failure,
					email: Input.Email,
					ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
					detail: "user_not_found"));
				ErrorMessage = "Invalid login attempt.";
				return Page();
			}

			var result = await _signInManager.PasswordSignInAsync(
				user,
				Input.Password,
				isPersistent: false,
				lockoutOnFailure: true);

			if (result.Succeeded)
			{
				SsoAuthMetrics.RecordLoginSuccess();
				await _portal.ClearContextAsync();
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginSucceeded,
					AuthAuditOutcomes.Success,
					userId: user.Id,
					email: user.Email,
					ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()));
				return LocalRedirect(ReturnUrl);
			}

			if (result.RequiresTwoFactor)
			{
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginRequiresTwoFactor,
					AuthAuditOutcomes.Success,
					userId: user.Id,
					email: user.Email));
				return RedirectToPage("./LoginWith2fa", new { returnUrl = ReturnUrl });
			}

			if (result.IsLockedOut)
			{
				SsoAuthMetrics.RecordLoginFailure("locked_out");
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginLockedOut,
					AuthAuditOutcomes.Failure,
					userId: user.Id,
					email: user.Email));
				ErrorMessage = "This account is locked out.";
				return Page();
			}

			if (result.IsNotAllowed)
			{
				SsoAuthMetrics.RecordLoginFailure("not_allowed");
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginFailed,
					AuthAuditOutcomes.Failure,
					userId: user.Id,
					email: user.Email,
					detail: "not_allowed"));
				ErrorMessage = "Email confirmation is required before signing in.";
				return Page();
			}

			SsoAuthMetrics.RecordLoginFailure("invalid_password");
			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.LoginFailed,
				AuthAuditOutcomes.Failure,
				userId: user.Id,
				email: user.Email,
				detail: "invalid_password"));
			ErrorMessage = "Invalid login attempt.";
			return Page();
		}
	}
}
