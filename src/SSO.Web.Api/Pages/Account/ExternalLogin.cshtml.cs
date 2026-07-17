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
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Pages.Account
{
	[IgnoreAntiforgeryToken]
	public sealed class ExternalLoginModel : PageModel
	{
		private readonly SignInManager<User> _signInManager;
		private readonly IAuthAuditService _auditService;
		private readonly FederatedAccountService _federated;

		public ExternalLoginModel(
			SignInManager<User> signInManager,
			IAuthAuditService auditService,
			FederatedAccountService federated)
		{
			_signInManager = signInManager;
			_auditService = auditService;
			_federated = federated;
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
				StatusMessage = "External provider error.";
				SsoAuthMetrics.RecordLoginFailure("remote_error", "external");
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginFailed,
					AuthAuditOutcomes.Failure,
					detail: $"external:remote_error"));
				return Page();
			}

			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info is null)
			{
				StatusMessage = "Error loading external login information.";
				SsoAuthMetrics.RecordLoginFailure("missing_external_info", "external");
				return Page();
			}

			var signInResult = await _signInManager.ExternalLoginSignInAsync(
				info.LoginProvider,
				info.ProviderKey,
				isPersistent: false,
				bypassTwoFactor: true);

			if (signInResult.Succeeded)
			{
				var existing = await _signInManager.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				SsoAuthMetrics.RecordLoginSuccess("external");
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

			var emailVerified = FederatedAccountService.IsEmailVerifiedFromClaims(
				info.Principal.Claims,
				info.LoginProvider);

			var outcome = await _federated.ResolveOrProvisionAsync(
				info.LoginProvider,
				info.ProviderKey,
				email,
				emailVerified,
				info.ProviderDisplayName);

			if (!outcome.Succeeded || outcome.User is null)
			{
				StatusMessage = outcome.Error switch
				{
					"user_not_provisioned" => "No local account exists for this identity. Contact an administrator.",
					"email_not_verified" => "Email must be verified by the identity provider.",
					"email_required" => "External provider did not return an email claim.",
					_ => "Unable to complete external login."
				};
				SsoAuthMetrics.RecordLoginFailure(outcome.Error ?? "external_failed", "external");
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginFailed,
					AuthAuditOutcomes.Failure,
					email: email,
					detail: $"external:{info.LoginProvider}:{outcome.Detail}"));
				return Page();
			}

			await _signInManager.SignInAsync(outcome.User, isPersistent: false);
			SsoAuthMetrics.RecordLoginSuccess("external");
			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.LoginSucceeded,
				AuthAuditOutcomes.Success,
				userId: outcome.User.Id,
				email: outcome.User.Email,
				detail: $"external:{info.LoginProvider}:{outcome.Detail}"));

			return LocalRedirect(returnUrl);
		}
	}
}
