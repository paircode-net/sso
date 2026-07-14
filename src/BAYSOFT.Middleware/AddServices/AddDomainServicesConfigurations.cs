
//using BAYSOFT.Core.Domain.Interfaces.Infrastructures.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BAYSOFT.Middleware.AddServices
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
