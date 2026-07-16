using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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
	public sealed class LoginWithLdapModel : PageModel
	{
		private readonly ILdapAuthenticationService _ldap;
		private readonly FederatedAccountService _federated;
		private readonly LdapGroupRoleSyncService _groupSync;
		private readonly SignInManager<User> _signInManager;
		private readonly IAuthAuditService _auditService;
		private readonly SsoHardeningOptions _options;

		public LoginWithLdapModel(
			ILdapAuthenticationService ldap,
			FederatedAccountService federated,
			LdapGroupRoleSyncService groupSync,
			SignInManager<User> signInManager,
			IAuthAuditService auditService,
			IOptions<SsoHardeningOptions> options)
		{
			_ldap = ldap;
			_federated = federated;
			_groupSync = groupSync;
			_signInManager = signInManager;
			_auditService = auditService;
			_options = options.Value;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		[BindProperty]
		public string? ReturnUrl { get; set; }

		public string? ErrorMessage { get; set; }
		public bool LdapEnabled => _options.ExternalAuth?.Ldap?.Enabled == true;

		public sealed class InputModel
		{
			[Required]
			public string Username { get; set; } = string.Empty;

			[Required]
			[DataType(DataType.Password)]
			public string Password { get; set; } = string.Empty;
		}

		public IActionResult OnGet(string? returnUrl = null)
		{
			ReturnUrl = returnUrl ?? Url.Content("~/");
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			ReturnUrl ??= Url.Content("~/");
			if (!LdapEnabled)
			{
				ErrorMessage = "LDAP authentication is not enabled.";
				return Page();
			}

			if (!ModelState.IsValid)
			{
				return Page();
			}

			var auth = await _ldap.AuthenticateAsync(Input.Username, Input.Password);
			if (!auth.Succeeded)
			{
				ErrorMessage = "Invalid login attempt.";
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginFailed,
					AuthAuditOutcomes.Failure,
					email: Input.Username,
					ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
					detail: $"ldap:{auth.Error}"));
				return Page();
			}

			var outcome = await _federated.ResolveOrProvisionAsync(
				AuthenticationSchemes.Ldap,
				auth.ProviderKey!,
				auth.Email,
				emailVerified: !string.IsNullOrWhiteSpace(auth.Email),
				auth.DisplayName);

			if (!outcome.Succeeded || outcome.User is null)
			{
				ErrorMessage = outcome.Error switch
				{
					"user_not_provisioned" => "No local account exists for this identity. Contact an administrator.",
					"email_required" => "Directory entry has no email; pre-provision the user first.",
					_ => "Invalid login attempt."
				};
				await _auditService.WriteAsync(AuthAuditEvent.Create(
					AuthAuditEventTypes.LoginFailed,
					AuthAuditOutcomes.Failure,
					email: auth.Email ?? Input.Username,
					detail: $"ldap:{outcome.Detail}"));
				return Page();
			}

			if (Guid.TryParse(_options.ExternalAuth?.Ldap?.DefaultOrganizationId, out var orgId)
				&& auth.Groups.Count > 0)
			{
				await _groupSync.SyncAsync(outcome.User.Id, orgId, auth.Groups);
			}

			await _signInManager.SignInAsync(outcome.User, isPersistent: false);
			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.LoginSucceeded,
				AuthAuditOutcomes.Success,
				userId: outcome.User.Id,
				email: outcome.User.Email,
				detail: $"ldap:{outcome.Detail};groups={auth.Groups.Count}"));

			return LocalRedirect(ReturnUrl);
		}
	}
}
