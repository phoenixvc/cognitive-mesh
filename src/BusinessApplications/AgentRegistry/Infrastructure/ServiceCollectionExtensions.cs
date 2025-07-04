using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using SendGrid.Extensions.DependencyInjection;
using CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports;
using CognitiveMesh.BusinessApplications.AgentRegistry.Services;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.FoundationLayer.AuditLogging;
using CognitiveMesh.FoundationLayer.AuditLogging.Services;
using CognitiveMesh.FoundationLayer.Notifications;
using CognitiveMesh.FoundationLayer.Notifications.Services;

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
            var auditDbConnection = configuration.GetConnectionString("AuditDb");
            
            // Validate connection strings
            if (string.IsNullOrEmpty(agentDbConnection))
                throw new InvalidOperationException("AgentDb connection string is not configured.");
            
            if (string.IsNullOrEmpty(authorityDbConnection))
                throw new InvalidOperationException("AuthorityDb connection string is not configured.");
            
            if (string.IsNullOrEmpty(consentDbConnection))
                throw new InvalidOperationException("ConsentDb connection string is not configured.");
            
            if (string.IsNullOrEmpty(auditDbConnection))
                throw new InvalidOperationException("AuditDb connection string is not configured.");
            
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
            
            services.AddDbContext<AuditDbContext>(options =>
            {
                options.UseSqlServer(auditDbConnection, sqlOptions =>
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
                
                // Configure adapter options
                var options = new AuditLoggingOptions
                {
                    QueueCapacity = configuration.GetValue<int>("AuditLogging:QueueCapacity", 10000),
                    RetryIntervalMs = configuration.GetValue<int>("AuditLogging:RetryIntervalMs", 5000),
                    MaxRetryAttempts = configuration.GetValue<int>("AuditLogging:MaxRetryAttempts", 5)
                };
                
                return new AuditLoggingAdapter(repository, options, logger);
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
            
            // Add SendGrid client
            services.AddSendGrid(options =>
            {
                options.ApiKey = configuration["SendGrid:ApiKey"];
            });
            
            // Register notification delivery service with singleton lifetime
            services.AddSingleton<INotificationDeliveryService, SendGridNotificationService>();
            
            // Register notification adapter with singleton lifetime
            services.AddSingleton<INotificationAdapter, NotificationAdapter>(sp =>
            {
                var deliveryService = sp.GetRequiredService<INotificationDeliveryService>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NotificationAdapter>>();
                
                // Configure adapter options
                var options = new NotificationAdapterOptions
                {
                    QueueCapacity = configuration.GetValue<int>("Notifications:QueueCapacity", 10000),
                    RetryIntervalMs = configuration.GetValue<int>("Notifications:RetryIntervalMs", 5000),
                    MaxRetryAttempts = configuration.GetValue<int>("Notifications:MaxRetryAttempts", 5)
                };
                
                return new NotificationAdapter(deliveryService, options, logger);
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
            
            // Example:
            // services.AddScoped<IMultiAgentOrchestrationPort, MultiAgentOrchestrationEngine>();
            
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
                .AddDbContextCheck<ConsentDbContext>("consent_db_health")
                .AddDbContextCheck<AuditDbContext>("audit_db_health");
            
            return services;
        }
    }

    /// <summary>
    /// Options for configuring the audit logging adapter.
    /// </summary>
    public class AuditLoggingOptions
    {
        /// <summary>
        /// Maximum capacity of the audit event queue.
        /// </summary>
        public int QueueCapacity { get; set; } = 10000;
        
        /// <summary>
        /// Interval in milliseconds between retry attempts.
        /// </summary>
        public int RetryIntervalMs { get; set; } = 5000;
        
        /// <summary>
        /// Maximum number of retry attempts.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 5;
    }

    /// <summary>
    /// Options for configuring the notification adapter.
    /// </summary>
    public class NotificationAdapterOptions
    {
        /// <summary>
        /// Maximum capacity of the notification queue.
        /// </summary>
        public int QueueCapacity { get; set; } = 10000;
        
        /// <summary>
        /// Interval in milliseconds between retry attempts.
        /// </summary>
        public int RetryIntervalMs { get; set; } = 5000;
        
        /// <summary>
        /// Maximum number of retry attempts.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 5;
    }
}
