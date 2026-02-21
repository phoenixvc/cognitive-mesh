using System;
using Microsoft.Extensions.DependencyInjection;
using CognitiveMesh.Shared.Interfaces;
using AgencyLayer.ActionPlanning;

namespace CognitiveMesh.AgencyLayer.ActionPlanning
{
    /// <summary>
    /// Extension methods for registering ActionPlanning services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ActionPlanning services to the service collection.
        /// </summary>
        public static IServiceCollection AddActionPlanning(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<IActionPlanner, ActionPlanner>();

            return services;
        }
    }
}
