using SSO.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSsoAuthentication(builder.Configuration);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.Cookie.Name = ".SsoBff.Session";
	options.Cookie.HttpOnly = true;
	options.Cookie.SameSite = SameSiteMode.Lax;
	options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddHttpClient("product");

var app = builder.Build();

app.UseSession();

const string AccessKey = "bff.access_token";
const string RefreshKey = "bff.refresh_token";

app.MapGet("/health", () => Results.Ok(new { status = "ok", role = "bff" }));

// SPA stores tokens in BFF session after OIDC code exchange (never expose refresh to JS long-term).
app.MapPost("/bff/session", async (HttpContext http, SessionTokens body) =>
{
	if (string.IsNullOrWhiteSpace(body.AccessToken))
	{
		return Results.BadRequest("access_token required");
	}

	http.Session.SetString(AccessKey, body.AccessToken);
	if (!string.IsNullOrWhiteSpace(body.RefreshToken))
	{
		http.Session.SetString(RefreshKey, body.RefreshToken!);
	}

	await http.Session.CommitAsync();
	return Results.Ok(new { stored = true });
});

app.MapPost("/bff/logout", async (HttpContext http) =>
{
	http.Session.Remove(AccessKey);
	http.Session.Remove(RefreshKey);
	await http.Session.CommitAsync();
	return Results.Ok(new { cleared = true });
});

app.MapGet("/bff/me", async (HttpContext http, IHttpClientFactory httpClientFactory, IConfiguration config) =>
{
	var access = http.Session.GetString(AccessKey);
	if (string.IsNullOrWhiteSpace(access))
	{
		return Results.Unauthorized();
	}

	var meUrl = config["ProductApi:MeUrl"] ?? "http://localhost:5101/me";
	var client = httpClientFactory.CreateClient("product");
	using var req = new HttpRequestMessage(HttpMethod.Get, meUrl);
	req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
	using var response = await client.SendAsync(req);
	var body = await response.Content.ReadAsStringAsync();
	return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
});

app.MapPost("/bff/switch-context", async (HttpContext http, SsoTokenClient tokens, SwitchBody body) =>
{
	var access = http.Session.GetString(AccessKey);
	if (string.IsNullOrWhiteSpace(access))
	{
		return Results.Unauthorized();
	}

	try
	{
		var result = await tokens.SwitchContextAsync(access, body.OrganizationId, body.BranchId);
		http.Session.SetString(AccessKey, result.AccessToken);
		if (!string.IsNullOrWhiteSpace(result.RefreshToken))
		{
			http.Session.SetString(RefreshKey, result.RefreshToken!);
		}

		await http.Session.CommitAsync();
		return Results.Ok(new { switched = true, expires_in = result.ExpiresIn });
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { error = ex.Message });
	}
});

app.MapPost("/bff/refresh", async (HttpContext http, SsoTokenClient tokens) =>
{
	var refresh = http.Session.GetString(RefreshKey);
	if (string.IsNullOrWhiteSpace(refresh))
	{
		return Results.Unauthorized();
	}

	try
	{
		var result = await tokens.RefreshAsync(refresh);
		http.Session.SetString(AccessKey, result.AccessToken);
		if (!string.IsNullOrWhiteSpace(result.RefreshToken))
		{
			http.Session.SetString(RefreshKey, result.RefreshToken!);
		}

		await http.Session.CommitAsync();
		return Results.Ok(new { refreshed = true, expires_in = result.ExpiresIn });
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { error = ex.Message });
	}
});

app.Run();

public sealed record SessionTokens(string AccessToken, string? RefreshToken);
public sealed record SwitchBody(Guid OrganizationId, Guid? BranchId);

public partial class Program { }
