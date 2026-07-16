using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Branches.Services;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class BranchesModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;
		private readonly IMediator _mediator;
		private readonly IIdentityDbContextWriter _writer;

		public BranchesModel(
			IAdminPortalContextService portal,
			IdentityDbContext db,
			IMediator mediator,
			IIdentityDbContextWriter writer) : base(portal)
		{
			_db = db;
			_mediator = mediator;
			_writer = writer;
		}

		public List<Branch> Items { get; set; } = new();
		public string? Error { get; set; }
		public string? Message { get; set; }

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public Guid? ParentBranchId { get; set; }

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

		public async Task<IActionResult> OnPostAsync()
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
				var branch = new Branch
				{
					OrganizationId = orgId,
					ParentBranchId = ParentBranchId,
					Name = Name.Trim(),
					Code = Code.Trim()
				};
				branch.MarkCreated();
				await _mediator.Send(new CreateBranchServiceRequest(branch));
				await _writer.CommitAsync();
				Message = "Filial criada.";
				Name = string.Empty;
				Code = string.Empty;
				ParentBranchId = null;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			var orgId = Portal.OrganizationId!.Value;
			Items = await _db.Branches.AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
				.OrderBy(x => x.Name)
				.ToListAsync();
		}
	}
}
