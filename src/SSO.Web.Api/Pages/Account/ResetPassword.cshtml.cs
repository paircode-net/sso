using System;
using System.ComponentModel.DataAnnotations;
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
	[IgnoreAntiforgeryToken]
	public sealed class ResetPasswordModel : PageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IAuthAuditService _auditService;
		private readonly IUserSessionService _sessionService;

		public ResetPasswordModel(
			UserManager<User> userManager,
			IAuthAuditService auditService,
			IUserSessionService sessionService)
		{
			_userManager = userManager;
			_auditService = auditService;
			_sessionService = sessionService;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public bool Completed { get; set; }
		public string? ErrorMessage { get; set; }

		public sealed class InputModel
		{
			[Required]
			public Guid UserId { get; set; }

			[Required]
			public string Code { get; set; } = string.Empty;

			[Required]
			[DataType(DataType.Password)]
			[MinLength(8)]
			public string Password { get; set; } = string.Empty;

			[Required]
			[DataType(DataType.Password)]
			[Compare(nameof(Password))]
			public string ConfirmPassword { get; set; } = string.Empty;
		}

		public IActionResult OnGet(Guid userId, string code)
		{
			if (userId == Guid.Empty || string.IsNullOrWhiteSpace(code))
			{
				ErrorMessage = "Invalid reset link.";
				return Page();
			}

			Input.UserId = userId;
			Input.Code = code;
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.FindByIdAsync(Input.UserId.ToString());
			if (user is null || user.IsDeleted)
			{
				ErrorMessage = "Invalid reset request.";
				return Page();
			}

			var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
			var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);
			if (!result.Succeeded)
			{
				ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
				return Page();
			}

			await _sessionService.RevokeAllForUserAsync(user.Id, "password.reset");

			await _auditService.WriteAsync(AuthAuditEvent.Create(
				AuthAuditEventTypes.PasswordResetCompleted,
				AuthAuditOutcomes.Success,
				userId: user.Id,
				email: user.Email));

			Completed = true;
			return Page();
		}
	}
}
