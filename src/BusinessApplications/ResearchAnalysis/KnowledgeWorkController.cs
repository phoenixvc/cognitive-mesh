using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CognitiveMesh.BusinessApplications.ResearchAnalysis;

/// <summary>
/// API controller for knowledge work operations including research, synthesis, and content creation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class KnowledgeWorkController : ControllerBase
{
    private readonly CognitiveMeshCoordinator _coordinator;
    private readonly ILogger<KnowledgeWorkController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="KnowledgeWorkController"/> class.
    /// </summary>
    /// <param name="coordinator">The cognitive mesh coordinator for query processing.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    public KnowledgeWorkController(
        CognitiveMeshCoordinator coordinator,
        ILogger<KnowledgeWorkController> logger)
    {
        _coordinator = coordinator;
        _logger = logger;
    }

    /// <summary>
    /// Researches a topic using multi-perspective analysis and returns findings with sources.
    /// </summary>
    /// <param name="request">The research request containing the topic and focus areas.</param>
    /// <returns>Research results including plan, findings, insights, and sources.</returns>
    [HttpPost("research")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> ResearchTopic([FromBody] ResearchRequest request)
    {
        try
        {
            // Create research plan
            var planQuery = $"Create a comprehensive research plan for the topic: {request.Topic}";
            var planOptions = new QueryOptions
            {
                EnableAgentExecution = true,
                Perspectives = new List<string> { "analytical", "creative", "critical" }
            };

            var planResponse = await _coordinator.ProcessQueryAsync(planQuery, planOptions);

            // Execute research
            var researchQuery = $"Conduct comprehensive research on the topic: {request.Topic}. " +
                               $"Focus areas: {string.Join(", ", request.FocusAreas)}. " +
                               $"Include key facts, different perspectives, and recent developments.";

            var researchOptions = new QueryOptions
            {
                EnableAgentExecution = true,
                MaxKnowledgeItems = 10,
                Perspectives = new List<string> { "analytical", "critical", "practical" }
            };

            var researchResponse = await _coordinator.ProcessQueryAsync(researchQuery, researchOptions);

            // Generate insights
            var insightsQuery = $"Based on research about {request.Topic}, identify key insights, " +
                               "emerging trends, and potential implications.";

            var insightsResponse = await _coordinator.ProcessQueryAsync(insightsQuery);

            // Compile results
            var result = new ResearchResult
            {
                Topic = request.Topic,
                ResearchPlan = planResponse.Response,
                Findings = researchResponse.Response,
                Insights = insightsResponse.Response,
                Sources = researchResponse.KnowledgeResults
                    .Select(k => new ResearchSource
                    {
                        Title = k.Title,
                        Source = k.Source,
                        Snippet = TruncateContent(k.Content, 200)
                    })
                    .ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing research request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Synthesizes multiple documents into a coherent analysis with key takeaways.
    /// </summary>
    /// <param name="request">The synthesis request containing documents and focus area.</param>
    /// <returns>Synthesis results including combined analysis and key takeaways.</returns>
    [HttpPost("synthesize")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> SynthesizeDocuments([FromBody] SynthesisRequest request)
    {
        try
        {
            // Index documents for RAG
            foreach (var document in request.Documents)
            {
                var knowledgeDoc = new KnowledgeDocument
                {
                    Title = document.Title,
                    Content = document.Content,
                    Source = document.Source,
                    Category = "User-Provided",
                    Tags = new List<string> { "temporary", "user-session" }
                };

                await _coordinator.IndexDocumentAsync(knowledgeDoc);
            }

            // Generate synthesis
            var synthesisQuery = $"Synthesize the provided documents with focus on: {request.FocusArea}. " +
                                $"Include {string.Join(", ", request.Elements)}.";

            var options = new QueryOptions
            {
                MaxKnowledgeItems = request.Documents.Count,
                Perspectives = new List<string> { "analytical", "critical", "practical" }
            };

            var response = await _coordinator.ProcessQueryAsync(synthesisQuery, options);

            // Generate key takeaways
            var takeawaysQuery = $"What are the key takeaways from these documents regarding {request.FocusArea}?";
            var takeawaysResponse = await _coordinator.ProcessQueryAsync(takeawaysQuery, options);

            // Compile results
            var result = new SynthesisResult
            {
                FocusArea = request.FocusArea,
                Synthesis = response.Response,
                KeyTakeaways = takeawaysResponse.Response
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing synthesis request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Creates content based on a topic, content type, target audience, and style.
    /// </summary>
    /// <param name="request">The content creation request with topic, type, audience, and style details.</param>
    /// <returns>Content creation results including outline, content, and research summary.</returns>
    [HttpPost("content-creation")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> CreateContent([FromBody] ContentCreationRequest request)
    {
        try
        {
            // Generate content outline
            var outlineQuery = $"Create a detailed outline for {request.ContentType} about {request.Topic}. " +
                             $"Target audience: {request.TargetAudience}. " +
                             $"Style: {request.Style}.";

            var outlineResponse = await _coordinator.ProcessQueryAsync(outlineQuery);

            // Research the topic
            var researchQuery = $"Research key information for {request.ContentType} about {request.Topic}.";
            var researchOptions = new QueryOptions
            {
                MaxKnowledgeItems = 8,
                EnableAgentExecution = true
            };

            var researchResponse = await _coordinator.ProcessQueryAsync(researchQuery, researchOptions);

            // Generate content
            var contentQuery = $"Create {request.ContentType} about {request.Topic}. " +
                             $"Target audience: {request.TargetAudience}. " +
                             $"Style: {request.Style}. " +
                             $"Length: {request.Length}.";

            var contentOptions = new QueryOptions
            {
                Perspectives = new List<string> { "creative", "practical" }
            };

            var contentResponse = await _coordinator.ProcessQueryAsync(contentQuery, contentOptions);

            // Compile results
            var result = new ContentCreationResult
            {
                Topic = request.Topic,
                ContentType = request.ContentType,
                Outline = outlineResponse.Response,
                Content = contentResponse.Response,
                ResearchSummary = researchResponse.Response
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing content creation request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    private string TruncateContent(string content, int maxLength)
    {
        if (content.Length <= maxLength)
            return content;

        return content.Substring(0, maxLength - 3) + "...";
    }
}

/// <summary>
/// Result of a research topic analysis.
/// </summary>
public class ResearchResult
{
    /// <summary>The research topic.</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>The generated research plan.</summary>
    public string ResearchPlan { get; set; } = string.Empty;

    /// <summary>The research findings.</summary>
    public string Findings { get; set; } = string.Empty;

    /// <summary>Key insights derived from the research.</summary>
    public string Insights { get; set; } = string.Empty;

    /// <summary>Sources referenced during the research.</summary>
    public List<ResearchSource> Sources { get; set; } = new List<ResearchSource>();
}

/// <summary>
/// A source referenced during research.
/// </summary>
public class ResearchSource
{
    /// <summary>Title of the source.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Origin or URL of the source.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>A brief snippet from the source content.</summary>
    public string Snippet { get; set; } = string.Empty;
}

/// <summary>
/// Request to synthesize multiple documents into a coherent analysis.
/// </summary>
public class SynthesisRequest
{
    /// <summary>The documents to synthesize.</summary>
    public List<DocumentInput> Documents { get; set; } = new List<DocumentInput>();

    /// <summary>The focus area for the synthesis.</summary>
    public string FocusArea { get; set; } = string.Empty;

    /// <summary>Elements to include in the synthesis.</summary>
    public List<string> Elements { get; set; } = new List<string>();
}

/// <summary>
/// Input document for synthesis operations.
/// </summary>
public class DocumentInput
{
    /// <summary>Title of the document.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Content of the document.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Source or origin of the document.</summary>
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Result of a document synthesis operation.
/// </summary>
public class SynthesisResult
{
    /// <summary>The focus area of the synthesis.</summary>
    public string FocusArea { get; set; } = string.Empty;

    /// <summary>The synthesized analysis.</summary>
    public string Synthesis { get; set; } = string.Empty;

    /// <summary>Key takeaways from the synthesis.</summary>
    public string KeyTakeaways { get; set; } = string.Empty;
}

/// <summary>
/// Request to create content on a specific topic.
/// </summary>
public class ContentCreationRequest
{
    /// <summary>The topic for content creation.</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>The type of content to create (e.g., blog post, white paper, email).</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>The target audience for the content.</summary>
    public string TargetAudience { get; set; } = string.Empty;

    /// <summary>The writing style to use.</summary>
    public string Style { get; set; } = string.Empty;

    /// <summary>The desired length (short, medium, long).</summary>
    public string Length { get; set; } = string.Empty;
}

/// <summary>
/// Result of a content creation operation.
/// </summary>
public class ContentCreationResult
{
    /// <summary>The topic of the created content.</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>The type of content created.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>The content outline.</summary>
    public string Outline { get; set; } = string.Empty;

    /// <summary>The generated content.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Summary of research used for content creation.</summary>
    public string ResearchSummary { get; set; } = string.Empty;
}
