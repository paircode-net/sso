using SSO.Client;
using SSO.Client.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Quickstart: JWT validation + RequirePermission (feature 00004 / SSO.Client)
builder.Services.AddSsoAuthentication(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/me", (HttpContext http) => Results.Ok(new
{
	sub = http.User.GetSubjectId(),
	organization_id = http.User.GetOrganizationId(),
	branch_id = http.User.GetBranchId(),
	perm_ver = http.User.GetPermissionVersion(),
	permissions = http.User.GetPermissions()
})).RequireAuthorization();

app.MapGet("/reports", () => Results.Ok(new { report = "hq" }))
	.RequireAuthorization(new RequiresPermissionAttribute("hq.reports"));

app.Run();

public partial class Program { }
