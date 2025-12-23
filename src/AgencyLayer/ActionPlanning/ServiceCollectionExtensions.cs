using Microsoft.Extensions.DependencyInjection;

namespace AgencyLayer.ActionPlanning
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddActionPlanning(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<IActionPlanner, ActionPlanner>();

            return services;
        }
    }
}
