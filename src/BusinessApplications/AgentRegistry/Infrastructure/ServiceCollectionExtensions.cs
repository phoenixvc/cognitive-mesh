using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AgencyLayer.MultiAgentOrchestration.Ports;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports;
using CognitiveMesh.BusinessApplications.AgentRegistry.Services;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.FoundationLayer.EnterpriseConnectors;
using CognitiveMesh.FoundationLayer.OneLakeIntegration;
using FoundationLayer.AuditLogging;
using FoundationLayer.AuditLogging.Services;
using FoundationLayer.Notifications;
using FoundationLayer.Notifications.Services;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Infrastructure
{
    /// <summary>
    /// Extension methods for configuring Agentic AI System services in the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all Agentic AI System services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAgenticAiSystem(this IServiceCollection services, IConfiguration configuration)
        {
            // Add database contexts
            services.AddAgenticDbContexts(configuration);

            // Add ports and services
            services.AddAgentRegistryServices();

            // Add foundation layer services
            services.AddAuditLoggingServices(configuration);
            services.AddNotificationServices(configuration);

            // Add Enterprise Connectors and OneLake Integration
            services.AddEnterpriseConnectors(configuration);
            services.AddOneLakeIntegration(configuration);

            return services;
        }

        /// <summary>
        /// Adds database contexts for the Agentic AI System.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAgenticDbContexts(this IServiceCollection services, IConfiguration configuration)
        {
            // Get connection strings from configuration
            var agentDbConnection = configuration.GetConnectionString("AgentDb");
            var authorityDbConnection = configuration.GetConnectionString("AuthorityDb");
            var consentDbConnection = configuration.GetConnectionString("ConsentDb");

            // Validate connection strings
            if (string.IsNullOrEmpty(agentDbConnection))
                throw new InvalidOperationException("AgentDb connection string is not configured.");

            if (string.IsNullOrEmpty(authorityDbConnection))
                throw new InvalidOperationException("AuthorityDb connection string is not configured.");

            if (string.IsNullOrEmpty(consentDbConnection))
                throw new InvalidOperationException("ConsentDb connection string is not configured.");

            // Add DbContexts with scoped lifetime (per request)
            services.AddDbContext<AgentDbContext>(options =>
            {
                options.UseSqlServer(agentDbConnection, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                });
            });

            services.AddDbContext<AuthorityDbContext>(options =>
            {
                options.UseSqlServer(authorityDbConnection, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                });
            });

            services.AddDbContext<ConsentDbContext>(options =>
            {
                options.UseSqlServer(consentDbConnection, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(30);
                });
            });

            return services;
        }

        /// <summary>
        /// Adds agent registry services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAgentRegistryServices(this IServiceCollection services)
        {
            // Register ports and their implementations with scoped lifetime
            services.AddScoped<IAgentRegistryPort, AgentRegistryService>();
            services.AddScoped<IAuthorityPort, AuthorityService>();
            services.AddScoped<IAgentConsentPort, AgentConsentService>();

            return services;
        }

        /// <summary>
        /// Adds audit logging services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAuditLoggingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register audit repository with singleton lifetime
            services.AddSingleton<IAuditEventRepository, AuditEventRepository>();

            // Register audit logging adapter with singleton lifetime
            services.AddSingleton<IAuditLoggingAdapter, AuditLoggingAdapter>(sp =>
            {
                var repository = sp.GetRequiredService<IAuditEventRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AuditLoggingAdapter>>();

                return new AuditLoggingAdapter(repository, logger);
            });

            return services;
        }

        /// <summary>
        /// Adds notification services to the service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure SendGrid options
            services.Configure<SendGridOptions>(configuration.GetSection("SendGrid"));

            // Register notification delivery service with singleton lifetime
            services.AddSingleton<INotificationDeliveryService, SendGridNotificationService>();

            // Register notification adapter with singleton lifetime
            services.AddSingleton<INotificationAdapter, NotificationAdapter>(sp =>
            {
                var deliveryService = sp.GetRequiredService<INotificationDeliveryService>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NotificationAdapter>>();

                return new NotificationAdapter(deliveryService, logger);
            });

            return services;
        }

        /// <summary>
        /// Adds multi-agent orchestration services to the service collection.
        /// This is used by the AgentController for task orchestration.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddMultiAgentOrchestrationServices(this IServiceCollection services)
        {
            // Register orchestration port and implementation
            // Note: The actual implementation should be provided by the Agency Layer
            // This is just a placeholder to ensure the dependency is registered

            return services;
        }

        /// <summary>
        /// Adds health checks for the Agentic AI System.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAgenticHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<AgentDbContext>("agent_db_health")
                .AddDbContextCheck<AuthorityDbContext>("authority_db_health")
                .AddDbContextCheck<ConsentDbContext>("consent_db_health");

            return services;
        }
    }
}
