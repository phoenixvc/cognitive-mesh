using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Storage;
using CognitiveMesh.FoundationLayer.AuditLogging.Exceptions;
using CognitiveMesh.FoundationLayer.AuditLogging.Interfaces;
using CognitiveMesh.FoundationLayer.AuditLogging.Models;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Repositories
{
    /// <summary>
    /// An Entity Framework Core implementation of <see cref="IAuditEventRepository"/>
    /// </summary>
    /// <summary>
    /// An Entity Framework Core implementation of <see cref="IAuditEventRepository"/>
    /// </summary>
    public class EfCoreAuditEventRepository : IAuditEventRepository, IDisposable, IAsyncDisposable
    {
        private readonly AuditLoggingDbContext _context;
        private readonly ILogger<EfCoreAuditEventRepository> _logger;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfCoreAuditEventRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger.</param>
        /// <summary>
        /// Initializes a new instance of the <see cref="EfCoreAuditEventRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public EfCoreAuditEventRepository(
            AuditLoggingDbContext context,
            ILogger<EfCoreAuditEventRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public async Task AddEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        {
            if (auditEvent == null)
                throw new ArgumentNullException(nameof(auditEvent));

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Ensure the event has required fields
                if (string.IsNullOrWhiteSpace(auditEvent.EventId))
                    auditEvent.EventId = Guid.NewGuid().ToString();

                if (auditEvent.Timestamp == default)
                    auditEvent.Timestamp = DateTimeOffset.UtcNow;

                _context.AuditEvents.Add(auditEvent);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Successfully added audit event {EventId}", auditEvent.EventId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Add audit event operation was canceled for event {EventId}", auditEvent.EventId);
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while adding audit event {EventId}", auditEvent.EventId);
                throw new AuditEventRepositoryException("A concurrency conflict occurred while adding the audit event.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while adding audit event {EventId}", auditEvent.EventId);
                throw new AuditEventRepositoryException("A database error occurred while adding the audit event.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding audit event {EventId}", auditEvent.EventId);
                throw new AuditEventRepositoryException($"An unexpected error occurred while adding audit event: {auditEvent.EventId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<AuditEvent?> GetEventByIdAsync(string eventId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventId))
                throw new ArgumentException("Event ID cannot be null or whitespace.", nameof(eventId));

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await _context.AuditEvents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);

                if (result == null)
                {
                    _logger.LogDebug("Audit event with ID {EventId} not found", eventId);
                }

                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Get audit event by ID operation was canceled for event {EventId}", eventId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit event with ID {EventId}", eventId);
                throw new AuditEventRepositoryException($"Failed to retrieve audit event with ID {eventId}.", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditEvent>> SearchEventsAsync(AuditSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            try
            {
                var query = _context.AuditEvents.AsNoTracking().AsQueryable();

                // Apply filters based on criteria
                if (!string.IsNullOrEmpty(criteria.UserId))
                    query = query.Where(e => e.UserId == criteria.UserId);

                if (!string.IsNullOrEmpty(criteria.EventType))
                    query = query.Where(e => e.EventType == criteria.EventType);

                if (criteria.StartTime.HasValue)
                    query = query.Where(e => e.Timestamp >= criteria.StartTime.Value);

                if (criteria.EndTime.HasValue)
                    query = query.Where(e => e.Timestamp <= criteria.EndTime.Value);

                // Apply sorting
                query = criteria.SortDescending
                    ? query.OrderByDescending(e => e.Timestamp)
                    : query.OrderBy(e => e.Timestamp);

                // Apply pagination
                if (criteria.PageSize.HasValue && criteria.PageSize.Value > 0 && 
                    criteria.PageNumber.HasValue && criteria.PageNumber.Value > 0)
                {
                    int pageSize = criteria.PageSize.Value;
                    int pageNumber = criteria.PageNumber.Value;
                    
                    query = query
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);
                }
                // If MaxResults is specified, use it as a fallback for backward compatibility
                else if (criteria.MaxResults.HasValue && criteria.MaxResults.Value > 0)
                {
                    query = query.Take(criteria.MaxResults.Value);
                }

                return await query.ToListAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Search audit events operation was canceled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching audit events");
                throw new AuditEventRepositoryException("Error searching audit events", ex);
            }
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="EfCoreAuditEventRepository"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                if (_context != null)
                {
                    await _context.DisposeAsync().ConfigureAwait(false);
                }
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// The database context for audit logging
    /// </summary>
    public class AuditLoggingDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLoggingDbContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public AuditLoggingDbContext(DbContextOptions<AuditLoggingDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the audit events.
        /// </summary>
        public DbSet<AuditEvent> AuditEvents { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditEvent>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventId).HasMaxLength(36);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.TenantId).HasMaxLength(100);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.CorrelationId).HasMaxLength(100);
                
                // Add indexes for commonly queried fields
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CorrelationId);
                entity.HasIndex(e => e.EventType);
            });
        }
    }
}
