using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.OrganizationInvites.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class InvitesModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public InvitesModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<OrganizationInvite> Items { get; set; } = new();

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		private bool CanManage => Portal.HasPermission(SsoAdminPermissions.Org) || Portal.IsPlatformAdmin;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			if (!RequireOrgContext())
			{
				return Page();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostSendAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			if (Portal.OrganizationId is not Guid orgId)
			{
				Error = "Selecione uma organização em Contexto.";
				return Page();
			}

			var cmd = AdminWrap.FromAnonymous<PostOrganizationInviteCommand>(new { organizationId = orgId, email = Email });
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, $"Convite enviado para {Email.Trim().ToLowerInvariant()}."))
			{
				Email = string.Empty;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostCancelAsync(Guid id)
		{
			if (!CanManage)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PatchCancelOrganizationInviteCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Convite cancelado.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostResendAsync(Guid id)
		{
			if (!CanManage)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PatchResendOrganizationInviteCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Convite reenviado.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			var orgId = Portal.OrganizationId!.Value;
			Items = await _reader.Query<OrganizationInvite>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
				.OrderByDescending(x => x.CreatedAt)
				.Take(100)
				.ToListAsync();
		}
	}
}
