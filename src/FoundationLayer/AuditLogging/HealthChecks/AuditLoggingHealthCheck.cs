using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using CognitiveMesh.FoundationLayer.AuditLogging.Interfaces;
using CognitiveMesh.FoundationLayer.AuditLogging.Models;
using CognitiveMesh.FoundationLayer.AuditLogging.Enums;

namespace CognitiveMesh.FoundationLayer.AuditLogging.HealthChecks
{
    /// <summary>
    /// Health check for the audit logging system.
    /// </summary>
    public class AuditLoggingHealthCheck : IHealthCheck
    {
        private readonly IAuditEventRepository _repository;
        private readonly ILogger<AuditLoggingHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLoggingHealthCheck"/> class.
        /// </summary>
        /// <param name="repository">The audit event repository.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> or <paramref name="logger"/> is null.</exception>
        public AuditLoggingHealthCheck(
            IAuditEventRepository repository,
            ILogger<AuditLoggingHealthCheck> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting audit logging health check");
                
                // Try to get a recent event as a health check
                var recentEvents = await _repository.SearchEventsAsync(
                    new AuditSearchCriteria 
                    { 
                        PageSize = 1,
                        SortDescending = true
                    }, 
                    cancellationToken);

                _logger.LogDebug("Audit logging health check completed successfully");
                return HealthCheckResult.Healthy("Audit logging service is healthy");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Audit logging health check was canceled");
                return HealthCheckResult.Unhealthy("Audit logging health check was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit logging health check failed");
                return HealthCheckResult.Unhealthy("Audit logging service is unhealthy", ex);
            }
        }
    }
}
