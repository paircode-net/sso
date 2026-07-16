using Microsoft.AspNetCore.Mvc;
using SSO.Middleware;
using SSO.Middleware.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMiddleware(builder.Configuration, typeof(Program).Assembly, builder.Environment);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
	options.Conventions.AddAreaFolderApplicationModelConvention(
		"Admin",
		"/",
		model => model.Filters.Add(new ServiceFilterAttribute(typeof(AdminPortalPageFilter))));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Account/Login";
	options.LogoutPath = "/Account/Login";
	options.AccessDeniedPath = "/Account/Login";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware();

app.MapControllers();
app.MapRazorPages();

app.Run();
