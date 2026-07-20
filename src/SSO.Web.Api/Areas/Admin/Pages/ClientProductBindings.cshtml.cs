using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.ClientProductBindings.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class ClientProductBindingsModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public ClientProductBindingsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<ClientProductBinding> Items { get; set; } = new();
		public List<Product> Products { get; set; } = new();

		[BindProperty]
		public string ClientId { get; set; } = string.Empty;

		[BindProperty]
		public Guid ProductId { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PostClientProductBindingCommand>(new { clientId = ClientId, productId = ProductId });
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Vínculo criado."))
			{
				ClientId = string.Empty;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteClientProductBindingCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Vínculo removido.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Products = await _reader.Query<Product>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Name)
				.ToListAsync();
			Items = await _reader.Query<ClientProductBinding>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.ClientId)
				.ToListAsync();
		}
	}
}
