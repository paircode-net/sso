using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SSO.Shared.Identity;

namespace SSO.Client
{
	/// <summary>
	/// OAuth token helper for refresh and switch_context against the SSO authority.
	/// </summary>
	public sealed class SsoTokenClient
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		private readonly HttpClient _http;
		private readonly SsoClientOptions _options;

		public SsoTokenClient(HttpClient http, IOptions<SsoClientOptions> options)
		{
			_http = http;
			_options = options.Value;
		}

		public async Task<SsoTokenResponse> RefreshAsync(
			string refreshToken,
			CancellationToken cancellationToken = default)
		{
			var form = new Dictionary<string, string>
			{
				["grant_type"] = "refresh_token",
				["refresh_token"] = refreshToken,
				["client_id"] = _options.ClientId
			};

			if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
			{
				form["client_secret"] = _options.ClientSecret!;
			}

			return await PostTokenAsync(form, cancellationToken);
		}

		public async Task<SsoTokenResponse> SwitchContextAsync(
			string accessToken,
			Guid organizationId,
			Guid? branchId = null,
			CancellationToken cancellationToken = default)
		{
			var form = new Dictionary<string, string>
			{
				["grant_type"] = SsoGrantTypes.SwitchContext,
				["client_id"] = _options.ClientId,
				[SsoClaimTypes.OrganizationId] = organizationId.ToString("D")
			};

			if (branchId is Guid branch)
			{
				form[SsoClaimTypes.BranchId] = branch.ToString("D");
			}

			if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
			{
				form["client_secret"] = _options.ClientSecret!;
			}

			using var request = new HttpRequestMessage(HttpMethod.Post, ResolveTokenEndpoint())
			{
				Content = new FormUrlEncodedContent(form)
			};
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			using var response = await _http.SendAsync(request, cancellationToken);
			var body = await response.Content.ReadAsStringAsync(cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException(
					$"switch_context failed ({(int)response.StatusCode}): {body}");
			}

			return JsonSerializer.Deserialize<SsoTokenResponse>(body, JsonOptions)
				?? throw new InvalidOperationException("Empty token response.");
		}

		private async Task<SsoTokenResponse> PostTokenAsync(
			Dictionary<string, string> form,
			CancellationToken cancellationToken)
		{
			using var response = await _http.PostAsync(
				ResolveTokenEndpoint(),
				new FormUrlEncodedContent(form),
				cancellationToken);
			var body = await response.Content.ReadAsStringAsync(cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException(
					$"token endpoint failed ({(int)response.StatusCode}): {body}");
			}

			return JsonSerializer.Deserialize<SsoTokenResponse>(body, JsonOptions)
				?? throw new InvalidOperationException("Empty token response.");
		}

		private string ResolveTokenEndpoint()
		{
			var authority = _options.Authority.TrimEnd('/');
			return authority + "/connect/token";
		}
	}
}
