using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Branches.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class BranchesModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public BranchesModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<Branch> Items { get; set; } = new();

		[BindProperty(SupportsGet = true)]
		public Guid? EditId { get; set; }

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public Guid? ParentBranchId { get; set; }

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

			if (EditId is Guid id)
			{
				var item = Items.FirstOrDefault(x => x.Id == id);
				if (item is not null)
				{
					Name = item.Name;
					Code = item.Code;
					ParentBranchId = item.ParentBranchId;
				}
			}

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
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

			var cmd = AdminWrap.FromAnonymous<PostBranchCommand>(new
			{
				organizationId = orgId,
				parentBranchId = ParentBranchId,
				name = Name,
				code = Code
			});
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Filial criada."))
			{
				Name = string.Empty;
				Code = string.Empty;
				ParentBranchId = null;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostUpdateAsync(Guid id)
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

			var cmd = AdminWrap.FromAnonymous<PutBranchCommand>(new
			{
				id,
				organizationId = orgId,
				parentBranchId = ParentBranchId,
				name = Name,
				code = Code
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Filial atualizada.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!CanManage)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteBranchCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Filial removida.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			var orgId = Portal.OrganizationId!.Value;
			Items = await _reader.Query<Branch>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
				.OrderBy(x => x.Name)
				.ToListAsync();
		}
	}
}
