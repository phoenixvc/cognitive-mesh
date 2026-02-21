using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.DecisionSupport
{
    /// <summary>
    /// Port interface for decision analysis operations.
    /// Adapters implement this to integrate with reasoning engines (ConclAIve, LLM, etc.).
    /// </summary>
    public interface IDecisionAnalysisPort
    {
        /// <summary>
        /// Scores a set of decision options against the given criteria using structured reasoning.
        /// </summary>
        /// <param name="decisionContext">The context describing the decision to be made.</param>
        /// <param name="options">The available decision options, each represented as a dictionary of properties.</param>
        /// <param name="criteria">The evaluation criteria with optional weights.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A dictionary containing scored options, the best option index, and recommendations.</returns>
        Task<Dictionary<string, object>> ScoreOptionsAsync(
            string decisionContext,
            IEnumerable<Dictionary<string, object>> options,
            Dictionary<string, object>? criteria,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Evaluates risk factors for a given scenario using structured analysis.
        /// </summary>
        /// <param name="scenario">The scenario description to evaluate.</param>
        /// <param name="parameters">Parameters describing the risk context (e.g., impact, likelihood inputs).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A dictionary containing risk level, risk score, identified risks, and mitigation strategies.</returns>
        Task<Dictionary<string, object>> AssessRiskAsync(
            string scenario,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates actionable recommendations based on context and supporting data.
        /// </summary>
        /// <param name="context">The decision or action context.</param>
        /// <param name="data">Supporting data for recommendation generation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A dictionary containing ranked recommendations with confidence scores and supporting evidence.</returns>
        Task<Dictionary<string, object>> GenerateRecommendationsAsync(
            string context,
            Dictionary<string, object> data,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Simulates potential outcomes for a scenario using scenario analysis.
        /// </summary>
        /// <param name="scenario">The scenario to simulate.</param>
        /// <param name="parameters">Parameters controlling the simulation (e.g., time horizon, variables).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A dictionary containing the most likely outcome, probability, and alternative scenarios.</returns>
        Task<Dictionary<string, object>> SimulateAsync(
            string scenario,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default);
    }
}
