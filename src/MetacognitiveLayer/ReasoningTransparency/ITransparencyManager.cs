using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.MetacognitiveLayer.ReasoningTransparency
{
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
