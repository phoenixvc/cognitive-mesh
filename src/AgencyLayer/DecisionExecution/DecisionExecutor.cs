using System.Collections.Concurrent;
using System.Diagnostics;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.DecisionExecution
{
    /// <summary>
    /// Handles the execution of decisions made by the cognitive mesh.
    /// Uses the knowledge graph for context retrieval and the LLM client
    /// for reasoning, while tracking execution state and logs internally.
    /// </summary>
    public class DecisionExecutor : IDecisionExecutor
    {
        private readonly ILogger<DecisionExecutor> _logger;
        private readonly IKnowledgeGraphManager _knowledgeGraphManager;
        private readonly ILLMClient _llmClient;
        private readonly ConcurrentDictionary<string, DecisionResult> _executionTracker = new();
        private readonly ConcurrentDictionary<string, DecisionLog> _logBuffer = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionExecutor"/> class.
        /// </summary>
        /// <param name="logger">Logger for structured diagnostic output.</param>
        /// <param name="knowledgeGraphManager">Knowledge graph for contextual lookups.</param>
        /// <param name="llmClient">LLM client for decision reasoning.</param>
        public DecisionExecutor(
            ILogger<DecisionExecutor> logger,
            IKnowledgeGraphManager knowledgeGraphManager,
            ILLMClient llmClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _knowledgeGraphManager = knowledgeGraphManager ?? throw new ArgumentNullException(nameof(knowledgeGraphManager));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        /// <inheritdoc/>
        public async Task<DecisionResult> ExecuteDecisionAsync(
            DecisionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var stopwatch = Stopwatch.StartNew();

            // Mark as executing
            var executingResult = new DecisionResult
            {
                RequestId = request.RequestId,
                Status = DecisionStatus.Executing,
                Outcome = DecisionOutcome.Success,
                Timestamp = DateTime.UtcNow
            };
            _executionTracker[request.RequestId] = executingResult;

            try
            {
                _logger.LogInformation(
                    "Executing decision for request: {RequestId}, type: {DecisionType}, priority: {Priority}",
                    request.RequestId, request.DecisionType, request.Priority);

                // 1. Query the knowledge graph for relevant context
                var contextEntries = await _knowledgeGraphManager.QueryAsync(
                    request.DecisionType ?? "default",
                    cancellationToken);

                var contextData = contextEntries?.ToList() ?? new List<Dictionary<string, object>>();
                _logger.LogDebug(
                    "Knowledge graph returned {ContextCount} entries for decision {RequestId}",
                    contextData.Count, request.RequestId);

                // 2. Build the prompt from request parameters and knowledge context
                var prompt = BuildDecisionPrompt(request, contextData);

                // 3. Invoke the LLM for reasoning
                var llmResponse = await _llmClient.GenerateCompletionAsync(
                    prompt,
                    temperature: 0.3f,
                    maxTokens: 500,
                    cancellationToken: cancellationToken);

                stopwatch.Stop();

                // 4. Build the successful result
                var result = new DecisionResult
                {
                    RequestId = request.RequestId,
                    Status = DecisionStatus.Completed,
                    Outcome = DecisionOutcome.Success,
                    ExecutionTime = stopwatch.Elapsed,
                    Timestamp = DateTime.UtcNow,
                    Results = new Dictionary<string, object>
                    {
                        ["llmResponse"] = llmResponse,
                        ["contextEntriesUsed"] = contextData.Count
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["executionNode"] = Environment.MachineName,
                        ["version"] = "1.0.0",
                        ["model"] = _llmClient.ModelName
                    }
                };

                // 5. Store the execution result for status lookups
                _executionTracker[request.RequestId] = result;

                // 6. Record the decision log entry
                RecordLog(request, result);

                // 7. Persist the decision node to the knowledge graph
                await _knowledgeGraphManager.AddNodeAsync(
                    $"decision:{request.RequestId}",
                    new Dictionary<string, object>
                    {
                        ["requestId"] = request.RequestId,
                        ["decisionType"] = request.DecisionType ?? "unknown",
                        ["outcome"] = result.Outcome.ToString(),
                        ["executionTimeMs"] = result.ExecutionTime.TotalMilliseconds,
                        ["timestamp"] = result.Timestamp.ToString("O")
                    },
                    label: "Decision",
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "Decision {RequestId} completed successfully in {ElapsedMs}ms",
                    request.RequestId, stopwatch.Elapsed.TotalMilliseconds);

                return result;
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                var cancelledResult = new DecisionResult
                {
                    RequestId = request.RequestId,
                    Status = DecisionStatus.Cancelled,
                    Outcome = DecisionOutcome.Error,
                    ErrorMessage = "Decision execution was cancelled",
                    ExecutionTime = stopwatch.Elapsed,
                    Timestamp = DateTime.UtcNow
                };

                _executionTracker[request.RequestId] = cancelledResult;
                RecordLog(request, cancelledResult);

                _logger.LogWarning("Decision {RequestId} was cancelled after {ElapsedMs}ms",
                    request.RequestId, stopwatch.Elapsed.TotalMilliseconds);

                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error executing decision for request: {RequestId}", request.RequestId);

                var failedResult = new DecisionResult
                {
                    RequestId = request.RequestId,
                    Status = DecisionStatus.Failed,
                    Outcome = DecisionOutcome.Error,
                    ErrorMessage = ex.Message,
                    ExecutionTime = stopwatch.Elapsed,
                    Timestamp = DateTime.UtcNow
                };

                _executionTracker[request.RequestId] = failedResult;
                RecordLog(request, failedResult);

                return failedResult;
            }
        }

        /// <inheritdoc/>
        public Task<DecisionResult> GetDecisionStatusAsync(
            string requestId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("Request ID cannot be empty", nameof(requestId));

            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Retrieving status for decision: {RequestId}", requestId);

            if (_executionTracker.TryGetValue(requestId, out var result))
            {
                _logger.LogDebug("Found status for decision {RequestId}: {Status}", requestId, result.Status);
                return Task.FromResult(result);
            }

            _logger.LogWarning("No execution record found for decision {RequestId}", requestId);
            return Task.FromResult(new DecisionResult
            {
                RequestId = requestId,
                Status = DecisionStatus.Pending,
                Outcome = DecisionOutcome.Error,
                ErrorMessage = $"No execution record found for request ID '{requestId}'",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <inheritdoc/>
        public Task<IEnumerable<DecisionLog>> GetDecisionLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Retrieving decision logs (startDate={StartDate}, endDate={EndDate}, limit={Limit})",
                startDate, endDate, limit);

            var effectiveStart = startDate ?? DateTime.MinValue;
            var effectiveEnd = endDate ?? DateTime.MaxValue;

            var logs = _logBuffer.Values
                .Where(log => log.Timestamp >= effectiveStart && log.Timestamp <= effectiveEnd)
                .OrderByDescending(log => log.Timestamp)
                .Take(limit)
                .ToList();

            _logger.LogDebug("Returning {LogCount} decision log entries", logs.Count);
            return Task.FromResult<IEnumerable<DecisionLog>>(logs);
        }

        private static string BuildDecisionPrompt(DecisionRequest request, List<Dictionary<string, object>> contextData)
        {
            var contextSummary = contextData.Count > 0
                ? string.Join("; ", contextData.Select(c => string.Join(", ", c.Select(kvp => $"{kvp.Key}={kvp.Value}"))))
                : "No additional context available.";

            var parameterSummary = request.Parameters.Count > 0
                ? string.Join(", ", request.Parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))
                : "No parameters specified.";

            return $"Decision Type: {request.DecisionType ?? "general"}\n" +
                   $"Parameters: {parameterSummary}\n" +
                   $"Context: {contextSummary}\n" +
                   $"Analyze the above and provide a decision recommendation.";
        }

        private void RecordLog(DecisionRequest request, DecisionResult result)
        {
            var log = new DecisionLog
            {
                RequestId = request.RequestId,
                DecisionType = request.DecisionType ?? "unknown",
                Status = result.Status,
                Outcome = result.Outcome,
                Timestamp = result.Timestamp,
                Metadata = new Dictionary<string, object>
                {
                    ["executionTimeMs"] = result.ExecutionTime.TotalMilliseconds,
                    ["node"] = Environment.MachineName,
                    ["priority"] = request.Priority
                }
            };

            _logBuffer[request.RequestId] = log;
        }
    }

    /// <summary>
    /// Represents a decision execution request
    /// </summary>
    public class DecisionRequest
    {
        /// <summary>
        /// Unique identifier for the request
        /// </summary>
        public string RequestId { get; set; } = $"req-{Guid.NewGuid()}";
        
        /// <summary>
        /// Type of decision to execute
        /// </summary>
        public string DecisionType { get; set; }
        
        /// <summary>
        /// Input parameters for the decision
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        /// <summary>
        /// Priority of the decision (higher values indicate higher priority)
        /// </summary>
        public int Priority { get; set; } = 1;
        
        /// <summary>
        /// When the request was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents the result of a decision execution
    /// </summary>
    public class DecisionResult
    {
        /// <summary>
        /// ID of the original request
        /// </summary>
        public string RequestId { get; set; }
        
        /// <summary>
        /// Current status of the decision
        /// </summary>
        public DecisionStatus Status { get; set; }
        
        /// <summary>
        /// Outcome of the decision
        /// </summary>
        public DecisionOutcome Outcome { get; set; }
        
        /// <summary>
        /// Error message if the decision failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// When the decision was processed
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// How long the decision took to execute
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }
        
        /// <summary>
        /// Results of the decision
        /// </summary>
        public Dictionary<string, object> Results { get; set; } = new();
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a decision log entry
    /// </summary>
    public class DecisionLog
    {
        /// <summary>
        /// ID of the decision request
        /// </summary>
        public string RequestId { get; set; }
        
        /// <summary>
        /// Type of decision
        /// </summary>
        public string DecisionType { get; set; }
        
        /// <summary>
        /// Status of the decision
        /// </summary>
        public DecisionStatus Status { get; set; }
        
        /// <summary>
        /// Outcome of the decision
        /// </summary>
        public DecisionOutcome Outcome { get; set; }
        
        /// <summary>
        /// When the decision was logged
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Status of a decision
    /// </summary>
    public enum DecisionStatus
    {
        /// <summary>
        /// Decision is pending execution
        /// </summary>
        Pending,
        
        /// <summary>
        /// Decision is currently being executed
        /// </summary>
        Executing,
        
        /// <summary>
        /// Decision execution is completed
        /// </summary>
        Completed,
        
        /// <summary>
        /// Decision execution failed
        /// </summary>
        Failed,
        
        /// <summary>
        /// Decision was cancelled
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Outcome of a decision
    /// </summary>
    public enum DecisionOutcome
    {
        /// <summary>
        /// Decision completed successfully
        /// </summary>
        Success,
        
        /// <summary>
        /// Decision completed with a warning
        /// </summary>
        Warning,
        
        /// <summary>
        /// Decision completed with an error
        /// </summary>
        Error,
        
        /// <summary>
        /// Decision was rejected by policy
        /// </summary>
        Rejected
    }

    /// <summary>
    /// Interface for decision execution functionality
    /// </summary>
    public interface IDecisionExecutor
    {
        /// <summary>
        /// Executes a decision based on the provided request
        /// </summary>
        Task<DecisionResult> ExecuteDecisionAsync(
            DecisionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the status of a previously executed decision
        /// </summary>
        Task<DecisionResult> GetDecisionStatusAsync(
            string requestId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a log of recent decisions
        /// </summary>
        Task<IEnumerable<DecisionLog>> GetDecisionLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int limit = 100,
            CancellationToken cancellationToken = default);
    }
}
