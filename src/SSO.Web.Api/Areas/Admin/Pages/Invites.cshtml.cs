using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class InvitesModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;
		private readonly IMediator _mediator;
		private readonly IIdentityDbContextWriter _writer;
		private readonly IMailService _mail;

		public InvitesModel(
			IAdminPortalContextService portal,
			IdentityDbContext db,
			IMediator mediator,
			IIdentityDbContextWriter writer,
			IMailService mail) : base(portal)
		{
			_db = db;
			_mediator = mediator;
			_writer = writer;
			_mail = mail;
		}

		public List<OrganizationInvite> Items { get; set; } = new();
		public string? Error { get; set; }
		public string? Message { get; set; }

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.HasPermission(SsoAdminPermissions.Org) && !Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			if (Portal.OrganizationId is null)
			{
				Error = "Selecione uma organização em Contexto.";
				return Page();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostSendAsync()
		{
			if (!Portal.HasPermission(SsoAdminPermissions.Org) && !Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			if (Portal.OrganizationId is not Guid orgId)
			{
				Error = "Selecione uma organização em Contexto.";
				return Page();
			}

			try
			{
				var raw = OrganizationInviteToken.CreateRawToken();
				var invitedBy = Guid.Parse(
					User.FindFirstValue(ClaimTypes.NameIdentifier)
					?? User.FindFirstValue("sub")!);

				var invite = new OrganizationInvite
				{
					OrganizationId = orgId,
					Email = Email.Trim().ToLowerInvariant(),
					TokenHash = OrganizationInviteToken.Hash(raw),
					Status = OrganizationInviteStatuses.Pending,
					ExpiresAt = DateTime.UtcNow.AddDays(7),
					InvitedByUserId = invitedBy
				};
				invite.MarkCreated();
				await _mediator.Send(new CreateOrganizationInviteServiceRequest(invite));
				await _writer.CommitAsync();

				var link =
					$"{Request.Scheme}://{Request.Host}/Account/AcceptInvite?token={Uri.EscapeDataString(raw)}";

				await _mail.SendAsync(
					invite.Email,
					"Convite para organização",
					$"InviteToken={raw};OrganizationId={orgId:D};Link={link}");

				Message = $"Convite enviado para {invite.Email}.";
				Email = string.Empty;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostCancelAsync(Guid id)
		{
			if (!Portal.HasPermission(SsoAdminPermissions.Org) && !Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var invite = await _writer.Query<OrganizationInvite>()
				.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
			if (invite is null)
			{
				Error = "Convite não encontrado.";
			}
			else
			{
				try
				{
					await _mediator.Send(new CancelOrganizationInviteServiceRequest(invite));
					await _writer.CommitAsync();
					Message = "Convite cancelado.";
				}
				catch (Exception ex)
				{
					Error = ex.Message;
				}
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			var orgId = Portal.OrganizationId!.Value;
			Items = await _db.OrganizationInvites.AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
				.OrderByDescending(x => x.CreatedAt)
				.Take(100)
				.ToListAsync();
		}
	}
}
