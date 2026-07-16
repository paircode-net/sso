using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Web.Api.Pages.Account
{
	[Authorize]
	[IgnoreAntiforgeryToken]
	public sealed class ConsentModel : PageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IOpenIddictApplicationManager _applicationManager;
		private readonly IOpenIddictAuthorizationManager _authorizationManager;
		private readonly IdentityDbContext _db;
		private readonly IAuthAuditService _auditService;

		public ConsentModel(
			UserManager<User> userManager,
			IOpenIddictApplicationManager applicationManager,
			IOpenIddictAuthorizationManager authorizationManager,
			IdentityDbContext db,
			IAuthAuditService auditService)
		{
			_userManager = userManager;
			_applicationManager = applicationManager;
			_authorizationManager = authorizationManager;
			_db = db;
			_auditService = auditService;
		}

		[BindProperty]
		public string ReturnUrl { get; set; } = "/";

		[BindProperty]
		public string ClientId { get; set; } = string.Empty;

		[BindProperty]
		public string Scope { get; set; } = string.Empty;

		[BindProperty]
		public bool Remember { get; set; } = true;

		public string ClientDisplayName { get; private set; } = string.Empty;
		public IReadOnlyList<string> Scopes { get; private set; } = Array.Empty<string>();
		public bool AllowRemember { get; private set; } = true;
		public string? ErrorMessage { get; private set; }

		public async Task<IActionResult> OnGetAsync(string returnUrl, string clientId, string? scope = null)
		{
			if (string.IsNullOrWhiteSpace(returnUrl) || string.IsNullOrWhiteSpace(clientId))
			{
				ErrorMessage = "Invalid consent request.";
				return Page();
			}

			ReturnUrl = returnUrl;
			ClientId = clientId;
			Scope = scope ?? string.Empty;
			Scopes = ParseScopes(Scope);

			var application = await _applicationManager.FindByClientIdAsync(ClientId);
			if (application is null)
			{
				ErrorMessage = "Unknown client.";
				return Page();
			}

			ClientDisplayName = await _applicationManager.GetDisplayNameAsync(application) ?? ClientId;
			var meta = await _db.AuthClientMetadata.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == ClientId);
			// First = one-shot permanent grant; Always = optional remember with TTL; Never should not reach here.
			AllowRemember = !string.Equals(
				meta?.RequireConsent,
				AuthClientConsentPolicies.First,
				StringComparison.OrdinalIgnoreCase);
			if (string.Equals(meta?.RequireConsent, AuthClientConsentPolicies.Always, StringComparison.OrdinalIgnoreCase))
			{
				Remember = true;
			}

			return Page();
		}

		public async Task<IActionResult> OnPostAcceptAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			var application = await _applicationManager.FindByClientIdAsync(ClientId);
			if (application is null)
			{
				ErrorMessage = "Unknown client.";
				return Page();
			}

			var scopes = ParseScopes(Scope);
			Scopes = scopes;
			ClientDisplayName = await _applicationManager.GetDisplayNameAsync(application) ?? ClientId;

			var meta = await _db.AuthClientMetadata.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == ClientId);

			var identity = new System.Security.Claims.ClaimsIdentity(
				authenticationType: "Consent",
				nameType: Claims.Name,
				roleType: Claims.Role);
			identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user));
			identity.SetScopes(scopes);
			var principal = new System.Security.Claims.ClaimsPrincipal(identity);

			if (Remember)
			{
				var descriptor = new OpenIddictAuthorizationDescriptor
				{
					Principal = principal,
					Subject = await _userManager.GetUserIdAsync(user),
					ApplicationId = await _applicationManager.GetIdAsync(application),
					Status = Statuses.Valid,
					Type = AuthorizationTypes.Permanent
				};
				foreach (var s in scopes)
				{
					descriptor.Scopes.Add(s);
				}

				await _authorizationManager.CreateAsync(descriptor);
			}

			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.ConsentGranted,
				AuthAuditOutcomes.Success,
				userId: user.Id,
				email: user.Email,
				clientId: ClientId,
				detail: $"scopes={string.Join(' ', scopes)};remember={Remember}"));

			return LocalRedirect(ReturnUrl);
		}

		public async Task<IActionResult> OnPostDenyAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.ConsentDenied,
				AuthAuditOutcomes.Failure,
				userId: user?.Id,
				email: user?.Email,
				clientId: ClientId,
				detail: "user_denied"));

			ErrorMessage = "Consent denied. You can close this window.";
			Scopes = ParseScopes(Scope);
			return Page();
		}

		private static IReadOnlyList<string> ParseScopes(string? scope)
			=> string.IsNullOrWhiteSpace(scope)
				? Array.Empty<string>()
				: scope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
					.Distinct(StringComparer.Ordinal)
					.ToList();
	}
}
