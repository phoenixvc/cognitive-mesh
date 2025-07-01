using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CognitiveMesh.BusinessApplications.ResearchAnalysis;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeWorkController : ControllerBase
{
    private readonly CognitiveMeshCoordinator _coordinator;
    private readonly ILogger<KnowledgeWorkController> _logger;
    
    public KnowledgeWorkController(
        CognitiveMeshCoordinator coordinator,
        ILogger<KnowledgeWorkController> logger)
    {
        _coordinator = coordinator;
        _logger = logger;
    }
    
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

public class ResearchResult
{
    public string Topic { get; set; }
    public string ResearchPlan { get; set; }
    public string Findings { get; set; }
    public string Insights { get; set; }
    public List<ResearchSource> Sources { get; set; } = new List<ResearchSource>();
}

public class ResearchSource
{
    public string Title { get; set; }
    public string Source { get; set; }
    public string Snippet { get; set; }
}

public class SynthesisRequest
{
    public List<DocumentInput> Documents { get; set; } = new List<DocumentInput>();
    public string FocusArea { get; set; }
    public List<string> Elements { get; set; } = new List<string>();
}

public class DocumentInput
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Source { get; set; }
}

public class SynthesisResult
{
    public string FocusArea { get; set; }
    public string Synthesis { get; set; }
    public string KeyTakeaways { get; set; }
}

public class ContentCreationRequest
{
    public string Topic { get; set; }
    public string ContentType { get; set; } // Blog post, white paper, email, etc.
    public string TargetAudience { get; set; }
    public string Style { get; set; }
    public string Length { get; set; } // Short, medium, long
}

public class ContentCreationResult
{
    public string Topic { get; set; }
    public string ContentType { get; set; }
    public string Outline { get; set; }
    public string Content { get; set; }
    public string ResearchSummary { get; set; }
}
