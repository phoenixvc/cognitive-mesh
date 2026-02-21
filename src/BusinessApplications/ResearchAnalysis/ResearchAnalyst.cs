using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CognitiveMesh.Shared.Interfaces;

namespace CognitiveMesh.BusinessApplications.ResearchAnalysis
{
    /// <summary>
    /// Handles research analysis tasks including data collection, processing, and insights generation.
    /// Uses IResearchDataPort for persistence, IResearchAnalysisPort for LLM-based analysis,
    /// and IVectorDatabaseAdapter for semantic search across research results.
    /// </summary>
    public class ResearchAnalyst : IResearchAnalyst
    {
        private readonly ILogger<ResearchAnalyst> _logger;
        private readonly IKnowledgeGraphManager _knowledgeGraphManager;
        private readonly ILLMClient _llmClient;
        private readonly IVectorDatabaseAdapter _vectorDatabase;
        private readonly IResearchDataPort _researchDataPort;
        private readonly IResearchAnalysisPort _analysisPort;

        private const string ResearchVectorCollection = "research-results";

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchAnalyst"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <param name="knowledgeGraphManager">The knowledge graph manager for relationship-based queries.</param>
        /// <param name="llmClient">The LLM client for embedding generation used in vector search.</param>
        /// <param name="vectorDatabase">The vector database for semantic similarity search.</param>
        /// <param name="researchDataPort">The port for research data persistence operations.</param>
        /// <param name="analysisPort">The port for LLM-based research analysis operations.</param>
        public ResearchAnalyst(
            ILogger<ResearchAnalyst> logger,
            IKnowledgeGraphManager knowledgeGraphManager,
            ILLMClient llmClient,
            IVectorDatabaseAdapter vectorDatabase,
            IResearchDataPort researchDataPort,
            IResearchAnalysisPort analysisPort)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _knowledgeGraphManager = knowledgeGraphManager ?? throw new ArgumentNullException(nameof(knowledgeGraphManager));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
            _vectorDatabase = vectorDatabase ?? throw new ArgumentNullException(nameof(vectorDatabase));
            _researchDataPort = researchDataPort ?? throw new ArgumentNullException(nameof(researchDataPort));
            _analysisPort = analysisPort ?? throw new ArgumentNullException(nameof(analysisPort));
        }

        /// <inheritdoc/>
        public async Task<ResearchResult> AnalyzeResearchTopicAsync(
            string topic,
            ResearchParameters? parameters = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Research topic cannot be empty", nameof(topic));

            parameters ??= new ResearchParameters();

            try
            {
                _logger.LogInformation("Starting research analysis for topic: {Topic}", topic);

                var researchId = $"research-{Guid.NewGuid()}";
                var createdAt = DateTime.UtcNow;

                // Use the analysis port to perform LLM-based topic analysis
                var (summary, keyFindings) = await _analysisPort.AnalyzeTopicAsync(
                    topic, parameters, cancellationToken).ConfigureAwait(false);

                var result = new ResearchResult
                {
                    Id = researchId,
                    Topic = topic,
                    Status = ResearchStatus.Completed,
                    Summary = summary,
                    KeyFindings = keyFindings,
                    CreatedAt = createdAt,
                    CompletedAt = DateTime.UtcNow,
                    Parameters = parameters,
                    Metadata = new Dictionary<string, object>
                    {
                        ["findingsCount"] = keyFindings.Count,
                        ["analysisMethod"] = "llm-structured"
                    }
                };

                // Persist the result
                await _researchDataPort.SaveResearchResultAsync(result, cancellationToken).ConfigureAwait(false);

                // Index in vector database for semantic search
                var embedding = await _llmClient.GetEmbeddingsAsync(
                    $"{topic}: {summary}", cancellationToken).ConfigureAwait(false);

                if (embedding.Length > 0)
                {
                    var items = new[] { new { Id = researchId, Vector = embedding } };
                    await _vectorDatabase.UpsertVectorsAsync(
                        ResearchVectorCollection,
                        items,
                        item => item.Vector,
                        item => item.Id,
                        cancellationToken).ConfigureAwait(false);
                }

                // Record in knowledge graph
                await _knowledgeGraphManager.AddNodeAsync(
                    researchId,
                    new { topic, status = "Completed", summary },
                    label: "Research",
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Research analysis completed for topic: {Topic}, findings: {FindingsCount}",
                    topic, keyFindings.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing research topic: {Topic}", topic);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ResearchResult> GetResearchResultAsync(
            string researchId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(researchId))
                throw new ArgumentException("Research ID cannot be empty", nameof(researchId));

            try
            {
                _logger.LogInformation("Retrieving research result: {ResearchId}", researchId);

                var result = await _researchDataPort.GetResearchResultByIdAsync(researchId, cancellationToken)
                    .ConfigureAwait(false);

                if (result is null)
                {
                    _logger.LogWarning("Research result not found: {ResearchId}", researchId);
                    throw new KeyNotFoundException($"Research result not found for ID: {researchId}");
                }

                _logger.LogInformation("Successfully retrieved research result: {ResearchId}", researchId);
                return result;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving research result: {ResearchId}", researchId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ResearchResult>> SearchResearchResultsAsync(
            string query,
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query cannot be empty", nameof(query));

            try
            {
                _logger.LogInformation("Searching research results for query: {Query}, limit: {Limit}", query, limit);

                // First attempt semantic search via vector database
                var queryEmbedding = await _llmClient.GetEmbeddingsAsync(query, cancellationToken)
                    .ConfigureAwait(false);

                var results = new List<ResearchResult>();

                if (queryEmbedding.Length > 0)
                {
                    var similarVectors = await _vectorDatabase.SearchVectorsAsync(
                        ResearchVectorCollection, queryEmbedding, limit, cancellationToken)
                        .ConfigureAwait(false);

                    foreach (var (id, score) in similarVectors)
                    {
                        var research = await _researchDataPort.GetResearchResultByIdAsync(id, cancellationToken)
                            .ConfigureAwait(false);
                        if (research is not null)
                        {
                            research.Metadata["similarityScore"] = score;
                            results.Add(research);
                        }
                    }
                }

                // Fall back to text-based search if semantic search yields no results
                if (results.Count == 0)
                {
                    _logger.LogDebug("No semantic search results; falling back to text search for query: {Query}", query);
                    var textResults = await _researchDataPort.SearchAsync(query, limit, cancellationToken)
                        .ConfigureAwait(false);
                    results.AddRange(textResults);
                }

                _logger.LogInformation("Found {ResultCount} research results for query: {Query}",
                    results.Count, query);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching research results for query: {Query}", query);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ResearchResult> UpdateResearchAsync(
            string researchId,
            ResearchUpdate update,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(researchId))
                throw new ArgumentException("Research ID cannot be empty", nameof(researchId));
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            try
            {
                _logger.LogInformation("Updating research: {ResearchId}", researchId);

                // Retrieve the existing research
                var existing = await _researchDataPort.GetResearchResultByIdAsync(researchId, cancellationToken)
                    .ConfigureAwait(false);

                if (existing is null)
                {
                    _logger.LogWarning("Research result not found for update: {ResearchId}", researchId);
                    throw new KeyNotFoundException($"Research result not found for ID: {researchId}");
                }

                // Apply the update
                if (update.Status.HasValue)
                {
                    existing.Status = update.Status.Value;
                    if (update.Status.Value == ResearchStatus.Completed)
                    {
                        existing.CompletedAt = DateTime.UtcNow;
                    }
                }

                if (!string.IsNullOrEmpty(update.Summary))
                {
                    existing.Summary = update.Summary;
                }

                if (update.KeyFindings is not null && update.KeyFindings.Count > 0)
                {
                    existing.KeyFindings = update.KeyFindings;
                }

                foreach (var kvp in update.Metadata)
                {
                    existing.Metadata[kvp.Key] = kvp.Value;
                }

                existing.UpdatedAt = DateTime.UtcNow;

                // Persist the updated result
                await _researchDataPort.UpdateResearchResultAsync(existing, cancellationToken)
                    .ConfigureAwait(false);

                // Update the knowledge graph node
                await _knowledgeGraphManager.UpdateNodeAsync(
                    researchId,
                    new { topic = existing.Topic, status = existing.Status.ToString(), summary = existing.Summary },
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                // Re-index in vector database if summary changed
                if (!string.IsNullOrEmpty(update.Summary))
                {
                    var embedding = await _llmClient.GetEmbeddingsAsync(
                        $"{existing.Topic}: {existing.Summary}", cancellationToken).ConfigureAwait(false);

                    if (embedding.Length > 0)
                    {
                        var items = new[] { new { Id = researchId, Vector = embedding } };
                        await _vectorDatabase.UpsertVectorsAsync(
                            ResearchVectorCollection,
                            items,
                            item => item.Vector,
                            item => item.Id,
                            cancellationToken).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("Successfully updated research: {ResearchId}", researchId);
                return existing;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating research: {ResearchId}", researchId);
                throw;
            }
        }
    }

    /// <summary>
    /// Represents the result of a research analysis
    /// </summary>
    public class ResearchResult
    {
        /// <summary>
        /// Unique identifier for the research result
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The research topic
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the research
        /// </summary>
        public ResearchStatus Status { get; set; }

        /// <summary>
        /// Summary of the research findings
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Key findings from the research
        /// </summary>
        public List<string> KeyFindings { get; set; } = new();

        /// <summary>
        /// When the research was started
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the research was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// When the research was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Parameters used for this research
        /// </summary>
        public ResearchParameters? Parameters { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Parameters for conducting research
    /// </summary>
    public class ResearchParameters
    {
        /// <summary>
        /// Maximum number of sources to consider
        /// </summary>
        public int MaxSources { get; set; } = 10;

        /// <summary>
        /// Whether to include external sources
        /// </summary>
        public bool IncludeExternalSources { get; set; } = true;

        /// <summary>
        /// Minimum confidence threshold for including results (0-1)
        /// </summary>
        public float MinConfidence { get; set; } = 0.7f;

        /// <summary>
        /// Timeout in seconds for the research
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Additional parameters
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; } = new();
    }

    /// <summary>
    /// Status of a research task
    /// </summary>
    public enum ResearchStatus
    {
        /// <summary>
        /// Research has been queued but not started
        /// </summary>
        Pending,

        /// <summary>
        /// Research is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Research has been completed
        /// </summary>
        Completed,

        /// <summary>
        /// Research failed
        /// </summary>
        Failed,

        /// <summary>
        /// Research was cancelled
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Represents an update to a research task
    /// </summary>
    public class ResearchUpdate
    {
        /// <summary>
        /// New status for the research
        /// </summary>
        public ResearchStatus? Status { get; set; }

        /// <summary>
        /// Updated summary
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Updated key findings
        /// </summary>
        public List<string>? KeyFindings { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Interface for research analysis functionality
    /// </summary>
    public interface IResearchAnalyst
    {
        /// <summary>
        /// Analyzes a research topic and returns the results
        /// </summary>
        Task<ResearchResult> AnalyzeResearchTopicAsync(
            string topic,
            ResearchParameters? parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the result of a specific research task
        /// </summary>
        Task<ResearchResult> GetResearchResultAsync(
            string researchId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for research results matching a query
        /// </summary>
        Task<IEnumerable<ResearchResult>> SearchResearchResultsAsync(
            string query,
            int limit = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a research task
        /// </summary>
        Task<ResearchResult> UpdateResearchAsync(
            string researchId,
            ResearchUpdate update,
            CancellationToken cancellationToken = default);
    }
}
