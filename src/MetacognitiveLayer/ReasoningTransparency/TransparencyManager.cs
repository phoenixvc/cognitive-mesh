using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CognitiveMesh.Shared.Interfaces;
using CognitiveMesh.MetacognitiveLayer.ReasoningTransparency.Strategies;

namespace CognitiveMesh.MetacognitiveLayer.ReasoningTransparency
{
    /// <summary>
    /// Manages transparency in the reasoning process, providing insights into decision-making
    /// </summary>
    public class TransparencyManager : ITransparencyManager
    {
        private readonly ILogger<TransparencyManager> _logger;
        private readonly IKnowledgeGraphManager _knowledgeGraphManager;
        private readonly Dictionary<string, IReportFormatStrategy> _reportStrategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransparencyManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="knowledgeGraphManager">The knowledge graph manager for persisting and retrieving traces.</param>
        public TransparencyManager(
            ILogger<TransparencyManager> logger,
            IKnowledgeGraphManager knowledgeGraphManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _knowledgeGraphManager = knowledgeGraphManager ?? throw new ArgumentNullException(nameof(knowledgeGraphManager));

            _reportStrategies = new Dictionary<string, IReportFormatStrategy>(StringComparer.OrdinalIgnoreCase)
            {
                ["json"] = new JsonReportFormatStrategy(),
                ["markdown"] = new MarkdownReportFormatStrategy()
            };
        }

        /// <inheritdoc/>
        public async Task<ReasoningTrace?> GetReasoningTraceAsync(
            string traceId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving reasoning trace: {TraceId}", traceId);
                
                // Retrieve the trace node
                var traceNode = await _knowledgeGraphManager.GetNodeAsync<ReasoningTrace>(traceId, cancellationToken);
                if (traceNode == null)
                {
                    _logger.LogWarning("Reasoning trace not found: {TraceId}", traceId);
                    return null;
                }

                // Retrieve associated steps
                var steps = await _knowledgeGraphManager.FindNodesAsync<ReasoningStep>(
                    new Dictionary<string, object> { ["TraceId"] = traceId },
                    cancellationToken);

                traceNode.Steps = steps?.OrderBy(s => s.Timestamp).ToList() ?? new List<ReasoningStep>();

                return traceNode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reasoning trace: {TraceId}", traceId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DecisionRationale>> GetDecisionRationalesAsync(
            string decisionId, 
            int limit = 10, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving rationales for decision: {DecisionId}", decisionId);
                
                // Retrieve rationales linked to the decision
                var rationales = await _knowledgeGraphManager.FindNodesAsync<DecisionRationale>(
                    new Dictionary<string, object> { ["DecisionId"] = decisionId },
                    cancellationToken);

                return rationales?.OrderByDescending(r => r.Confidence).Take(limit) ?? Enumerable.Empty<DecisionRationale>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving decision rationales for decision: {DecisionId}", decisionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task LogReasoningStepAsync(
            ReasoningStep step, 
            CancellationToken cancellationToken = default)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (string.IsNullOrEmpty(step.Id)) throw new ArgumentException("Step Id cannot be null or empty", nameof(step));
            if (string.IsNullOrEmpty(step.TraceId)) throw new ArgumentException("Trace Id cannot be null or empty", nameof(step));
            
            try
            {
                _logger.LogInformation("Logging reasoning step: {StepId} for trace: {TraceId}", 
                    step.Id, step.TraceId);
                
                // 1. Ensure the Trace node exists. If not, create it.
                var trace = await _knowledgeGraphManager.GetNodeAsync<ReasoningTrace>(step.TraceId, cancellationToken);
                if (trace == null)
                {
                    trace = new ReasoningTrace
                    {
                        Id = step.TraceId,
                        Name = $"Trace {step.TraceId}", // Default name
                        Description = "Automatically created trace",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _knowledgeGraphManager.AddNodeAsync(trace.Id, trace, "ReasoningTrace", cancellationToken);
                }
                else
                {
                    // Update UpdatedAt
                    trace.UpdatedAt = DateTime.UtcNow;
                    // trace.Id is string?, but we know it's not null here because we fetched it using step.TraceId
                    await _knowledgeGraphManager.UpdateNodeAsync(trace.Id!, trace, cancellationToken);
                }

                // 2. Create the Step node
                await _knowledgeGraphManager.AddNodeAsync(step.Id, step, "ReasoningStep", cancellationToken);

                // 3. Create relationship
                await _knowledgeGraphManager.AddRelationshipAsync(
                    step.TraceId,
                    step.Id,
                    "HAS_STEP",
                    null,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging reasoning step: {StepId}", step.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TransparencyReport>> GenerateTransparencyReportAsync(
            string traceId, 
            string format = "json", 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Generating transparency report for trace: {TraceId}", traceId);
                
                var trace = await GetReasoningTraceAsync(traceId, cancellationToken);
                if (trace == null)
                {
                     throw new KeyNotFoundException($"Trace with ID {traceId} not found.");
                }

                if (!_reportStrategies.TryGetValue(format, out var strategy))
                {
                    throw new NotSupportedException($"Report format '{format}' is not supported.");
                }

                // Calculate aggregations
                var aggregations = CalculateAggregations(trace);

                var content = strategy.GenerateReport(trace, aggregations);
                
                return new[]
                {
                    new TransparencyReport
                    {
                        Id = $"report-{Guid.NewGuid()}",
                        TraceId = traceId,
                        Format = format,
                        Content = content,
                        GeneratedAt = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating transparency report for trace: {TraceId}", traceId);
                throw;
            }
        }

        private Dictionary<string, object> CalculateAggregations(ReasoningTrace trace)
        {
            var stats = new Dictionary<string, object>();

            var steps = trace.Steps ?? new List<ReasoningStep>();
            stats["TotalSteps"] = steps.Count;

            if (steps.Any())
            {
                stats["AverageConfidence"] = steps.Average(s => s.Confidence);
                stats["MinConfidence"] = steps.Min(s => s.Confidence);
                stats["MaxConfidence"] = steps.Max(s => s.Confidence);

                var sortedSteps = steps.OrderBy(s => s.Timestamp).ToList();
                stats["StartTime"] = sortedSteps.First().Timestamp;
                stats["EndTime"] = sortedSteps.Last().Timestamp;
                stats["Duration"] = (sortedSteps.Last().Timestamp - sortedSteps.First().Timestamp).ToString();

                // Aggregate used models if available in metadata
                var models = steps
                    .Where(s => s.Metadata != null && s.Metadata.ContainsKey("model"))
                    .Select(s => s.Metadata["model"]?.ToString())
                    .Where(m => !string.IsNullOrEmpty(m))
                    .Distinct()
                    .ToList();

                if (models.Any())
                {
                    stats["ModelsUsed"] = models;
                }
            }
            else
            {
                stats["AverageConfidence"] = 0;
                stats["Duration"] = TimeSpan.Zero.ToString();
            }

            return stats;
        }
    }

    /// <summary>
    /// Represents a step in a reasoning process
    /// </summary>
    public class ReasoningStep
    {
        /// <summary>
        /// Unique identifier for the step
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Identifier of the trace this step belongs to
        /// </summary>
        public string? TraceId { get; set; }
        
        /// <summary>
        /// Name of the step
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Description of the step
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// When the step occurred
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Inputs to the step
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; } = new();
        
        /// <summary>
        /// Outputs from the step
        /// </summary>
        public Dictionary<string, object> Outputs { get; set; } = new();
        
        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public float Confidence { get; set; }
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a trace of reasoning steps
    /// </summary>
    public class ReasoningTrace
    {
        /// <summary>
        /// Unique identifier for the trace
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Name of the trace
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Description of the trace
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// The steps in this reasoning trace
        /// </summary>
        public List<ReasoningStep> Steps { get; set; } = new();
        
        /// <summary>
        /// When the trace was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the trace was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the rationale behind a decision
    /// </summary>
    public class DecisionRationale
    {
        /// <summary>
        /// Unique identifier for the rationale
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Identifier of the decision this rationale is for
        /// </summary>
        public string? DecisionId { get; set; }
        
        /// <summary>
        /// Description of the rationale
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public float Confidence { get; set; }
        
        /// <summary>
        /// Factors that contributed to the decision
        /// </summary>
        public Dictionary<string, float> Factors { get; set; } = new();
        
        /// <summary>
        /// When the rationale was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Represents a transparency report
    /// </summary>
    public class TransparencyReport
    {
        /// <summary>
        /// Unique identifier for the report
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// Identifier of the trace this report is for
        /// </summary>
        public string? TraceId { get; set; }
        
        /// <summary>
        /// Format of the report (e.g., json, html, markdown)
        /// </summary>
        public string? Format { get; set; }
        
        /// <summary>
        /// The report content
        /// </summary>
        public string? Content { get; set; }
        
        /// <summary>
        /// When the report was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Interface for managing reasoning transparency
    /// </summary>
    public interface ITransparencyManager
    {
        /// <summary>
        /// Gets a trace of reasoning steps for a given trace ID
        /// </summary>
        Task<ReasoningTrace?> GetReasoningTraceAsync(
            string traceId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the rationales behind a specific decision
        /// </summary>
        Task<IEnumerable<DecisionRationale>> GetDecisionRationalesAsync(
            string decisionId, 
            int limit = 10, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs a reasoning step
        /// </summary>
        Task LogReasoningStepAsync(
            ReasoningStep step, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a transparency report for a given trace
        /// </summary>
        Task<IEnumerable<TransparencyReport>> GenerateTransparencyReportAsync(
            string traceId, 
            string format = "json", 
            CancellationToken cancellationToken = default);
    }
}
