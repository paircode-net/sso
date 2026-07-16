using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using SSO.Core.Domain.Identity.AuthClientMetadata.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Web.Api.Resources.IdentityDb
{
	[ApiController]
	[Route("api/identity/auth-clients")]
	[IgnoreAntiforgeryToken]
	[RequiresPermission(SsoAdminPermissions.Platform)]
	public sealed class AuthClientsController : ControllerBase
	{
		private readonly IOpenIddictApplicationManager _applications;
		private readonly IOpenIddictScopeManager _scopes;
		private readonly IdentityDbContext _db;
		private readonly IWebHostEnvironment _environment;

		public AuthClientsController(
			IOpenIddictApplicationManager applications,
			IOpenIddictScopeManager scopes,
			IdentityDbContext db,
			IWebHostEnvironment environment)
		{
			_applications = applications;
			_scopes = scopes;
			_db = db;
			_environment = environment;
		}

		[HttpGet]
		public async Task<IActionResult> List(CancellationToken cancellationToken)
		{
			var metas = await _db.AuthClientMetadata.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.ToListAsync(cancellationToken);
			var bindings = await _db.ClientProductBindings.AsNoTracking()
				.Where(x => !x.IsDeleted)
				.ToDictionaryAsync(x => x.ClientId, x => x.ProductId, cancellationToken);

			var items = new List<object>();
			foreach (var meta in metas.OrderBy(x => x.ClientId))
			{
				var app = await _applications.FindByClientIdAsync(meta.ClientId, cancellationToken);
				items.Add(await MapAsync(app, meta, bindings.GetValueOrDefault(meta.ClientId), includeSecret: false));
			}

			return Ok(items);
		}

		[HttpGet("{clientId}")]
		public async Task<IActionResult> Get(string clientId, CancellationToken cancellationToken)
		{
			var meta = await _db.AuthClientMetadata.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == clientId, cancellationToken);
			var app = await _applications.FindByClientIdAsync(clientId, cancellationToken);
			if (app is null && meta is null)
			{
				return NotFound();
			}

			Guid? productId = null;
			var binding = await _db.ClientProductBindings.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == clientId, cancellationToken);
			if (binding is not null)
			{
				productId = binding.ProductId;
			}

			return Ok(await MapAsync(app, meta, productId, includeSecret: false));
		}

		public sealed class CreateRequest
		{
			[Required]
			public string ClientId { get; set; } = string.Empty;
			public string? DisplayName { get; set; }
			/// <summary>public | confidential</summary>
			[Required]
			public string ClientType { get; set; } = ClientTypes.Public;
			public bool IsFirstParty { get; set; }
			public string RequireConsent { get; set; } = AuthClientConsentPolicies.First;
			public int ConsentRememberDays { get; set; } = 180;
			public List<string> RedirectUris { get; set; } = new();
			public List<string> PostLogoutRedirectUris { get; set; } = new();
			public List<string> AllowedScopes { get; set; } = new()
			{
				"openid",
				"offline_access"
			};
			public Guid? ProductId { get; set; }
			public bool AllowAuthorizationCode { get; set; } = true;
			public bool AllowClientCredentials { get; set; }
			public bool RequirePkce { get; set; } = true;
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateRequest request, CancellationToken cancellationToken)
		{
			if (!AuthClientConsentPolicies.IsValid(request.RequireConsent))
			{
				return BadRequest(new { error = "invalid_require_consent" });
			}

			if (string.Equals(request.RequireConsent, AuthClientConsentPolicies.Never, StringComparison.OrdinalIgnoreCase)
				&& !request.IsFirstParty)
			{
				return BadRequest(new { error = "never_requires_first_party" });
			}

			if (await _applications.FindByClientIdAsync(request.ClientId, cancellationToken) is not null)
			{
				return Conflict(new { error = "client_exists" });
			}

			if (!ValidateRedirects(request.RedirectUris, out var redirectError))
			{
				return BadRequest(new { error = redirectError });
			}

			var isConfidential = string.Equals(request.ClientType, ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase);
			string? plainSecret = null;
			if (isConfidential)
			{
				plainSecret = GenerateSecret();
			}

			var descriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = request.ClientId,
				DisplayName = request.DisplayName ?? request.ClientId,
				ClientType = isConfidential ? ClientTypes.Confidential : ClientTypes.Public,
				ClientSecret = plainSecret,
				ConsentType = MapConsentType(request.RequireConsent)
			};

			foreach (var uri in request.RedirectUris)
			{
				descriptor.RedirectUris.Add(new Uri(uri));
			}

			foreach (var uri in request.PostLogoutRedirectUris)
			{
				descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
			}

			ApplyPermissions(descriptor, request);
			if (request.RequirePkce && request.AllowAuthorizationCode)
			{
				descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
			}

			await _applications.CreateAsync(descriptor, cancellationToken);

			var meta = AuthClientMetadataEntity.Create(
				request.ClientId,
				request.DisplayName ?? request.ClientId,
				isSystem: false,
				request.IsFirstParty,
				request.RequireConsent,
				request.ConsentRememberDays);
			_db.AuthClientMetadata.Add(meta);

			if (request.ProductId is Guid productId)
			{
				var binding = new ClientProductBinding
				{
					Id = Guid.NewGuid(),
					ClientId = request.ClientId,
					ProductId = productId
				};
				binding.MarkCreated();
				_db.ClientProductBindings.Add(binding);
			}

			await EnsureScopesExistAsync(request.AllowedScopes, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);

			return Ok(new
			{
				client_id = request.ClientId,
				client_secret = plainSecret,
				message = plainSecret is null ? null : "Store client_secret now; it will not be shown again."
			});
		}

		public sealed class UpdateRequest
		{
			public string? DisplayName { get; set; }
			public bool? IsFirstParty { get; set; }
			public string? RequireConsent { get; set; }
			public int? ConsentRememberDays { get; set; }
			public bool? IsEnabled { get; set; }
			public List<string>? RedirectUris { get; set; }
			public List<string>? PostLogoutRedirectUris { get; set; }
			public Guid? ProductId { get; set; }
			public bool ClearProductBinding { get; set; }
		}

		[HttpPut("{clientId}")]
		public async Task<IActionResult> Update(string clientId, [FromBody] UpdateRequest request, CancellationToken cancellationToken)
		{
			var app = await _applications.FindByClientIdAsync(clientId, cancellationToken);
			var meta = await _db.AuthClientMetadata
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == clientId, cancellationToken);
			if (app is null || meta is null)
			{
				return NotFound();
			}

			var isFirstParty = request.IsFirstParty ?? meta.IsFirstParty;
			var requireConsent = request.RequireConsent ?? meta.RequireConsent;
			if (!AuthClientConsentPolicies.IsValid(requireConsent))
			{
				return BadRequest(new { error = "invalid_require_consent" });
			}

			if (string.Equals(requireConsent, AuthClientConsentPolicies.Never, StringComparison.OrdinalIgnoreCase)
				&& !isFirstParty)
			{
				return BadRequest(new { error = "never_requires_first_party" });
			}

			if (request.RedirectUris is not null && !ValidateRedirects(request.RedirectUris, out var redirectError))
			{
				return BadRequest(new { error = redirectError });
			}

			var descriptor = new OpenIddictApplicationDescriptor();
			await _applications.PopulateAsync(descriptor, app, cancellationToken);
			if (request.DisplayName is not null)
			{
				descriptor.DisplayName = request.DisplayName;
			}

			descriptor.ConsentType = MapConsentType(requireConsent);
			if (request.RedirectUris is not null)
			{
				descriptor.RedirectUris.Clear();
				foreach (var uri in request.RedirectUris)
				{
					descriptor.RedirectUris.Add(new Uri(uri));
				}
			}

			if (request.PostLogoutRedirectUris is not null)
			{
				descriptor.PostLogoutRedirectUris.Clear();
				foreach (var uri in request.PostLogoutRedirectUris)
				{
					descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
				}
			}

			await _applications.UpdateAsync(app, descriptor, cancellationToken);

			meta.DisplayName = request.DisplayName ?? meta.DisplayName;
			meta.IsFirstParty = isFirstParty;
			meta.RequireConsent = requireConsent;
			if (request.ConsentRememberDays is int days && days > 0)
			{
				meta.ConsentRememberDays = days;
			}

			if (request.IsEnabled is bool enabled)
			{
				meta.IsEnabled = enabled;
			}

			meta.TouchUpdated();

			if (request.ClearProductBinding)
			{
				var existing = await _db.ClientProductBindings
					.Where(x => !x.IsDeleted && x.ClientId == clientId)
					.ToListAsync(cancellationToken);
				foreach (var b in existing)
				{
					b.MarkDeleted();
				}
			}
			else if (request.ProductId is Guid productId)
			{
				var binding = await _db.ClientProductBindings
					.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == clientId, cancellationToken);
				if (binding is null)
				{
					binding = new ClientProductBinding
					{
						Id = Guid.NewGuid(),
						ClientId = clientId,
						ProductId = productId
					};
					binding.MarkCreated();
					_db.ClientProductBindings.Add(binding);
				}
				else
				{
					binding.ProductId = productId;
					binding.TouchUpdated();
				}
			}

			await _db.SaveChangesAsync(cancellationToken);
			return Ok(new { updated = true, client_id = clientId });
		}

		[HttpPost("{clientId}/rotate-secret")]
		public async Task<IActionResult> RotateSecret(string clientId, CancellationToken cancellationToken)
		{
			var app = await _applications.FindByClientIdAsync(clientId, cancellationToken);
			var meta = await _db.AuthClientMetadata.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == clientId, cancellationToken);
			if (app is null || meta is null)
			{
				return NotFound();
			}

			if (!string.Equals(await _applications.GetClientTypeAsync(app, cancellationToken), ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase))
			{
				return BadRequest(new { error = "public_client_has_no_secret" });
			}

			var plainSecret = GenerateSecret();
			var descriptor = new OpenIddictApplicationDescriptor();
			await _applications.PopulateAsync(descriptor, app, cancellationToken);
			descriptor.ClientSecret = plainSecret;
			await _applications.UpdateAsync(app, descriptor, cancellationToken);

			return Ok(new
			{
				client_id = clientId,
				client_secret = plainSecret,
				message = "Store client_secret now; it will not be shown again."
			});
		}

		[HttpPost("{clientId}/disable")]
		public async Task<IActionResult> Disable(string clientId, CancellationToken cancellationToken)
		{
			var meta = await _db.AuthClientMetadata
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == clientId, cancellationToken);
			if (meta is null)
			{
				return NotFound();
			}

			meta.IsEnabled = false;
			meta.TouchUpdated();
			await _db.SaveChangesAsync(cancellationToken);
			return Ok(new { disabled = true, client_id = clientId });
		}

		[HttpDelete("{clientId}")]
		public async Task<IActionResult> Delete(string clientId, CancellationToken cancellationToken)
		{
			var meta = await _db.AuthClientMetadata
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == clientId, cancellationToken);
			if (meta is null)
			{
				return NotFound();
			}

			if (meta.IsSystem)
			{
				return BadRequest(new { error = "system_client_not_deletable" });
			}

			meta.MarkDeleted();
			meta.IsEnabled = false;
			await _db.SaveChangesAsync(cancellationToken);

			var app = await _applications.FindByClientIdAsync(clientId, cancellationToken);
			if (app is not null)
			{
				await _applications.DeleteAsync(app, cancellationToken);
			}

			return Ok(new { deleted = true, client_id = clientId });
		}

		[HttpGet("~/api/identity/auth-scopes")]
		public async Task<IActionResult> ListScopes(CancellationToken cancellationToken)
		{
			var names = new List<object>();
			await foreach (var scope in _scopes.ListAsync(cancellationToken: cancellationToken))
			{
				names.Add(new
				{
					name = await _scopes.GetNameAsync(scope, cancellationToken),
					display_name = await _scopes.GetDisplayNameAsync(scope, cancellationToken)
				});
			}

			return Ok(names);
		}

		public sealed class CreateScopeRequest
		{
			[Required]
			public string Name { get; set; } = string.Empty;
			public string? DisplayName { get; set; }
		}

		[HttpPost("~/api/identity/auth-scopes")]
		public async Task<IActionResult> CreateScope([FromBody] CreateScopeRequest request, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(request.Name))
			{
				return BadRequest(new { error = "name_required" });
			}

			// Convention: product scopes use {product_code}.{feature}
			if (await _scopes.FindByNameAsync(request.Name, cancellationToken) is not null)
			{
				return Conflict(new { error = "scope_exists" });
			}

			await _scopes.CreateAsync(new OpenIddictScopeDescriptor
			{
				Name = request.Name,
				DisplayName = request.DisplayName ?? request.Name
			}, cancellationToken);

			return Ok(new { name = request.Name });
		}

		private async Task<object> MapAsync(
			object? app,
			AuthClientMetadataEntity? meta,
			Guid? productId,
			bool includeSecret)
		{
			if (app is null)
			{
				return new
				{
					client_id = meta?.ClientId,
					display_name = meta?.DisplayName,
					is_system = meta?.IsSystem,
					is_first_party = meta?.IsFirstParty,
					is_enabled = meta?.IsEnabled,
					require_consent = meta?.RequireConsent,
					consent_remember_days = meta?.ConsentRememberDays,
					product_id = productId,
					orphan_metadata = true
				};
			}

			var redirectUris = new List<string>();
			foreach (var uri in await _applications.GetRedirectUrisAsync(app))
			{
				redirectUris.Add(uri);
			}

			return new
			{
				client_id = await _applications.GetClientIdAsync(app),
				display_name = meta?.DisplayName ?? await _applications.GetDisplayNameAsync(app),
				client_type = await _applications.GetClientTypeAsync(app),
				consent_type = await _applications.GetConsentTypeAsync(app),
				is_system = meta?.IsSystem ?? false,
				is_first_party = meta?.IsFirstParty ?? false,
				is_enabled = meta?.IsEnabled ?? true,
				require_consent = meta?.RequireConsent ?? AuthClientConsentPolicies.Never,
				consent_remember_days = meta?.ConsentRememberDays ?? 180,
				redirect_uris = redirectUris,
				product_id = productId
			};
		}

		private bool ValidateRedirects(IEnumerable<string> uris, out string? error)
		{
			error = null;
			foreach (var raw in uris)
			{
				if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
				{
					error = "invalid_redirect_uri";
					return false;
				}

				if (_environment.IsProduction()
					&& !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
				{
					error = "https_required_in_production";
					return false;
				}

				if (_environment.IsProduction()
					&& (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
						|| uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
				{
					error = "localhost_not_allowed_in_production";
					return false;
				}
			}

			return true;
		}

		private static string MapConsentType(string policy)
			=> policy switch
			{
				AuthClientConsentPolicies.Never => ConsentTypes.Implicit,
				AuthClientConsentPolicies.Always => ConsentTypes.Explicit,
				_ => ConsentTypes.External
			};

		private static void ApplyPermissions(OpenIddictApplicationDescriptor descriptor, CreateRequest request)
		{
			descriptor.Permissions.Add(Permissions.Endpoints.Token);
			descriptor.Permissions.Add(Permissions.Endpoints.Revocation);

			if (request.AllowAuthorizationCode)
			{
				descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
				descriptor.Permissions.Add(Permissions.Endpoints.EndSession);
				descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
				descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
				descriptor.Permissions.Add(Permissions.Prefixes.GrantType + SsoGrantTypes.SwitchContext);
				descriptor.Permissions.Add(Permissions.ResponseTypes.Code);
				descriptor.Permissions.Add(Permissions.Scopes.Email);
				descriptor.Permissions.Add(Permissions.Scopes.Profile);
			}

			if (request.AllowClientCredentials
				|| string.Equals(request.ClientType, ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase))
			{
				descriptor.Permissions.Add(Permissions.GrantTypes.ClientCredentials);
			}

			foreach (var scope in request.AllowedScopes.Distinct(StringComparer.Ordinal))
			{
				if (string.Equals(scope, Scopes.OpenId, StringComparison.Ordinal))
				{
					continue;
				}

				descriptor.Permissions.Add(Permissions.Prefixes.Scope + scope);
			}
		}

		private async Task EnsureScopesExistAsync(IEnumerable<string> scopes, CancellationToken cancellationToken)
		{
			foreach (var name in scopes.Distinct(StringComparer.Ordinal))
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				if (await _scopes.FindByNameAsync(name, cancellationToken) is null)
				{
					await _scopes.CreateAsync(new OpenIddictScopeDescriptor
					{
						Name = name,
						DisplayName = name
					}, cancellationToken);
				}
			}
		}

		private static string GenerateSecret()
		{
			var bytes = RandomNumberGenerator.GetBytes(32);
			return Convert.ToBase64String(bytes);
		}
	}
}
