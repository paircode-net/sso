using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class ClaimDefinitionsModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;

		public ClaimDefinitionsModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		public List<ClaimDefinition> Items { get; set; } = new();
		public List<Product> Products { get; set; } = new();

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		[BindProperty]
		public string? Description { get; set; }

		[BindProperty]
		public string ValueType { get; set; } = ClaimValueTypes.String;

		[BindProperty]
		public Guid? ProductId { get; set; }

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

			if (!ClaimValueTypes.IsValid(ValueType))
			{
				Error = "Tipo de valor inválido.";
				await LoadAsync();
				return Page();
			}

			var code = Code.Trim().ToLowerInvariant();
			if (await _db.ClaimDefinitions.AnyAsync(x => !x.IsDeleted && x.Code == code))
			{
				Error = "Já existe uma definição de claim com esse código.";
				await LoadAsync();
				return Page();
			}

			try
			{
				var entity = ClaimDefinition.Create(code, Name, ValueType, ProductId, Description);
				_db.ClaimDefinitions.Add(entity);
				await _db.SaveChangesAsync();
				Message = "Definição de claim criada.";
				Code = string.Empty;
				Name = string.Empty;
				Description = null;
				ProductId = null;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
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

			var entity = await _db.ClaimDefinitions.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id);
			if (entity is null)
			{
				Error = "Definição não encontrada.";
			}
			else
			{
				entity.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Definição removida.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Products = await _db.Products.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Items = await _db.ClaimDefinitions.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Code)
				.ToListAsync();
		}
	}
}
