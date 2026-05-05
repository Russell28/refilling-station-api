using Microsoft.Extensions.DependencyInjection;

namespace RefillingStation.Application
{
    public static class DependencyInjection
    {
        // This method is used to register application services in the dependency injection container.
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            return services;
        }
    }
}
