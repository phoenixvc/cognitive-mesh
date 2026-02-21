using System.Text.RegularExpressions;
using Azure;
using Azure.AI.OpenAI;
using CognitiveMesh.ReasoningLayer.AnalyticalReasoning;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.SelfEvaluation;

/// <summary>
/// Provides metacognitive oversight by evaluating AI-generated responses
/// across multiple dimensions (factual accuracy, reasoning quality, relevance, completeness)
/// and generating improved responses based on evaluation feedback.
/// </summary>
public class MetacognitiveOversightComponent
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<MetacognitiveOversightComponent> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetacognitiveOversightComponent"/> class.
    /// </summary>
    /// <param name="openAIEndpoint">The Azure OpenAI endpoint URI.</param>
    /// <param name="openAIApiKey">The Azure OpenAI API key.</param>
    /// <param name="completionDeployment">The deployment name for chat completions.</param>
    /// <param name="telemetryClient">The Application Insights telemetry client.</param>
    /// <param name="logger">The logger instance.</param>
    public MetacognitiveOversightComponent(
        string openAIEndpoint,
        string openAIApiKey,
        string completionDeployment,
        TelemetryClient telemetryClient,
        ILogger<MetacognitiveOversightComponent> logger)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    /// <summary>
    /// Evaluates a response across multiple dimensions and returns a metacognitive evaluation.
    /// </summary>
    /// <param name="query">The original query.</param>
    /// <param name="response">The response to evaluate.</param>
    /// <param name="meshResponse">The cognitive mesh response containing knowledge and perspective data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the evaluation result.</returns>
    public async Task<MetacognitiveEvaluation> EvaluateResponseAsync(
        string query,
        string response,
        CognitiveMeshResponse meshResponse,
        CancellationToken cancellationToken = default)
    {
        // Track evaluation start
        var startTime = DateTime.UtcNow;
        var evaluationOperation = _telemetryClient.StartOperation<RequestTelemetry>("MetacognitiveEvaluation");

        try
        {
            // Step 1: Evaluate factual accuracy
            var factualAccuracy = await EvaluateFactualAccuracyAsync(query, response, meshResponse.KnowledgeResults);

            // Step 2: Evaluate reasoning quality
            var reasoningQuality = await EvaluateReasoningQualityAsync(query, response, meshResponse.PerspectiveAnalysis);

            // Step 3: Evaluate relevance
            var relevance = await EvaluateRelevanceAsync(query, response);

            // Step 4: Evaluate completeness
            var completeness = await EvaluateCompletenessAsync(query, response);

            // Step 5: Generate improvement suggestions
            var improvements = await GenerateImprovementsAsync(
                query,
                response,
                factualAccuracy,
                reasoningQuality,
                relevance,
                completeness);

            // Create evaluation result
            var evaluation = new MetacognitiveEvaluation
            {
                QueryId = meshResponse.QueryId,
                FactualAccuracy = factualAccuracy,
                ReasoningQuality = reasoningQuality,
                Relevance = relevance,
                Completeness = completeness,
                ImprovementSuggestions = improvements,
                EvaluationTime = DateTime.UtcNow - startTime
            };

            // Track metrics
            _telemetryClient.TrackMetric("MetacognitiveEvaluation.FactualAccuracy", factualAccuracy.Score);
            _telemetryClient.TrackMetric("MetacognitiveEvaluation.ReasoningQuality", reasoningQuality.Score);
            _telemetryClient.TrackMetric("MetacognitiveEvaluation.Relevance", relevance.Score);
            _telemetryClient.TrackMetric("MetacognitiveEvaluation.Completeness", completeness.Score);

            return evaluation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating response for query: {Query}", query);
            throw;
        }
        finally
        {
            // Complete the operation
            _telemetryClient.StopOperation(evaluationOperation);
        }
    }

    private async Task<EvaluationDimension> EvaluateFactualAccuracyAsync(
        string query,
        string responseText,
        List<KnowledgeDocument> knowledgeResults)
    {
        // Format knowledge for evaluation
        var knowledgeText = new StringBuilder();
        foreach (var doc in knowledgeResults)
        {
            knowledgeText.AppendLine($"--- {doc.Title} ---");
            knowledgeText.AppendLine($"Source: {doc.Source}");
            knowledgeText.AppendLine(doc.Content);
            knowledgeText.AppendLine();
        }

        // Create system prompt
        var systemPrompt = "You are a factual accuracy evaluation system. " +
                           "Assess whether the response contains factual claims that are supported by the provided knowledge. " +
                           "Identify any factual errors or unsupported claims.";

        var userPrompt = $"Query: {query}\n\n" +
                         $"Response to evaluate: {responseText}\n\n" +
                         $"Knowledge sources:\n{knowledgeText}\n\n" +
                         "Evaluate the factual accuracy of the response. " +
                         "Provide a score from 0.0 (completely inaccurate) to 1.0 (completely accurate). " +
                         "Explain your reasoning and identify any factual errors or unsupported claims.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.1f,
            MaxTokens = 1000,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var completionResponse = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var evaluationText = completionResponse.Value.Choices[0].Message.Content;

        // Extract score and explanation
        var score = ExtractScore(evaluationText);

        return new EvaluationDimension
        {
            Dimension = "FactualAccuracy",
            Score = score,
            Explanation = evaluationText
        };
    }

    private async Task<EvaluationDimension> EvaluateReasoningQualityAsync(
        string query,
        string responseText,
        MultiPerspectiveAnalysis perspectiveAnalysis)
    {
        // Format perspectives for evaluation
        var perspectivesText = new StringBuilder();
        foreach (var perspective in perspectiveAnalysis.PerspectiveResults)
        {
            perspectivesText.AppendLine($"--- {perspective.Perspective} Perspective ---");
            perspectivesText.AppendLine(perspective.Analysis);
            perspectivesText.AppendLine();
        }

        perspectivesText.AppendLine("--- Synthesis ---");
        perspectivesText.AppendLine(perspectiveAnalysis.Synthesis);

        // Create system prompt
        var systemPrompt = "You are a reasoning quality evaluation system. " +
                           "Assess the logical coherence, consideration of multiple perspectives, and quality of inferences in the response.";

        var userPrompt = $"Query: {query}\n\n" +
                         $"Response to evaluate: {responseText}\n\n" +
                         $"Perspective analyses:\n{perspectivesText}\n\n" +
                         "Evaluate the reasoning quality of the response. " +
                         "Provide a score from 0.0 (poor reasoning) to 1.0 (excellent reasoning). " +
                         "Consider logical coherence, consideration of multiple perspectives, and quality of inferences.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.1f,
            MaxTokens = 1000,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var completionResponse = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var evaluationText = completionResponse.Value.Choices[0].Message.Content;

        // Extract score and explanation
        var score = ExtractScore(evaluationText);

        return new EvaluationDimension
        {
            Dimension = "ReasoningQuality",
            Score = score,
            Explanation = evaluationText
        };
    }

    private async Task<EvaluationDimension> EvaluateRelevanceAsync(string query, string responseText)
    {
        // Create system prompt
        var systemPrompt = "You are a relevance evaluation system. " +
                           "Assess how directly the response addresses the query and whether it contains irrelevant information.";

        var userPrompt = $"Query: {query}\n\n" +
                         $"Response to evaluate: {responseText}\n\n" +
                         "Evaluate the relevance of the response to the query. " +
                         "Provide a score from 0.0 (completely irrelevant) to 1.0 (perfectly relevant). " +
                         "Explain your reasoning and identify any irrelevant content.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.1f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var completionResponse = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var evaluationText = completionResponse.Value.Choices[0].Message.Content;

        // Extract score and explanation
        var score = ExtractScore(evaluationText);

        return new EvaluationDimension
        {
            Dimension = "Relevance",
            Score = score,
            Explanation = evaluationText
        };
    }

    private async Task<EvaluationDimension> EvaluateCompletenessAsync(string query, string responseText)
    {
        // Create system prompt
        var systemPrompt = "You are a completeness evaluation system. " +
                           "Assess whether the response fully addresses all aspects of the query or if important elements are missing.";

        var userPrompt = $"Query: {query}\n\n" +
                         $"Response to evaluate: {responseText}\n\n" +
                         "Evaluate the completeness of the response. " +
                         "Provide a score from 0.0 (very incomplete) to 1.0 (fully complete). " +
                         "Explain your reasoning and identify any missing elements.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.1f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var completionResponse = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var evaluationText = completionResponse.Value.Choices[0].Message.Content;

        // Extract score and explanation
        var score = ExtractScore(evaluationText);

        return new EvaluationDimension
        {
            Dimension = "Completeness",
            Score = score,
            Explanation = evaluationText
        };
    }

    private async Task<List<string>> GenerateImprovementsAsync(
        string query,
        string responseText,
        EvaluationDimension factualAccuracy,
        EvaluationDimension reasoningQuality,
        EvaluationDimension relevance,
        EvaluationDimension completeness)
    {
        // Format evaluations for improvement generation
        var evaluationsText = new StringBuilder();
        evaluationsText.AppendLine($"Factual Accuracy (Score: {factualAccuracy.Score:F2}):");
        evaluationsText.AppendLine(factualAccuracy.Explanation);
        evaluationsText.AppendLine();

        evaluationsText.AppendLine($"Reasoning Quality (Score: {reasoningQuality.Score:F2}):");
        evaluationsText.AppendLine(reasoningQuality.Explanation);
        evaluationsText.AppendLine();

        evaluationsText.AppendLine($"Relevance (Score: {relevance.Score:F2}):");
        evaluationsText.AppendLine(relevance.Explanation);
        evaluationsText.AppendLine();

        evaluationsText.AppendLine($"Completeness (Score: {completeness.Score:F2}):");
        evaluationsText.AppendLine(completeness.Explanation);

        // Create system prompt
        var systemPrompt = "You are an improvement suggestion system. " +
                           "Based on the evaluations of a response, suggest specific ways to improve it.";

        var userPrompt = $"Query: {query}\n\n" +
                         $"Response: {responseText}\n\n" +
                         $"Evaluations:\n{evaluationsText}\n\n" +
                         "Suggest 3-5 specific improvements that would address the weaknesses identified in the evaluations. " +
                         "Format each suggestion as a separate point.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 1000,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var completionResponse = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var suggestionsText = completionResponse.Value.Choices[0].Message.Content;

        // Parse suggestions
        return ParseSuggestions(suggestionsText);
    }

    private static double ExtractScore(string text)
    {
        // Look for patterns like "Score: 0.8" or "I rate this as 0.8"
        var scorePattern = @"(?:score|rating|rate)(?:\s*(?:of|as|is|:))?\s*(\d+(?:\.\d+)?)";
        var match = Regex.Match(text, scorePattern, RegexOptions.IgnoreCase);

        if (match.Success && double.TryParse(match.Groups[1].Value, out var score))
        {
            return Math.Min(Math.Max(score, 0.0), 1.0); // Clamp between 0 and 1
        }

        // Fallback: scan for any number between 0 and 1
        var numberPattern = @"(\d+\.\d+)";
        match = Regex.Match(text, numberPattern);

        if (match.Success && double.TryParse(match.Groups[1].Value, out score))
        {
            if (score >= 0.0 && score <= 1.0)
            {
                return score;
            }
        }

        // Default score if no valid score found
        return 0.5;
    }

    private static List<string> ParseSuggestions(string suggestionsText)
    {
        var suggestions = new List<string>();

        // Split by numbered points or bullet points
        var lines = suggestionsText.Split('\n');
        var currentSuggestion = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check if this is a new suggestion
            if (Regex.IsMatch(trimmedLine, @"^(\d+\.|\-|\*)\s") && currentSuggestion.Length > 0)
            {
                // Add previous suggestion to list
                suggestions.Add(currentSuggestion.ToString().Trim());
                currentSuggestion.Clear();
            }

            // Append to current suggestion
            if (!string.IsNullOrWhiteSpace(trimmedLine))
            {
                currentSuggestion.AppendLine(trimmedLine);
            }
        }

        // Add final suggestion if any
        if (currentSuggestion.Length > 0)
        {
            suggestions.Add(currentSuggestion.ToString().Trim());
        }

        return suggestions;
    }

    /// <summary>
    /// Generates an improved response based on metacognitive evaluation feedback.
    /// </summary>
    /// <param name="query">The original query.</param>
    /// <param name="originalResponse">The original response to improve.</param>
    /// <param name="evaluation">The metacognitive evaluation of the original response.</param>
    /// <param name="meshResponse">The cognitive mesh response containing knowledge and perspective data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the improved response text.</returns>
    public async Task<string> GenerateImprovedResponseAsync(
        string query,
        string originalResponse,
        MetacognitiveEvaluation evaluation,
        CognitiveMeshResponse meshResponse,
        CancellationToken cancellationToken = default)
    {
        // Format improvement suggestions
        var suggestionsText = new StringBuilder();
        foreach (var suggestion in evaluation.ImprovementSuggestions)
        {
            suggestionsText.AppendLine($"- {suggestion}");
        }

        // Format knowledge for context
        var knowledgeText = new StringBuilder();
        foreach (var doc in meshResponse.KnowledgeResults.Take(3)) // Limit to top 3 for brevity
        {
            knowledgeText.AppendLine($"--- {doc.Title} ---");
            knowledgeText.AppendLine($"Source: {doc.Source}");
            knowledgeText.AppendLine(doc.Content);
            knowledgeText.AppendLine();
        }

        // Format perspectives for context
        var perspectivesText = new StringBuilder();
        foreach (var perspective in meshResponse.PerspectiveAnalysis.PerspectiveResults)
        {
            perspectivesText.AppendLine($"--- {perspective.Perspective} Perspective ---");
            perspectivesText.AppendLine(perspective.Analysis);
            perspectivesText.AppendLine();
        }

        // Create system prompt
        var systemPrompt = "You are a response improvement system. " +
                           "Your task is to generate an improved version of a response based on evaluation feedback. " +
                           "Maintain the core information while addressing the identified weaknesses.";

        var userPrompt = $"Query: {query}\n\n" +
                         $"Original Response: {originalResponse}\n\n" +
                         $"Improvement Suggestions:\n{suggestionsText}\n\n" +
                         $"Relevant Knowledge:\n{knowledgeText}\n\n" +
                         $"Perspective Analyses:\n{perspectivesText}\n\n" +
                         "Generate an improved response that addresses the suggestions while maintaining the core information.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 1500,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var completionResponse = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);

        return completionResponse.Value.Choices[0].Message.Content;
    }
}

/// <summary>
/// Represents the result of a metacognitive evaluation across multiple dimensions.
/// </summary>
public class MetacognitiveEvaluation
{
    /// <summary>Gets or sets the query identifier.</summary>
    public string QueryId { get; set; } = string.Empty;

    /// <summary>Gets or sets the factual accuracy evaluation dimension.</summary>
    public EvaluationDimension FactualAccuracy { get; set; } = new();

    /// <summary>Gets or sets the reasoning quality evaluation dimension.</summary>
    public EvaluationDimension ReasoningQuality { get; set; } = new();

    /// <summary>Gets or sets the relevance evaluation dimension.</summary>
    public EvaluationDimension Relevance { get; set; } = new();

    /// <summary>Gets or sets the completeness evaluation dimension.</summary>
    public EvaluationDimension Completeness { get; set; } = new();

    /// <summary>Gets or sets the list of improvement suggestions.</summary>
    public List<string> ImprovementSuggestions { get; set; } = new();

    /// <summary>Gets or sets the time taken for the evaluation.</summary>
    public TimeSpan EvaluationTime { get; set; }
}

/// <summary>
/// Represents a single dimension of evaluation with a score and explanation.
/// </summary>
public class EvaluationDimension
{
    /// <summary>Gets or sets the name of the dimension being evaluated.</summary>
    public string Dimension { get; set; } = string.Empty;

    /// <summary>Gets or sets the evaluation score (0.0 to 1.0).</summary>
    public double Score { get; set; }

    /// <summary>Gets or sets the explanation for the score.</summary>
    public string Explanation { get; set; } = string.Empty;
}

/// <summary>
/// Represents a response from the cognitive mesh pipeline, including knowledge results
/// and multi-perspective analysis data used during metacognitive oversight evaluation.
/// </summary>
public class CognitiveMeshResponse
{
    /// <summary>Gets or sets the query identifier.</summary>
    public string QueryId { get; set; } = string.Empty;

    /// <summary>Gets or sets the original query.</summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>Gets or sets the generated response.</summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>Gets or sets the processing time.</summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>Gets or sets the knowledge documents retrieved for the query.</summary>
    public List<KnowledgeDocument> KnowledgeResults { get; set; } = new();

    /// <summary>Gets or sets the multi-perspective analysis results.</summary>
    public MultiPerspectiveAnalysis PerspectiveAnalysis { get; set; } = new();
}

/// <summary>
/// Represents a document in the knowledge management system used for factual accuracy evaluation.
/// </summary>
public class KnowledgeDocument
{
    /// <summary>Gets or sets the unique identifier of the document.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the title of the document.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the content of the document.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the source of the document.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the category of the document.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets the tags associated with the document.</summary>
    public List<string> Tags { get; set; } = new();
}
