using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CognitiveMesh.FoundationLayer.AuditLogging.Exceptions;
using CognitiveMesh.FoundationLayer.AuditLogging.Interfaces;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Policies
{
    /// <summary>
    /// Implements an exponential backoff retry policy with jitter.
    /// </summary>
    /// <summary>
    /// Implements an exponential backoff retry policy with jitter for transient fault handling.
    /// </summary>
    public class ExponentialBackoffRetryPolicy : IRetryPolicy
    {
        private const int MaxJitterMs = 1000; // Maximum jitter to add to delay (1 second)
        private readonly int _maxRetryAttempts;
        private readonly int _initialDelayMs;
        private readonly ILogger<ExponentialBackoffRetryPolicy> _logger;
        private readonly Random _random = new();
        private static readonly HashSet<int> TransientSqlErrorNumbers = new()
        {
            4060,  // Cannot open database
            40197, // Service encountered an error
            40501, // Service is busy
            49918, // Cannot process request
            49919, // Too many operations in progress
            49920, // Too many login requests
            11001  // Server not found
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialBackoffRetryPolicy"/> class.
        /// </summary>
        /// <param name="maxRetryAttempts">The maximum number of retry attempts.</param>
        /// <param name="initialDelayMs">The initial delay in milliseconds.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when maxRetryAttempts or initialDelayMs is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
        public ExponentialBackoffRetryPolicy(int maxRetryAttempts, int initialDelayMs, ILogger<ExponentialBackoffRetryPolicy> logger)
        {
            _maxRetryAttempts = maxRetryAttempts > 0 
                ? maxRetryAttempts 
                : throw new ArgumentOutOfRangeException(nameof(maxRetryAttempts), "Max retry attempts must be greater than zero");
                
            _initialDelayMs = initialDelayMs > 0 
                ? initialDelayMs 
                : throw new ArgumentOutOfRangeException(nameof(initialDelayMs), "Initial delay must be greater than zero");
                
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialBackoffRetryPolicy"/> class with default logger.
        /// </summary>
        /// <param name="maxRetryAttempts">The maximum number of retry attempts.</param>
        /// <param name="initialDelayMs">The initial delay in milliseconds.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when maxRetryAttempts or initialDelayMs is less than or equal to zero.</exception>
        public ExponentialBackoffRetryPolicy(int maxRetryAttempts, int initialDelayMs)
            : this(maxRetryAttempts, initialDelayMs, CreateDefaultLogger())
        {
        }

        private static ILogger<ExponentialBackoffRetryPolicy> CreateDefaultLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
            
            return loggerFactory.CreateLogger<ExponentialBackoffRetryPolicy>();
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            int attempt = 0;
            var exceptions = new List<Exception>();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                try
                {
                    await operation().ConfigureAwait(false);
                    return; // Success - exit the retry loop
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Operation was canceled by the caller");
                    throw;
                }
                catch (Exception ex) when (IsTransientException(ex))
                {
                    exceptions.Add(ex);
                    attempt++;

                    if (attempt > _maxRetryAttempts)
                    {
                        _logger.LogError(ex, "Max retry attempts ({MaxRetries}) reached. Giving up.", _maxRetryAttempts);
                        throw new MaxRetryAttemptsExceededException(
                            $"Maximum number of retry attempts ({_maxRetryAttempts}) exceeded. See inner exception for details.", 
                            new AggregateException(exceptions));
                    }

                    var delay = CalculateBackoffWithJitter(attempt);
                    _logger.LogWarning(ex, "Transient error occurred on attempt {Attempt} of {MaxRetries}. Retrying in {Delay}ms...", 
                        attempt, _maxRetryAttempts, delay);

                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    await operation().ConfigureAwait(false);
                    return;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogDebug("Operation was canceled by the caller");
                    throw;
                }
                catch (Exception ex) when (IsTransientException(ex) && attempt < _maxRetryAttempts)
                {
                    attempt++;
                    exceptions.Add(ex);
                    
                    var delay = CalculateBackoffDelay(attempt);
                    _logger?.LogWarning(ex, 
                        "Operation failed with transient error. Attempt {Attempt} of {MaxAttempts}. Retrying in {Delay}ms...", 
                        attempt, _maxRetryAttempts, delay);

                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger?.LogDebug("Operation was canceled during backoff delay");
                        throw new OperationCanceledException("Operation was canceled during backoff delay", ex, cancellationToken);
                    }
                }
                catch (Exception ex) when (IsTransientException(ex) && attempt >= _maxRetryAttempts)
                {
                    _logger?.LogError(ex, "Operation failed after {MaxAttempts} attempts. Giving up.", _maxRetryAttempts);
                    throw new AggregateException($"Operation failed after {_maxRetryAttempts} attempts. See inner exceptions for details.", exceptions);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            int attempt = 0;
            var exceptions = new List<Exception>();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                try
                {
                    return await operation().ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Operation was canceled by the caller");
                    throw;
                }
                catch (Exception ex) when (IsTransientException(ex))
                {
                    exceptions.Add(ex);
                    attempt++;

                    if (attempt > _maxRetryAttempts)
                    {
                        _logger.LogError(ex, "Max retry attempts ({MaxRetries}) reached. Giving up.", _maxRetryAttempts);
                        throw new MaxRetryAttemptsExceededException(
                            $"Maximum number of retry attempts ({_maxRetryAttempts}) exceeded. See inner exception for details.", 
                            new AggregateException(exceptions));
                    }

                    var delay = CalculateBackoffWithJitter(attempt);
                    _logger.LogWarning(ex, "Transient error occurred on attempt {Attempt} of {MaxRetries}. Retrying in {Delay}ms...", 
                        attempt, _maxRetryAttempts, delay);

                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    return await operation().ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogDebug("Operation was canceled by the caller");
                    throw;
                }
                catch (Exception ex) when (IsTransientException(ex) && attempt < _maxRetryAttempts)
                {
                    attempt++;
                    exceptions.Add(ex);
                    
                    var delay = CalculateBackoffDelay(attempt);
                    _logger?.LogWarning(ex, 
                        "Operation failed with transient error. Attempt {Attempt} of {MaxAttempts}. Retrying in {Delay}ms...", 
                        attempt, _maxRetryAttempts, delay);

                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger?.LogDebug("Operation was canceled during backoff delay");
                        throw new OperationCanceledException("Operation was canceled during backoff delay", ex, cancellationToken);
                    }
                }
                catch (Exception ex) when (IsTransientException(ex) && attempt >= _maxRetryAttempts)
                {
                    _logger?.LogError(ex, "Operation failed after {MaxAttempts} attempts. Giving up.", _maxRetryAttempts);
                    throw new AggregateException($"Operation failed after {_maxRetryAttempts} attempts. See inner exceptions for details.", exceptions);
                }
            }
        }

        /// <summary>
        /// Determines if an exception is transient and should be retried.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        /// <returns>True if the exception is transient; otherwise, false.</returns>
        protected virtual bool IsTransientException(Exception ex)
        {
            if (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                return false;
            }

            // Handle SQL Server transient errors
            if (ex is SqlException sqlEx)
            {
                // SQL Server transient error numbers
                int[] sqlTransientErrors = { 4060, 40197, 40501, 49918, 49919, 49920, 11001 };
                return Array.Exists(sqlTransientErrors, e => e == sqlEx.Number);
            }

            // Handle Cosmos DB transient errors
            if (ex is CosmosException cosmosEx)
            {
                // Cosmos DB transient status codes
                HttpStatusCode[] cosmosTransientStatusCodes =
                {
                    HttpStatusCode.RequestTimeout,          // 408
                    HttpStatusCode.TooManyRequests,         // 429
                    HttpStatusCode.InternalServerError,     // 500
                    HttpStatusCode.ServiceUnavailable,      // 503
                    HttpStatusCode.GatewayTimeout           // 504
                };
                return Array.Exists(cosmosTransientStatusCodes, code => code == cosmosEx.StatusCode);
            }

            // Handle Entity Framework Core transient errors
            if (ex is DbException dbEx)
            {
                return true; // Most DbException types are considered transient
            }

            // Handle HTTP request timeouts and transient network issues
            if (ex is HttpRequestException || ex is TimeoutException)
            {
                return true;
            }

            // Handle aggregate exceptions
            if (ex is AggregateException aggregateEx)
            {
                return aggregateEx.InnerExceptions.Any(IsTransientException);
            }

            return false;
        }

        private int CalculateBackoffDelay(int attempt)
        {
            // Ensure attempt is at least 1 and capped to prevent overflow
            attempt = Math.Max(1, Math.Min(attempt, 30)); // Cap at 30 attempts to prevent overflow
            
            try
            {
                // Exponential backoff with jitter: (2^attempt - 1) * initialDelayMs * (0.8 + 0.4 * random)
                double exponentialBackoff = (Math.Pow(2, attempt) - 1) * _initialDelayMs;
                
                // Add jitter to spread out retry attempts (between 0.8 and 1.2 of the calculated delay)
                double jitter = 0.8 + (_random.NextDouble() * 0.4); // Random value between 0.8 and 1.2
                double delay = exponentialBackoff * jitter;
                
                // Ensure delay is within reasonable bounds
                return (int)Math.Min(delay, int.MaxValue);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error calculating backoff delay. Using default delay.");
                return _initialDelayMs;
            }
        }

        /// <summary>
        /// Calculates the backoff delay with jitter to prevent thundering herd problem.
        /// </summary>
        /// <param name="attempt">The current attempt number (1-based).</param>
        /// <returns>The calculated delay in milliseconds.</returns>
        protected virtual int CalculateBackoffWithJitter(int attempt)
        {
            // Exponential backoff: delay = initialDelay * (2^attempt) with jitter
            // Cap the exponent to prevent overflow
            int exponent = Math.Min(attempt, 30); // 2^30 is a safe upper limit
            double delayMs = _initialDelayMs * Math.Pow(2, exponent - 1);
            
            // Add jitter: random value between 0 and MaxJitterMs
            int jitter = _random.Next(0, MaxJitterMs);
            
            // Cap the total delay to prevent excessive wait times
            int totalDelay = (int)Math.Min(delayMs + jitter, int.MaxValue);
            
            return totalDelay;
        }
    }
}
