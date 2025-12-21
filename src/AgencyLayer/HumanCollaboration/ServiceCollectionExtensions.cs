using System;
using Microsoft.Extensions.DependencyInjection;
using CognitiveMesh.AgencyLayer.HumanCollaboration.Features.Messages;

namespace CognitiveMesh.AgencyLayer.HumanCollaboration
{
    /// <summary>
    /// Extension methods for registering Human Collaboration services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the human collaboration services and MediatR handlers.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddHumanCollaborationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            });

            services.AddScoped<ICollaborationManager, CollaborationManager>();

            return services;
        }
    }
}
