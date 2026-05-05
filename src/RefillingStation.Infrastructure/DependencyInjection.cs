using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace RefillingStation.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register your infrastructure services here
            // For example:
            // services.AddScoped<IMyRepository, MyRepository>();
            return services;
        }
    }
}
