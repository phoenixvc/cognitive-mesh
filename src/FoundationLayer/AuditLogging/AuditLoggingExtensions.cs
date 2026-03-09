using System;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CognitiveMesh.FoundationLayer.AuditLogging.Exceptions;
using CognitiveMesh.FoundationLayer.AuditLogging.HealthChecks;
using CognitiveMesh.FoundationLayer.AuditLogging.Interfaces;
using CognitiveMesh.FoundationLayer.AuditLogging.Models;
using CognitiveMesh.FoundationLayer.AuditLogging.Policies;
using CognitiveMesh.FoundationLayer.AuditLogging.Repositories;

namespace CognitiveMesh.FoundationLayer.AuditLogging
{

    /// <summary>
    /// Extension methods for setting up audit logging services in an <see cref="IServiceCollection"/>
    /// </summary>
    public static class AuditLoggingExtensions
    {
        /// <summary>
        /// Adds audit logging services to the service collection with default configuration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The configuration containing the audit logging settings.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when required configuration is missing or invalid.</exception>
        public static IServiceCollection AddAuditLogging(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Configure options from configuration with validation
            services.AddOptions<AuditLoggingOptions>()
                .Bind(configuration.GetSection("AuditLogging"))
                .ValidateDataAnnotations()
                .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), 
                    "Audit logging connection string must be configured")
                .Validate(options => options.MaxRetryAttempts > 0, 
                    "Max retry attempts must be greater than zero")
                .Validate(options => options.InitialRetryDelayMs > 0, 
                    "Initial retry delay must be greater than zero")
                .Validate(options => options.CommandTimeoutSeconds > 0,
                    "Command timeout must be greater than zero");

            // Register the audit logging database context with retry on failure
            services.AddDbContext<AuditLoggingDbContext>((serviceProvider, options) =>
            {
                var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AuditLoggingOptions>>();
                var auditOptions = optionsMonitor.CurrentValue;

                if (string.IsNullOrEmpty(auditOptions.ConnectionString))
                    throw new InvalidOperationException("Audit logging connection string is not configured.");

                options.UseSqlServer(
                    connectionString: auditOptions.ConnectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: Math.Max(1, auditOptions.MaxRetryAttempts),
                            maxRetryDelay: TimeSpan.FromMilliseconds(auditOptions.InitialRetryDelayMs * 10),
                            errorNumbersToAdd: new[] { 4060, 40197, 40501, 49918, 49919, 49920, 11001 });
                        
                        sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "audit");
                        sqlOptions.CommandTimeout(auditOptions.CommandTimeoutSeconds);
                    });

                // Enable detailed errors and sensitive data logging in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            // Register the audit event repository with scoped lifetime
            services.AddScoped<IAuditEventRepository, EfCoreAuditEventRepository>();

            // Register the retry policy as a singleton
            services.AddSingleton<IRetryPolicy>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AuditLoggingOptions>>().Value;
                var logger = serviceProvider.GetService<ILogger<ExponentialBackoffRetryPolicy>>();
                
                return new ExponentialBackoffRetryPolicy(
                    maxRetryAttempts: Math.Max(1, options.MaxRetryAttempts),
                    initialDelayMs: Math.Max(100, options.InitialRetryDelayMs),
                    logger: logger);
            });

            // Register the audit logging adapter with scoped lifetime
            services.AddScoped<IAuditLoggingAdapter, AuditLoggingAdapter>();

            // Register health checks
            services.AddHealthChecks()
                // Database health check
                .AddDbContextCheck<AuditLoggingDbContext>(
                    name: "audit-db",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "audit", "database", "infrastructure" },
                    testQuery: async (context, token) =>
                    {
                        // Execute a simple query to verify the database is responsive
                        var result = await context.Database.ExecuteSqlRawAsync("SELECT 1", token);
                        return result == 1;
                    })
                // Custom audit logging health check
                .AddCheck<AuditLoggingHealthCheck>(
                    "audit-logging",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "audit", "service", "infrastructure" });
                
            // Register the health check service
            services.AddScoped<AuditLoggingHealthCheck>();

            return services;
        }

        /// <summary>
        /// Adds audit logging services to the service collection with a custom configuration action.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configureOptions">A delegate to configure the <see cref="AuditLoggingOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configureOptions"/> is null.</exception>
        public static IServiceCollection AddAuditLogging(this IServiceCollection services, Action<AuditLoggingOptions> configureOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            // Configure options with validation
            services.Configure(configureOptions);
            services.AddOptions<AuditLoggingOptions>()
                .ValidateDataAnnotations()
                .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), 
                    "Audit logging connection string must be configured")
                .Validate(options => options.MaxRetryAttempts > 0, 
                    "Max retry attempts must be greater than zero")
                .Validate(options => options.InitialRetryDelayMs > 0, 
                    "Initial retry delay must be greater than zero")
                .Validate(options => options.CommandTimeoutSeconds > 0,
                    "Command timeout must be greater than zero");

            // Register the audit logging database context
            services.AddDbContext<AuditLoggingDbContext>((serviceProvider, options) =>
            {
                var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AuditLoggingOptions>>();
                var auditOptions = optionsMonitor.CurrentValue;

                options.UseSqlServer(
                    connectionString: auditOptions.ConnectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: Math.Max(1, auditOptions.MaxRetryAttempts),
                            maxRetryDelay: TimeSpan.FromMilliseconds(auditOptions.InitialRetryDelayMs * 10),
                            errorNumbersToAdd: new[] { 4060, 40197, 40501, 49918, 49919, 49920, 11001 });
                        
                        sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "audit");
                        sqlOptions.CommandTimeout(auditOptions.CommandTimeoutSeconds);
                    });

                // Enable detailed errors and sensitive data logging in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            // Register the audit event repository with scoped lifetime
            services.AddScoped<IAuditEventRepository, EfCoreAuditEventRepository>();

            // Register the retry policy as a singleton
            services.AddSingleton<IRetryPolicy>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AuditLoggingOptions>>().Value;
                var logger = serviceProvider.GetService<ILogger<ExponentialBackoffRetryPolicy>>();
                
                return new ExponentialBackoffRetryPolicy(
                    maxRetryAttempts: Math.Max(1, options.MaxRetryAttempts),
                    initialDelayMs: Math.Max(100, options.InitialRetryDelayMs),
                    logger: logger);
            });

            // Register the audit logging adapter with scoped lifetime
            services.AddScoped<IAuditLoggingAdapter, AuditLoggingAdapter>();

            // Register health check for the audit logging database
            services.AddHealthChecks()
                .AddDbContextCheck<AuditLoggingDbContext>(
                    name: "audit-db",
                    tags: new[] { "audit", "database", "infrastructure" });

            return services;
        }

        /// <summary>
        /// Adds audit logging services to the specified <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="configureOptions">A delegate to configure the audit logging options.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddAuditLogging(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<AuditLoggingOptions>? configureOptions = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Register and configure options
            services.Configure<AuditLoggingOptions>(configuration.GetSection("AuditLogging"));
            
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            // Register the DbContext with the connection string from configuration
            services.AddDbContext<AuditLoggingDbContext>((serviceProvider, options) =>
            {
                var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AuditLoggingOptions>>();
                var optionsValue = optionsMonitor.CurrentValue;
                
                options.UseSqlServer(
                    optionsValue.ConnectionString ?? 
                    throw new InvalidOperationException("Audit logging connection string is not configured"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            }, ServiceLifetime.Scoped);

            // Register the repository
            services.AddScoped<IAuditEventRepository, EfCoreAuditEventRepository>();

            // Register the retry policy
            services.AddSingleton<IRetryPolicy>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AuditLoggingOptions>>().Value;
                return new ExponentialBackoffRetryPolicy(
                    maxRetryAttempts: options.MaxRetryAttempts,
                    initialDelayMs: options.InitialRetryDelayMs);
            });

            // Register the auditing adapter
            services.AddScoped<IAuditLoggingAdapter, AuditLoggingAdapter>();

            return services;
        }

        /// <summary>
        /// Adds audit logging services to the specified <see cref="IServiceCollection"/>
        /// with a custom repository implementation.
        /// </summary>
        /// <typeparam name="TRepository">The type of the audit event repository.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="lifetime">The service lifetime (defaults to Scoped).</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddAuditLogging<TRepository>(
            this IServiceCollection services,
            IConfiguration configuration,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TRepository : class, IAuditEventRepository
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Register the repository with the specified lifetime
            services.TryAdd(new ServiceDescriptor(
                typeof(IAuditEventRepository),
                typeof(TRepository),
                lifetime));

            // Add the core audit logging services
            return services.AddAuditLogging(configuration);
        }
    }

    /// <summary>
    /// Options for configuring audit logging
    /// </summary>
    public class AuditLoggingOptions
    {
        /// <summary>
        /// Gets or sets the database connection string for audit logging
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the initial delay in milliseconds for retries
        /// </summary>
        public int InitialRetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets whether to enable batch processing of audit events
        /// </summary>
        public bool EnableBatchProcessing { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum batch size for processing audit events
        /// </summary>
        public int MaxBatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the retry queue processing interval in seconds
        /// </summary>
        public int RetryQueueProcessingIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum number of retry queue items to process in a single batch
        /// </summary>
        public int MaxRetryBatchSize { get; set; } = 50;

        /// <summary>
        /// Gets or sets the command timeout in seconds for database operations
        /// </summary>
        public int CommandTimeoutSeconds { get; set; } = 30;
    }
}
