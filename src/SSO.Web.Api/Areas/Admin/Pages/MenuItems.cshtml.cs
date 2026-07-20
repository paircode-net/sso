using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.MenuItems.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class MenuItemsModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public MenuItemsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<MenuItem> Items { get; set; } = new();
		public List<Product> Products { get; set; } = new();

		[BindProperty(SupportsGet = true)]
		public Guid? EditId { get; set; }

		[BindProperty]
		public Guid ProductId { get; set; }

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public string Title { get; set; } = string.Empty;

		[BindProperty]
		public string Route { get; set; } = string.Empty;

		[BindProperty]
		public string PermissionCode { get; set; } = string.Empty;

		[BindProperty]
		public int SortOrder { get; set; }

		private bool CanManage => Portal.IsPlatformAdmin || Portal.HasPermission(SsoAdminPermissions.Menus);

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			await LoadAsync();

			if (EditId is Guid id)
			{
				var item = Items.FirstOrDefault(x => x.Id == id);
				if (item is not null)
				{
					ProductId = item.ProductId;
					Code = item.Code;
					Title = item.Title;
					Route = item.Route;
					PermissionCode = item.PermissionCode;
					SortOrder = item.SortOrder;
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

			var cmd = AdminWrap.FromAnonymous<PostMenuItemCommand>(new
			{
				productId = ProductId,
				code = Code,
				title = Title,
				route = Route,
				permissionCode = PermissionCode,
				sortOrder = SortOrder
			});
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Item de menu criado."))
			{
				Code = string.Empty;
				Title = string.Empty;
				Route = string.Empty;
				PermissionCode = string.Empty;
				SortOrder = 0;
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

			var cmd = AdminWrap.FromAnonymous<PutMenuItemCommand>(new
			{
				id,
				productId = ProductId,
				code = Code,
				title = Title,
				route = Route,
				permissionCode = PermissionCode,
				sortOrder = SortOrder
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Item de menu atualizado.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!CanManage)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteMenuItemCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Item de menu removido.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Products = await _reader.Query<Product>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Name)
				.ToListAsync();
			Items = await _reader.Query<MenuItem>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.ProductId).ThenBy(x => x.SortOrder)
				.ToListAsync();
		}
	}
}
