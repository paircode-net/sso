using System;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Infrastructures.Services;
using SSO.Infrastructures.Services.Identity;
using SSO.Middleware.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SSO.Middleware.AddServices
{
	public static class AddDomainServicesConfigurations
	{
		public static IServiceCollection AddDomainServices(this IServiceCollection services)
		{
			services.AddSingleton<IMailService, MailService>();
			services.AddScoped<IAuthAuditService, AuthAuditService>();
			services.AddScoped<IUserSessionService, UserSessionService>();
			services.AddHttpClient("sso-webhooks", client =>
			{
				client.Timeout = TimeSpan.FromSeconds(10);
			});
			services.AddHostedService<WebhookOutboxSenderHostedService>();

			return services;
		}
	}
}
