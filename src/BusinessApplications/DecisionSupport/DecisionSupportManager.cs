using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.DecisionSupport
{
    /// <summary>
    /// Provides decision support capabilities for the cognitive mesh.
    /// Delegates analysis, risk evaluation, recommendation generation, and outcome simulation
    /// to an <see cref="IDecisionAnalysisPort"/> adapter that integrates with reasoning engines.
    /// </summary>
    public class DecisionSupportManager : IDecisionSupportManager, IDisposable
    {
        private readonly ILogger<DecisionSupportManager> _logger;
        private readonly IDecisionAnalysisPort _analysisPort;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionSupportManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <param name="analysisPort">The port for decision analysis operations backed by reasoning engines.</param>
        public DecisionSupportManager(
            ILogger<DecisionSupportManager> logger,
            IDecisionAnalysisPort analysisPort)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _analysisPort = analysisPort ?? throw new ArgumentNullException(nameof(analysisPort));
            _logger.LogInformation("DecisionSupportManager initialized");
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> AnalyzeDecisionOptionsAsync(
            string decisionContext,
            IEnumerable<Dictionary<string, object>> options,
            Dictionary<string, object>? criteria = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(decisionContext))
                throw new ArgumentException("Decision context cannot be empty", nameof(decisionContext));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            try
            {
                _logger.LogInformation("Analyzing decision options for context: {Context}", decisionContext);

                var optionList = options.ToList();
                if (optionList.Count == 0)
                {
                    _logger.LogWarning("No options provided for decision analysis: {Context}", decisionContext);
                    return new Dictionary<string, object>
                    {
                        ["bestOption"] = -1,
                        ["scores"] = new Dictionary<string, double>(),
                        ["recommendations"] = Array.Empty<string>()
                    };
                }

                var result = await _analysisPort.ScoreOptionsAsync(
                    decisionContext, optionList, criteria, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Decision analysis completed for context: {Context}, best option index: {BestOption}",
                    decisionContext, result.GetValueOrDefault("bestOption", -1));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing decision options for context: {Context}", decisionContext);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> EvaluateRiskAsync(
            string scenario,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(scenario))
                throw new ArgumentException("Scenario cannot be empty", nameof(scenario));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            try
            {
                _logger.LogInformation("Evaluating risk for scenario: {Scenario}", scenario);

                var result = await _analysisPort.AssessRiskAsync(scenario, parameters, cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogInformation(
                    "Risk evaluation completed for scenario: {Scenario}, risk level: {RiskLevel}",
                    scenario, result.GetValueOrDefault("riskLevel", "unknown"));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating risk for scenario: {Scenario}", scenario);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> GenerateRecommendationsAsync(
            string context,
            Dictionary<string, object> data,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(context))
                throw new ArgumentException("Context cannot be empty", nameof(context));
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            try
            {
                _logger.LogInformation("Generating recommendations for context: {Context}", context);

                var result = await _analysisPort.GenerateRecommendationsAsync(context, data, cancellationToken)
                    .ConfigureAwait(false);

                var recommendationCount = result.TryGetValue("recommendations", out var recs) && recs is object[] arr
                    ? arr.Length
                    : 0;

                _logger.LogInformation(
                    "Recommendation generation completed for context: {Context}, count: {Count}",
                    context, recommendationCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations for context: {Context}", context);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> SimulateOutcomesAsync(
            string scenario,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(scenario))
                throw new ArgumentException("Scenario cannot be empty", nameof(scenario));
            if (parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            try
            {
                _logger.LogInformation("Simulating outcomes for scenario: {Scenario}", scenario);

                var result = await _analysisPort.SimulateAsync(scenario, parameters, cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogInformation(
                    "Outcome simulation completed for scenario: {Scenario}, probability: {Probability}",
                    scenario, result.GetValueOrDefault("probability", 0.0));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating outcomes for scenario: {Scenario}", scenario);
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DecisionSupportManager"/> and optionally releases managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources if the analysis port is disposable
                    if (_analysisPort is IDisposable disposablePort)
                    {
                        disposablePort.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for <see cref="DecisionSupportManager"/>.
        /// </summary>
        ~DecisionSupportManager()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Defines the contract for decision support in the cognitive mesh.
    /// </summary>
    public interface IDecisionSupportManager : IDisposable
    {
        /// <summary>
        /// Analyzes decision options based on the given context and criteria.
        /// </summary>
        /// <param name="decisionContext">The context of the decision to be made.</param>
        /// <param name="options">The available decision options.</param>
        /// <param name="criteria">The criteria to evaluate the options against.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the analysis results.</returns>
        Task<Dictionary<string, object>> AnalyzeDecisionOptionsAsync(
            string decisionContext,
            IEnumerable<Dictionary<string, object>> options,
            Dictionary<string, object>? criteria = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Evaluates the risk associated with a given scenario.
        /// </summary>
        /// <param name="scenario">The scenario to evaluate.</param>
        /// <param name="parameters">The parameters for the evaluation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the risk assessment results.</returns>
        Task<Dictionary<string, object>> EvaluateRiskAsync(
            string scenario,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates recommendations based on the given context and data.
        /// </summary>
        /// <param name="context">The context for generating recommendations.</param>
        /// <param name="data">The data to base the recommendations on.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the generated recommendations.</returns>
        Task<Dictionary<string, object>> GenerateRecommendationsAsync(
            string context,
            Dictionary<string, object> data,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Simulates the outcomes of a given scenario.
        /// </summary>
        /// <param name="scenario">The scenario to simulate.</param>
        /// <param name="parameters">The parameters for the simulation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the simulation results.</returns>
        Task<Dictionary<string, object>> SimulateOutcomesAsync(
            string scenario,
            Dictionary<string, object> parameters,
            CancellationToken cancellationToken = default);
    }
}
