using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.ResearchAnalysis
{
    /// <summary>
    /// Port interface for research data persistence and retrieval operations.
    /// Adapters implement this to integrate with specific data stores.
    /// </summary>
    public interface IResearchDataPort
    {
        /// <summary>
        /// Persists a research result to the data store.
        /// </summary>
        /// <param name="result">The research result to save.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveResearchResultAsync(ResearchResult result, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a research result by its unique identifier.
        /// </summary>
        /// <param name="researchId">The unique identifier of the research result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The research result, or null if not found.</returns>
        Task<ResearchResult?> GetResearchResultByIdAsync(string researchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for research results matching a text query.
        /// </summary>
        /// <param name="query">The search query text.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A collection of matching research results.</returns>
        Task<IEnumerable<ResearchResult>> SearchAsync(string query, int limit, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing research result in the data store.
        /// </summary>
        /// <param name="result">The updated research result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        Task UpdateResearchResultAsync(ResearchResult result, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Port interface for research analysis operations using LLM-based reasoning.
    /// Adapters implement this to integrate with reasoning engines.
    /// </summary>
    public interface IResearchAnalysisPort
    {
        /// <summary>
        /// Analyzes a research topic and produces key findings and a summary.
        /// </summary>
        /// <param name="topic">The research topic to analyze.</param>
        /// <param name="parameters">Parameters controlling the analysis (max sources, confidence threshold, etc.).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A tuple of the generated summary and key findings.</returns>
        Task<(string Summary, List<string> KeyFindings)> AnalyzeTopicAsync(
            string topic,
            ResearchParameters parameters,
            CancellationToken cancellationToken = default);
    }
}
