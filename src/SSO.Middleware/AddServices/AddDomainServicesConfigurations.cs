
//using SSO.Core.Domain.Interfaces.Infrastructures.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SSO.Middleware.AddServices
{
	public static class AddDomainServicesConfigurations
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
		{
			// Add services
            // services.AddTransient<IMailService, MailService>();

	        return services;
	    }
	}
}
