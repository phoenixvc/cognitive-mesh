using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.FoundationLayer.EnterpriseConnectors
{
    /// <summary>
    /// Implements a circuit breaker pattern for resilient API calls.
    /// </summary>
    public class AgentCircuitBreakerPolicy
    {
        private readonly int _failureThreshold;
        private readonly int _resetTimeoutMs;
        private readonly int _successThreshold;
        private readonly ILogger _logger;
        
        private int _failureCount = 0;
        private int _successCount = 0;
        private CircuitState _state = CircuitState.Closed;
        private DateTime _lastFailureTime = DateTime.MinValue;
        private readonly object _lock = new object();

        /// <summary>
        /// The current state of the circuit breaker.
        /// </summary>
        public CircuitState State => _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentCircuitBreakerPolicy"/> class.
        /// </summary>
        /// <param name="failureThreshold">Number of failures before opening the circuit.</param>
        /// <param name="resetTimeoutMs">Time in milliseconds before attempting to close the circuit.</param>
        /// <param name="successThreshold">Number of successful operations required to close the circuit.</param>
        /// <param name="logger">Optional logger for circuit breaker events.</param>
        public AgentCircuitBreakerPolicy(
            int failureThreshold = 3, 
            int resetTimeoutMs = 5000, 
            int successThreshold = 3,
            ILogger logger = null)
        {
            _failureThreshold = failureThreshold;
            _resetTimeoutMs = resetTimeoutMs;
            _successThreshold = successThreshold;
            _logger = logger;
        }

        /// <summary>
        /// Executes an action with circuit breaker protection.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="action">The action to execute.</param>
        /// <returns>The result of the action.</returns>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            if (IsOpen())
            {
                _logger?.LogWarning("Circuit breaker is open. Blocking execution.");
                throw new CircuitBrokenException("Circuit breaker is open. Service unavailable.");
            }

            try
            {
                var result = await action().ConfigureAwait(false);
                RecordSuccess();
                return result;
            }
            catch (Exception ex)
            {
                RecordFailure();
                _logger?.LogError(ex, "Error executing action in circuit breaker");
                throw;
            }
        }

        private bool IsOpen()
        {
            if (_state == CircuitState.Closed)
            {
                return false;
            }

            if (_state == CircuitState.Open && 
                DateTime.UtcNow > _lastFailureTime.AddMilliseconds(_resetTimeoutMs))
            {
                _state = CircuitState.HalfOpen;
                _logger?.LogInformation("Circuit breaker moved to Half-Open state");
                return false;
            }

            return true;
        }

        private void RecordFailure()
        {
            lock (_lock)
            {
                _failureCount++;
                _successCount = 0;

                if (_state == CircuitState.HalfOpen || 
                    (_state == CircuitState.Closed && _failureCount >= _failureThreshold))
                {
                    _state = CircuitState.Open;
                    _lastFailureTime = DateTime.UtcNow;
                    _logger?.LogWarning("Circuit breaker tripped to Open state");
                }
            }
        }

        private void RecordSuccess()
        {
            lock (_lock)
            {
                if (_state == CircuitState.HalfOpen)
                {
                    _successCount++;
                    if (_successCount >= _successThreshold)
                    {
                        _state = CircuitState.Closed;
                        _failureCount = 0;
                        _successCount = 0;
                        _logger?.LogInformation("Circuit breaker reset to Closed state");
                    }
                }
                else
                {
                    _failureCount = 0;
                }
            }
        }
    }

    /// <summary>
    /// Represents the state of a circuit breaker.
    /// </summary>
    public enum CircuitState
    {
        /// <summary>
        /// The circuit is closed and operations are allowed.
        /// </summary>
        Closed,
        
        /// <summary>
        /// The circuit is open and operations are blocked.
        /// </summary>
        Open,
        
        /// <summary>
        /// The circuit is in a test state to determine if it should be closed.
        /// </summary>
        HalfOpen
    }

    /// <summary>
    /// Exception thrown when the circuit breaker is open.
    /// </summary>
    public class CircuitBrokenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBrokenException"/> class.
        /// </summary>
        public CircuitBrokenException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBrokenException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CircuitBrokenException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBrokenException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CircuitBrokenException(string message, Exception innerException) : base(message, innerException) { }
    }
}
