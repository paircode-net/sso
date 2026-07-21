using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.OrganizationInvites.Commands;
using SSO.Core.Domain.Identity.OrganizationInvites;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;

namespace SSO.Web.Api.Pages.Account
{
	public sealed class AcceptInviteModel : PageModel
	{
		private readonly IMediator _mediator;
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IdentityDbContext _db;

		public AcceptInviteModel(
			IMediator mediator,
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			IdentityDbContext db)
		{
			_mediator = mediator;
			_userManager = userManager;
			_signInManager = signInManager;
			_db = db;
		}

		[BindProperty(SupportsGet = true)]
		public string? Token { get; set; }

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public OrganizationInvite? Invite { get; set; }
		public string? OrganizationName { get; set; }
		public string? Error { get; set; }
		public string? Message { get; set; }
		public bool NeedsRegistration { get; set; }
		public bool NeedsLogin { get; set; }

		public sealed class InputModel
		{
			[DataType(DataType.Password)]
			[MinLength(8)]
			public string? Password { get; set; }

			[DataType(DataType.Password)]
			[Compare(nameof(Password))]
			public string? ConfirmPassword { get; set; }
		}

		public async Task<IActionResult> OnGetAsync()
		{
			await LoadInviteAsync();
			if (Invite is null)
			{
				Error = "Convite inválido.";
				return Page();
			}

			if (!Invite.IsPending())
			{
				Error = "Convite expirado ou já utilizado.";
				return Page();
			}

			var existing = await _userManager.FindByEmailAsync(Invite.Email);
			if (existing is null)
			{
				NeedsRegistration = true;
			}
			else if (!_signInManager.IsSignedIn(User)
				|| !string.Equals(User.Identity?.Name, existing.UserName, StringComparison.OrdinalIgnoreCase)
					&& !string.Equals(
						(await _userManager.GetUserAsync(User))?.Email,
						Invite.Email,
						StringComparison.OrdinalIgnoreCase))
			{
				NeedsLogin = !string.Equals(
					(await _userManager.GetUserAsync(User))?.Email,
					Invite.Email,
					StringComparison.OrdinalIgnoreCase);
			}

			return Page();
		}

		public async Task<IActionResult> OnPostAcceptAsync()
		{
			await LoadInviteAsync();
			if (Invite is null || !Invite.IsPending())
			{
				Error = "Convite inválido ou expirado.";
				return Page();
			}

			User? user = await _userManager.GetUserAsync(User);
			var existing = await _userManager.FindByEmailAsync(Invite.Email);

			if (existing is null)
			{
				if (string.IsNullOrWhiteSpace(Input.Password))
				{
					NeedsRegistration = true;
					Error = "Informe uma senha para criar a conta.";
					return Page();
				}

				user = new User
				{
					Email = Invite.Email,
					UserName = Invite.Email,
					EmailConfirmed = true
				};
				user.MarkCreated();
				var create = await _userManager.CreateAsync(user, Input.Password);
				if (!create.Succeeded)
				{
					NeedsRegistration = true;
					Error = string.Join("; ", create.Errors.Select(e => e.Description));
					return Page();
				}

				await _signInManager.SignInAsync(user, isPersistent: false);
			}
			else
			{
				if (user is null
					|| !string.Equals(user.Email, Invite.Email, StringComparison.OrdinalIgnoreCase))
				{
					NeedsLogin = true;
					Error = "Faça login com o e-mail do convite para aceitar.";
					return Page();
				}
			}

			var result = await _mediator.Send(new PatchAcceptOrganizationInviteCommand
			{
				RawToken = Token!,
				AcceptingUserId = user!.Id
			});

			if (!result.Succeeded)
			{
				Error = result.Error;
				return Page();
			}

			Message = "Convite aceito. Você agora faz parte da organização.";
			Invite = null;
			return Page();
		}

		public async Task<IActionResult> OnPostDeclineAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			var result = await _mediator.Send(new PatchDeclineOrganizationInviteCommand
			{
				RawToken = Token ?? string.Empty,
				ActingUserId = user?.Id
			});

			if (!result.Succeeded)
			{
				Error = result.Error;
				await LoadInviteAsync();
				return Page();
			}

			Message = "Convite recusado.";
			Invite = null;
			return Page();
		}

		private async Task LoadInviteAsync()
		{
			if (string.IsNullOrWhiteSpace(Token))
			{
				return;
			}

			var hash = OrganizationInviteToken.Hash(Token);
			Invite = await _db.OrganizationInvites.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.TokenHash == hash);

			if (Invite is not null)
			{
				OrganizationName = await _db.Organizations.AsNoTracking()
					.Where(x => x.Id == Invite.OrganizationId)
					.Select(x => x.Name)
					.FirstOrDefaultAsync();
			}
		}
	}
}
