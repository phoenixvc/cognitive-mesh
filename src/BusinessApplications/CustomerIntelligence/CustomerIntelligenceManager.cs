using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CognitiveMesh.Shared.Interfaces;

namespace CognitiveMesh.BusinessApplications.CustomerIntelligence
{
    /// <summary>
    /// Manages customer intelligence operations including analysis, segmentation, and insights.
    /// Uses ICustomerDataPort for data retrieval, ILLMClient for AI-driven insight generation,
    /// and IVectorDatabaseAdapter for similarity-based behavioral analysis.
    /// </summary>
    public class CustomerIntelligenceManager : ICustomerIntelligenceManager
    {
        private readonly ILogger<CustomerIntelligenceManager> _logger;
        private readonly IKnowledgeGraphManager _knowledgeGraphManager;
        private readonly ILLMClient _llmClient;
        private readonly IVectorDatabaseAdapter _vectorDatabase;
        private readonly ICustomerDataPort _customerDataPort;

        private const string CustomerVectorCollection = "customer-behaviors";

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerIntelligenceManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <param name="knowledgeGraphManager">The knowledge graph manager for relationship queries.</param>
        /// <param name="llmClient">The LLM client for generating insights and predictions.</param>
        /// <param name="vectorDatabase">The vector database for behavioral similarity searches.</param>
        /// <param name="customerDataPort">The port for customer data retrieval operations.</param>
        public CustomerIntelligenceManager(
            ILogger<CustomerIntelligenceManager> logger,
            IKnowledgeGraphManager knowledgeGraphManager,
            ILLMClient llmClient,
            IVectorDatabaseAdapter vectorDatabase,
            ICustomerDataPort customerDataPort)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _knowledgeGraphManager = knowledgeGraphManager ?? throw new ArgumentNullException(nameof(knowledgeGraphManager));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
            _vectorDatabase = vectorDatabase ?? throw new ArgumentNullException(nameof(vectorDatabase));
            _customerDataPort = customerDataPort ?? throw new ArgumentNullException(nameof(customerDataPort));
        }

        /// <inheritdoc/>
        public async Task<CustomerProfile> GetCustomerProfileAsync(
            string customerId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

            try
            {
                _logger.LogInformation("Retrieving customer profile: {CustomerId}", customerId);

                var profile = await _customerDataPort.GetProfileAsync(customerId, cancellationToken).ConfigureAwait(false);

                if (profile is null)
                {
                    _logger.LogWarning("Customer profile not found: {CustomerId}", customerId);
                    throw new KeyNotFoundException($"Customer profile not found for ID: {customerId}");
                }

                // Enrich profile with knowledge graph relationships
                var relationships = await _knowledgeGraphManager.QueryAsync(
                    $"MATCH (c:Customer {{id: '{customerId}'}})-[r]->(s:Segment) RETURN s",
                    cancellationToken).ConfigureAwait(false);

                foreach (var relation in relationships)
                {
                    if (relation.TryGetValue("name", out var segmentName) && segmentName is string name)
                    {
                        if (!profile.Segments.Contains(name))
                        {
                            profile.Segments.Add(name);
                        }
                    }
                }

                profile.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Successfully retrieved customer profile: {CustomerId}", customerId);
                return profile;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer profile: {CustomerId}", customerId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CustomerSegment>> GetCustomerSegmentsAsync(
            CustomerSegmentQuery query,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            try
            {
                _logger.LogInformation("Retrieving customer segments with filter: {NameFilter}, limit: {Limit}",
                    query.NameContains ?? "(none)", query.Limit);

                var segments = await _customerDataPort.QuerySegmentsAsync(query, cancellationToken).ConfigureAwait(false);
                var segmentList = segments.ToList();

                _logger.LogInformation("Retrieved {SegmentCount} customer segments", segmentList.Count);
                return segmentList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer segments");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CustomerInsight>> GenerateCustomerInsightsAsync(
            string customerId,
            InsightType insightType = InsightType.All,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

            try
            {
                _logger.LogInformation("Generating insights for customer: {CustomerId}, types: {InsightType}",
                    customerId, insightType);

                var interactions = await _customerDataPort.GetInteractionHistoryAsync(customerId, cancellationToken)
                    .ConfigureAwait(false);
                var interactionList = interactions.ToList();

                if (interactionList.Count == 0)
                {
                    _logger.LogWarning("No interaction history found for customer: {CustomerId}", customerId);
                    return Enumerable.Empty<CustomerInsight>();
                }

                var insights = new List<CustomerInsight>();

                if (insightType.HasFlag(InsightType.PurchasePatterns))
                {
                    var purchaseInsight = await GeneratePurchaseInsightAsync(customerId, interactionList, cancellationToken)
                        .ConfigureAwait(false);
                    if (purchaseInsight is not null)
                    {
                        insights.Add(purchaseInsight);
                    }
                }

                if (insightType.HasFlag(InsightType.BehavioralPatterns))
                {
                    var behavioralInsight = await GenerateBehavioralInsightAsync(customerId, interactionList, cancellationToken)
                        .ConfigureAwait(false);
                    if (behavioralInsight is not null)
                    {
                        insights.Add(behavioralInsight);
                    }
                }

                _logger.LogInformation("Generated {InsightCount} insights for customer: {CustomerId}",
                    insights.Count, customerId);
                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating insights for customer: {CustomerId}", customerId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CustomerPrediction> PredictCustomerBehaviorAsync(
            string customerId,
            PredictionType predictionType,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

            try
            {
                _logger.LogInformation("Predicting behavior for customer: {CustomerId}, type: {PredictionType}",
                    customerId, predictionType);

                // Retrieve behavioral features for prediction
                var features = await _customerDataPort.GetBehavioralFeaturesAsync(customerId, cancellationToken)
                    .ConfigureAwait(false);

                // Build a feature vector from the behavioral features
                var featureVector = features.Values.Select(v => (float)v).ToArray();

                // Find similar customer behaviors using vector similarity
                var similarBehaviors = Enumerable.Empty<(string Id, float Score)>();
                if (featureVector.Length > 0)
                {
                    similarBehaviors = await _vectorDatabase.SearchVectorsAsync(
                        CustomerVectorCollection, featureVector, limit: 20, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }

                // Use LLM to interpret the behavioral data and produce a prediction
                var featureSummary = string.Join(", ", features.Select(kv => $"{kv.Key}={kv.Value:F2}"));
                var similarCount = similarBehaviors.Count();
                var avgSimilarity = similarBehaviors.Any() ? similarBehaviors.Average(s => s.Score) : 0f;

                var prompt = $@"Analyze the following customer behavioral features and predict the {predictionType} outcome.

Customer ID: {customerId}
Behavioral features: {featureSummary}
Similar customer profiles found: {similarCount} (average similarity: {avgSimilarity:F2})

Provide:
1. A predicted value between 0.0 and 1.0
2. A confidence score between 0.0 and 1.0
3. A brief explanation

Format your response as:
PredictedValue: [value]
Confidence: [value]
Explanation: [explanation]";

                var llmResponse = await _llmClient.GenerateCompletionAsync(
                    prompt, temperature: 0.3f, maxTokens: 300, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var (predictedValue, confidence, explanation) = ParsePredictionResponse(llmResponse, predictionType);

                var prediction = new CustomerPrediction
                {
                    CustomerId = customerId,
                    Type = predictionType,
                    PredictedValue = predictedValue,
                    Confidence = confidence,
                    GeneratedAt = DateTime.UtcNow,
                    Explanation = explanation,
                    Metadata = new Dictionary<string, object>
                    {
                        ["featureCount"] = features.Count,
                        ["similarProfileCount"] = similarCount,
                        ["averageSimilarity"] = avgSimilarity,
                        ["modelName"] = _llmClient.ModelName
                    }
                };

                _logger.LogInformation(
                    "Prediction completed for customer {CustomerId}: {PredictionType} = {PredictedValue}, confidence = {Confidence}",
                    customerId, predictionType, predictedValue, confidence);

                return prediction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting behavior for customer: {CustomerId}", customerId);
                throw;
            }
        }

        private async Task<CustomerInsight?> GeneratePurchaseInsightAsync(
            string customerId,
            List<Dictionary<string, object>> interactions,
            CancellationToken cancellationToken)
        {
            var interactionSummary = JsonSerializer.Serialize(interactions.Take(50));

            var prompt = $@"Analyze the following customer interaction data and identify purchase patterns.

Customer ID: {customerId}
Interaction count: {interactions.Count}
Recent interactions (sample): {interactionSummary}

Provide:
1. A concise title for the purchase pattern insight
2. A description of the pattern
3. A confidence score between 0.0 and 1.0

Format your response as:
Title: [title]
Description: [description]
Confidence: [score]";

            var response = await _llmClient.GenerateCompletionAsync(
                prompt, temperature: 0.4f, maxTokens: 400, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var (title, description, confidence) = ParseInsightResponse(response);

            return new CustomerInsight
            {
                Type = InsightType.PurchasePatterns,
                Title = title,
                Description = description,
                Confidence = confidence,
                GeneratedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["interactionCount"] = interactions.Count,
                    ["modelName"] = _llmClient.ModelName
                }
            };
        }

        private async Task<CustomerInsight?> GenerateBehavioralInsightAsync(
            string customerId,
            List<Dictionary<string, object>> interactions,
            CancellationToken cancellationToken)
        {
            var interactionSummary = JsonSerializer.Serialize(interactions.Take(50));

            var prompt = $@"Analyze the following customer interaction data and identify behavioral patterns.

Customer ID: {customerId}
Interaction count: {interactions.Count}
Recent interactions (sample): {interactionSummary}

Provide:
1. A concise title for the behavioral pattern insight
2. A description of the pattern
3. A confidence score between 0.0 and 1.0

Format your response as:
Title: [title]
Description: [description]
Confidence: [score]";

            var response = await _llmClient.GenerateCompletionAsync(
                prompt, temperature: 0.4f, maxTokens: 400, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var (title, description, confidence) = ParseInsightResponse(response);

            return new CustomerInsight
            {
                Type = InsightType.BehavioralPatterns,
                Title = title,
                Description = description,
                Confidence = confidence,
                GeneratedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["interactionCount"] = interactions.Count,
                    ["modelName"] = _llmClient.ModelName
                }
            };
        }

        private static (string Title, string Description, float Confidence) ParseInsightResponse(string response)
        {
            var title = "Customer Insight";
            var description = response;
            var confidence = 0.7f;

            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
                {
                    title = trimmed["Title:".Length..].Trim();
                }
                else if (trimmed.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
                {
                    description = trimmed["Description:".Length..].Trim();
                }
                else if (trimmed.StartsWith("Confidence:", StringComparison.OrdinalIgnoreCase))
                {
                    if (float.TryParse(trimmed["Confidence:".Length..].Trim(), out var conf))
                    {
                        confidence = Math.Clamp(conf, 0f, 1f);
                    }
                }
            }

            return (title, description, confidence);
        }

        private static (float PredictedValue, float Confidence, string Explanation) ParsePredictionResponse(
            string response, PredictionType predictionType)
        {
            var predictedValue = predictionType == PredictionType.Churn ? 0.5f : 0.5f;
            var confidence = 0.7f;
            var explanation = response;

            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("PredictedValue:", StringComparison.OrdinalIgnoreCase))
                {
                    if (float.TryParse(trimmed["PredictedValue:".Length..].Trim(), out var pv))
                    {
                        predictedValue = Math.Clamp(pv, 0f, 1f);
                    }
                }
                else if (trimmed.StartsWith("Confidence:", StringComparison.OrdinalIgnoreCase))
                {
                    if (float.TryParse(trimmed["Confidence:".Length..].Trim(), out var conf))
                    {
                        confidence = Math.Clamp(conf, 0f, 1f);
                    }
                }
                else if (trimmed.StartsWith("Explanation:", StringComparison.OrdinalIgnoreCase))
                {
                    explanation = trimmed["Explanation:".Length..].Trim();
                }
            }

            return (predictedValue, confidence, explanation);
        }
    }

    /// <summary>
    /// Represents a customer profile
    /// </summary>
    public class CustomerProfile
    {
        /// <summary>
        /// Unique identifier for the customer
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Customer's full name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Customer's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Segments the customer belongs to
        /// </summary>
        public List<string> Segments { get; set; } = new();

        /// <summary>
        /// Customer's lifetime value
        /// </summary>
        public decimal LifetimeValue { get; set; }

        /// <summary>
        /// Date of last purchase
        /// </summary>
        public DateTime? LastPurchaseDate { get; set; }

        /// <summary>
        /// When the customer was first seen
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the profile was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a customer segment
    /// </summary>
    public class CustomerSegment
    {
        /// <summary>
        /// Unique identifier for the segment
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the segment
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the segment
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Number of customers in this segment
        /// </summary>
        public int CustomerCount { get; set; }

        /// <summary>
        /// Average value of customers in this segment
        /// </summary>
        public decimal AverageValue { get; set; }

        /// <summary>
        /// When the segment was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the segment was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Rules that define this segment
        /// </summary>
        public Dictionary<string, object> Rules { get; set; } = new();
    }

    /// <summary>
    /// Represents a customer insight
    /// </summary>
    public class CustomerInsight
    {
        /// <summary>
        /// Type of insight
        /// </summary>
        public InsightType Type { get; set; }

        /// <summary>
        /// Title of the insight
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the insight
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// When the insight was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a customer behavior prediction
    /// </summary>
    public class CustomerPrediction
    {
        /// <summary>
        /// ID of the customer
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Type of prediction
        /// </summary>
        public PredictionType Type { get; set; }

        /// <summary>
        /// Predicted value (e.g., probability of churn)
        /// </summary>
        public float PredictedValue { get; set; }

        /// <summary>
        /// Confidence in the prediction (0-1)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Explanation of the prediction
        /// </summary>
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// When the prediction was made
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Query parameters for customer segments
    /// </summary>
    public class CustomerSegmentQuery
    {
        /// <summary>
        /// Filter by segment name
        /// </summary>
        public string? NameContains { get; set; }

        /// <summary>
        /// Minimum number of customers in the segment
        /// </summary>
        public int? MinCustomerCount { get; set; }

        /// <summary>
        /// Maximum number of results to return
        /// </summary>
        public int Limit { get; set; } = 100;

        /// <summary>
        /// Sort order
        /// </summary>
        public SortOrder SortBy { get; set; } = SortOrder.Descending;
    }

    /// <summary>
    /// Types of customer insights
    /// </summary>
    [Flags]
    public enum InsightType
    {
        /// <summary>
        /// No specific type
        /// </summary>
        None = 0,

        /// <summary>
        /// Purchase patterns and history
        /// </summary>
        PurchasePatterns = 1,

        /// <summary>
        /// Behavioral patterns
        /// </summary>
        BehavioralPatterns = 2,

        /// <summary>
        /// All insight types
        /// </summary>
        All = PurchasePatterns | BehavioralPatterns
    }

    /// <summary>
    /// Types of customer behavior predictions
    /// </summary>
    public enum PredictionType
    {
        /// <summary>
        /// Likelihood of customer churn
        /// </summary>
        Churn,

        /// <summary>
        /// Likelihood of making a purchase
        /// </summary>
        Purchase,

        /// <summary>
        /// Predicted lifetime value
        /// </summary>
        LifetimeValue
    }

    /// <summary>
    /// Sort order for queries
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Ascending order
        /// </summary>
        Ascending,

        /// <summary>
        /// Descending order
        /// </summary>
        Descending
    }

    /// <summary>
    /// Interface for customer intelligence operations
    /// </summary>
    public interface ICustomerIntelligenceManager
    {
        /// <summary>
        /// Gets a customer profile by ID
        /// </summary>
        Task<CustomerProfile> GetCustomerProfileAsync(
            string customerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets customer segments based on query parameters
        /// </summary>
        Task<IEnumerable<CustomerSegment>> GetCustomerSegmentsAsync(
            CustomerSegmentQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates insights for a customer
        /// </summary>
        Task<IEnumerable<CustomerInsight>> GenerateCustomerInsightsAsync(
            string customerId,
            InsightType insightType = InsightType.All,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Predicts customer behavior
        /// </summary>
        Task<CustomerPrediction> PredictCustomerBehaviorAsync(
            string customerId,
            PredictionType predictionType,
            CancellationToken cancellationToken = default);
    }
}
