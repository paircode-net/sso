using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity.AuthClientMetadata.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class AuthClientsModel : AdminPageModel
	{
		private readonly IdentityDbContext _db;
		private readonly IOpenIddictApplicationManager _applications;

		public AuthClientsModel(
			IAdminPortalContextService portal,
			IdentityDbContext db,
			IOpenIddictApplicationManager applications) : base(portal)
		{
			_db = db;
			_applications = applications;
		}

		public List<AuthClientMetadataEntity> Items { get; set; } = new();
		public List<Product> Products { get; set; } = new();

		[BindProperty]
		public string ClientId { get; set; } = string.Empty;

		[BindProperty]
		public string DisplayName { get; set; } = string.Empty;

		[BindProperty]
		public string ClientType { get; set; } = OpenIddictConstants.ClientTypes.Public;

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

			if (await _applications.FindByClientIdAsync(ClientId) is not null)
			{
				Error = "Já existe um client com esse Client Id.";
				await LoadAsync();
				return Page();
			}

			var isConfidential = string.Equals(ClientType, OpenIddictConstants.ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase);
			string? plainSecret = isConfidential ? GenerateSecret() : null;

			var descriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = ClientId,
				DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? ClientId : DisplayName,
				ClientType = isConfidential ? OpenIddictConstants.ClientTypes.Confidential : OpenIddictConstants.ClientTypes.Public,
				ClientSecret = plainSecret,
				ConsentType = ConsentTypes.External
			};
			descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
			descriptor.Permissions.Add(Permissions.Endpoints.Token);
			descriptor.Permissions.Add(Permissions.Endpoints.Revocation);
			descriptor.Permissions.Add(Permissions.Endpoints.EndSession);
			descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
			descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
			descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
			descriptor.Permissions.Add(Permissions.Scopes.Email);
			descriptor.Permissions.Add(Permissions.Scopes.Profile);
			descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);

			await _applications.CreateAsync(descriptor);

			var meta = AuthClientMetadataEntity.Create(
				ClientId,
				string.IsNullOrWhiteSpace(DisplayName) ? ClientId : DisplayName,
				isSystem: false,
				isFirstParty: false,
				AuthClientConsentPolicies.First);
			_db.AuthClientMetadata.Add(meta);

			if (ProductId is Guid productId)
			{
				var binding = new ClientProductBinding
				{
					Id = Guid.NewGuid(),
					ClientId = ClientId,
					ProductId = productId
				};
				binding.MarkCreated();
				_db.ClientProductBindings.Add(binding);
			}

			await _db.SaveChangesAsync();

			Message = plainSecret is null
				? "Client criado."
				: $"Client criado. Client secret (guarde agora, não será exibido novamente): {plainSecret}";
			ClientId = string.Empty;
			DisplayName = string.Empty;
			ProductId = null;

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDisableAsync(string clientId)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var meta = await _db.AuthClientMetadata
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == clientId);
			if (meta is null)
			{
				Error = "Client não encontrado.";
			}
			else
			{
				meta.IsEnabled = false;
				meta.TouchUpdated();
				await _db.SaveChangesAsync();
				Message = "Client desabilitado.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Products = await _db.Products.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Items = await _db.AuthClientMetadata.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.ClientId)
				.ToListAsync();
		}

		private static string GenerateSecret()
		{
			var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
			return Convert.ToBase64String(bytes);
		}
	}
}
