using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CognitiveMesh.FoundationLayer.AuditLogging.Exceptions;
using CognitiveMesh.FoundationLayer.AuditLogging.Interfaces;
using CognitiveMesh.FoundationLayer.AuditLogging.Models;
using Microsoft.EntityFrameworkCore;

namespace CognitiveMesh.FoundationLayer.AuditLogging
{
    /// <summary>
    /// Adapter for logging agent-related audit events to a persistent store.
    /// Implements circuit breaker pattern and retry logic for resilient event logging.
    /// </summary>
    public class AuditLoggingAdapter : IAuditLoggingAdapter
    {
        private readonly ILogger<AuditLoggingAdapter> _logger;
        private readonly IAuditEventRepository _repository;
        private readonly IRetryPolicy _retryPolicy;
        private readonly AuditLoggingOptions _options;
        private readonly ConcurrentQueue<AuditEvent> _retryQueue = new();
        private readonly SemaphoreSlim _queueLock = new(1, 1);
        private readonly Timer _retryTimer;
        private bool _disposed;
        private readonly CancellationTokenSource _shutdownCts = new();

        /// <summary>
        /// Initializes a new instance of the AuditLoggingAdapter class.
        /// </summary>
        /// <param name="repository">The audit event repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="retryPolicy">The retry policy (optional).</param>
        public AuditLoggingAdapter(
            IAuditEventRepository repository,
            ILogger<AuditLoggingAdapter> logger,
            IOptions<AuditLoggingOptions> options,
            IRetryPolicy? retryPolicy = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _retryPolicy = retryPolicy ?? new ExponentialBackoffRetryPolicy(
                _options.MaxRetryAttempts, 
                _options.InitialRetryDelayMs,
                logger);
                
            // Initialize the retry timer with configured interval
            var interval = TimeSpan.FromSeconds(_options.RetryQueueProcessingIntervalSeconds);
            _retryTimer = new Timer(
                callback: _ => _ = ProcessRetryQueueAsync(), 
                state: null, 
                dueTime: interval, 
                period: interval);
        }

        /// <inheritdoc />
        public async Task LogEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        {
            if (auditEvent == null)
                throw new ArgumentNullException(nameof(auditEvent));

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Try to log the event directly first
                await _retryPolicy.ExecuteAsync(
                    async () => await _repository.AddEventAsync(auditEvent, cancellationToken).ConfigureAwait(false),
                    cancellationToken).ConfigureAwait(false);
                
                _logger.LogDebug("Successfully logged audit event {EventId}", auditEvent.EventId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Audit event logging was canceled for event {EventId}", auditEvent.EventId);
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to log audit event {EventId}, adding to retry queue", auditEvent.EventId);
                
                // Add to retry queue for background processing
                await _queueLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    _retryQueue.Enqueue(auditEvent);
                }
                finally
                {
                    _queueLock.Release();
                }

                throw new AuditLoggingException("Failed to log audit event and added to retry queue", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditEvent>> SearchEventsAsync(AuditSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            try
            {
                return await _retryPolicy.ExecuteAsync(
                    operation: () => _repository.SearchEventsAsync(criteria, cancellationToken),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Audit event search was canceled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search audit events");
                throw new AuditLoggingException("Failed to search audit events", ex);
            }
        }

        public async Task<AuditEvent?> GetEventByIdAsync(string eventId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(eventId))
                throw new ArgumentException("Event ID cannot be null or whitespace.", nameof(eventId));

            try
            {
                return await _retryPolicy.ExecuteAsync(
                    operation: () => _repository.GetEventByIdAsync(eventId, cancellationToken),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Get audit event by ID was canceled for event {EventId}", eventId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve audit event with ID {EventId}", eventId);
                throw new AuditLoggingException($"Failed to retrieve audit event with ID {eventId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditEvent>> GetEventsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentException("Correlation ID cannot be null or whitespace.", nameof(correlationId));

            try
            {
                var criteria = new AuditSearchCriteria { CorrelationId = correlationId };
                return await _retryPolicy.ExecuteAsync(
                    operation: () => _repository.SearchEventsAsync(criteria, cancellationToken),
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Get audit events by correlation ID was canceled for {CorrelationId}", correlationId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve audit events with correlation ID {CorrelationId}", correlationId);
                throw new AuditLoggingException($"Failed to retrieve audit events with correlation ID {correlationId}", ex);
            }
        }

        /// <summary>
        /// Enqueues an audit event for retry.
        /// </summary>
        /// <param name="auditEvent">The audit event to enqueue</param>
        private async Task EnqueueForRetryAsync(AuditEvent auditEvent)
        {
            if (auditEvent == null)
                return;

            try
            {
                await _queueLock.WaitAsync();
                _retryQueue.Enqueue(auditEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue audit event {EventId} for retry", auditEvent.EventId);
                throw;
            }
            finally
            {
                _queueLock.Release();
            }
        }

        /// <summary>
        /// Processes the retry queue.
        /// </summary>
        private async void ProcessRetryQueueAsync(object? state)
        {
            if (_disposed) 
            {
                _logger.LogDebug("Skipping retry queue processing - adapter is disposed");
                return;
            }

            if (!await _queueLock.WaitAsync(0, _shutdownCts.Token).ConfigureAwait(false))
            {
                _logger.LogDebug("Skipping retry queue processing - another operation is in progress");
                return;
            }

            try
            {
                var eventsToRetry = new List<AuditEvent>();
                int batchSize = Math.Min(_options.MaxRetryBatchSize, 100); // Cap at 100 for safety
                
                // Dequeue up to the batch size
                while (eventsToRetry.Count < batchSize && _retryQueue.TryDequeue(out var auditEvent))
                {
                    eventsToRetry.Add(auditEvent);
                }

                if (eventsToRetry.Count == 0)
                {
                    _logger.LogDebug("No events to retry in the queue");
                    return;
                }

                _logger.LogInformation("Retrying {Count} failed audit events", eventsToRetry.Count);

                // Process events in parallel with degree of parallelism based on batch size
                var options = new ParallelOptions 
                { 
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2),
                    CancellationToken = _shutdownCts.Token
                };

                await Parallel.ForEachAsync(eventsToRetry, options, async (auditEvent, ct) =>
                {
                    try
                    {
                        await _retryPolicy.ExecuteAsync(async () =>
                        {
                            await _repository.AddEventAsync(auditEvent);
                            _logger.LogDebug("Successfully retried audit event {EventId}", auditEvent.EventId);
                        }, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        // Operation was canceled, re-queue the event
                        _retryQueue.Enqueue(auditEvent);
                        _logger.LogWarning("Retry operation was canceled for event {EventId}, re-queuing", auditEvent.EventId);
                    }
                    catch (Exception ex)
                    {
                        _retryQueue.Enqueue(auditEvent); // Re-queue for next attempt
                        _logger.LogError(ex, "Failed to retry audit event {EventId}, re-queued for next attempt", auditEvent.EventId);
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unexpected error processing retry queue");
            }
            finally
            {
                try
                {
                    _queueLock.Release();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore if the semaphore is already disposed
                }
            }
        }

        /// <summary>
        /// Disposes the adapter.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            // Fire and forget the async dispose
            _ = DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_disposed) 
                return;

            // Signal cancellation for any in-progress operations
            _shutdownCts.Cancel();

            // Stop the timer first to prevent new operations
            _retryTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _retryTimer?.Dispose();

            // Process any remaining items in the queue
            await ProcessRemainingEventsAsync().ConfigureAwait(false);

            _disposed = true;
        }

        private async Task ProcessRemainingEventsAsync()
        {
            if (_retryQueue.IsEmpty)
                return;

            _logger.LogInformation("Processing {Count} remaining audit events before shutdown", _retryQueue.Count);
            
            try
            {
                // Try to process remaining events with a timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _shutdownCts.Token);
                
                await ProcessRetryQueueAsync(null).ConfigureAwait(false);
                
                // Wait briefly to allow in-progress operations to complete
                await Task.Delay(1000, linkedCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timed out while processing remaining audit events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing remaining audit events");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _shutdownCts.Cancel();
                    _retryTimer?.Dispose();
                    _queueLock?.Dispose();
                    _shutdownCts.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AuditLoggingAdapter"/> class.
        /// </summary>
        ~AuditLoggingAdapter()
        {
            Dispose(false);
        }
    }
 
}
