using System.Text.Json;
using MetacognitiveLayer.ReasoningTransparency.Strategies;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.ReasoningTransparency
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
        /// <param name="knowledgeGraphManager">The knowledge graph manager instance.</param>
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
                
                // 1. Retrieve the Trace Node
                string traceNodeId = $"trace:{traceId}";
                var traceNode = await _knowledgeGraphManager.GetNodeAsync<ReasoningTraceNode>(traceNodeId, cancellationToken);

                if (traceNode == null)
                {
                    _logger.LogWarning("Reasoning trace not found: {TraceId}", traceId);
                    return null;
                }

                // 2. Retrieve the Step Nodes associated with this trace
                var searchProps = new Dictionary<string, object>
                {
                    { nameof(ReasoningStepNode.TraceId), traceId }
                };
                
                var stepNodes = await _knowledgeGraphManager.FindNodesAsync<ReasoningStepNode>(searchProps, cancellationToken);

                // 3. Convert Storage Models back to Domain Models
                var steps = new List<ReasoningStep>();
                if (stepNodes != null)
                {
                    foreach (var node in stepNodes)
                    {
                        steps.Add(MapToReasoningStep(node));
                    }
                }

                steps.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

                return new ReasoningTrace
                {
                    Id = traceNode.Id,
                    Name = traceNode.Name,
                    Description = traceNode.Description,
                    CreatedAt = traceNode.CreatedAt,
                    UpdatedAt = traceNode.UpdatedAt,
                    Steps = steps
                };
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
                
                var searchProps = new Dictionary<string, object>
                {
                    { nameof(DecisionRationaleNode.DecisionId), decisionId }
                };

                var rationaleNodes = await _knowledgeGraphManager.FindNodesAsync<DecisionRationaleNode>(searchProps, cancellationToken);

                var rationales = new List<DecisionRationale>();
                if (rationaleNodes != null)
                {
                    foreach (var node in rationaleNodes.Take(limit))
                    {
                        rationales.Add(MapToDecisionRationale(node));
                    }
                }

                return rationales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving decision rationales for decision: {DecisionId}", decisionId);
                throw;
            }
        }

        private DecisionRationale MapToDecisionRationale(DecisionRationaleNode node)
        {
            return new DecisionRationale
            {
                Id = node.Id,
                DecisionId = node.DecisionId,
                Description = node.Description,
                Confidence = node.Confidence,
                CreatedAt = node.CreatedAt,
                Factors = DeserializeFactors(node.FactorsJson)
            };
        }

        private Dictionary<string, float> DeserializeFactors(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<string, float>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, float>>(json) ?? new Dictionary<string, float>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, float>();
            }
        }

        /// <inheritdoc/>
        public async Task LogReasoningStepAsync(
            ReasoningStep step, 
            CancellationToken cancellationToken = default)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            
            try
            {
                _logger.LogInformation("Logging reasoning step: {StepId} for trace: {TraceId}", 
                    step.Id, step.TraceId);

                // 1. Ensure the Trace Node exists (Create if not)
                string traceNodeId = $"trace:{step.TraceId}";
                var traceNode = await _knowledgeGraphManager.GetNodeAsync<ReasoningTraceNode>(traceNodeId, cancellationToken);

                if (traceNode == null)
                {
                    _logger.LogInformation("Trace {TraceId} does not exist, creating placeholder.", step.TraceId);
                    var newTrace = new ReasoningTraceNode
                    {
                        Id = step.TraceId,
                        Name = $"Trace {step.TraceId}", // Default name
                        Description = "Auto-generated trace from step logging",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _knowledgeGraphManager.AddNodeAsync(traceNodeId, newTrace, "ReasoningTrace", cancellationToken);
                }
                else
                {
                    traceNode.UpdatedAt = DateTime.UtcNow;
                    await _knowledgeGraphManager.UpdateNodeAsync(traceNodeId, traceNode, cancellationToken);
                }

                // 2. Create the Step Node
                string stepNodeId = $"step:{step.Id}";
                var stepNode = MapToReasoningStepNode(step);
                
                // Store the reasoning step as a node in the knowledge graph
                await _knowledgeGraphManager.AddNodeAsync(
                    stepNodeId,
                    stepNode,
                    "ReasoningStep",
                    cancellationToken);

                // Link the step to its trace
                // We assume the trace node exists or can be linked to.
                // "BELONGS_TO" relationship from ReasoningStep -> ReasoningTrace
                await _knowledgeGraphManager.AddRelationshipAsync(
                    stepNodeId,
                    traceNodeId,
                    "BELONGS_TO",
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

        private ReasoningStepNode MapToReasoningStepNode(ReasoningStep step)
        {
            return new ReasoningStepNode
            {
                Id = step.Id,
                TraceId = step.TraceId,
                Name = step.Name,
                Description = step.Description,
                Timestamp = step.Timestamp,
                Confidence = step.Confidence,
                InputsJson = JsonSerializer.Serialize(step.Inputs),
                OutputsJson = JsonSerializer.Serialize(step.Outputs),
                MetadataJson = JsonSerializer.Serialize(step.Metadata)
            };
        }

        private ReasoningStep MapToReasoningStep(ReasoningStepNode node)
        {
            return new ReasoningStep
            {
                Id = node.Id,
                TraceId = node.TraceId,
                Name = node.Name,
                Description = node.Description,
                Timestamp = node.Timestamp,
                Confidence = node.Confidence,
                Inputs = DeserializeAndUnwrap(node.InputsJson),
                Outputs = DeserializeAndUnwrap(node.OutputsJson),
                Metadata = DeserializeAndUnwrap(node.MetadataJson)
            };
        }

        private Dictionary<string, object> DeserializeAndUnwrap(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<string, object>();

            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (dict == null)
                return new Dictionary<string, object>();

            // Unwrap JsonElements to primitive types
            var result = new Dictionary<string, object>();
            foreach (var kvp in dict)
            {
                if (kvp.Value is JsonElement element)
                {
                    var val = UnwrapJsonElement(element);
                    if (val != null)
                    {
                        result[kvp.Key] = val;
                    }
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        private object? UnwrapJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int i)) return i;
                    if (element.TryGetInt64(out long l)) return l;
                    if (element.TryGetDouble(out double d)) return d;
                    return 0; // Fallback
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Object:
                    // Recursively unwrap objects to Dictionary<string, object>
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        var val = UnwrapJsonElement(prop.Value);
                        if (val != null)
                        {
                            dict[prop.Name] = val;
                        }
                    }
                    return dict;
                case JsonValueKind.Array:
                    // Recursively unwrap arrays to List<object>
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        var val = UnwrapJsonElement(item);
                        if (val != null)
                        {
                            list.Add(val);
                        }
                    }
                    return list;
                default:
                    return element.ToString();
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
}
