using System.Text;
using System.Text.Json;

namespace CognitiveMesh.ReasoningLayer.AnalyticalReasoning;

/// <summary>
/// Provides multi-perspective cognitive analysis by combining OpenAI completions
/// with distributed perspective endpoints for comprehensive reasoning.
/// </summary>
public class MultiPerspectiveCognition
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly Dictionary<string, string> _perspectiveEndpoints;
    private readonly HttpClient _httpClient;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiPerspectiveCognition"/> class.
    /// </summary>
    public MultiPerspectiveCognition(
        string openAIEndpoint,
        string openAIApiKey,
        string completionDeployment,
        Dictionary<string, string> perspectiveEndpoints)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _perspectiveEndpoints = perspectiveEndpoints;
        _httpClient = new HttpClient();
    }
    
    /// <summary>
    /// Analyzes a query from multiple perspectives in parallel and synthesizes the results.
    /// </summary>
    public async Task<MultiPerspectiveAnalysis> AnalyzeFromMultiplePerspectivesAsync(
        string query,
        List<string> perspectives)
    {
        var tasks = new List<Task<PerspectiveResult>>();
        
        // Process each perspective in parallel
        foreach (var perspective in perspectives)
        {
            tasks.Add(GetPerspectiveAnalysisAsync(perspective, query));
        }
        
        // Wait for all perspective analyses to complete
        var perspectiveResults = await Task.WhenAll(tasks);
        
        // Synthesize results
        var synthesis = await SynthesizePerspectivesAsync(query, perspectiveResults.ToList());
        
        return new MultiPerspectiveAnalysis
        {
            Query = query,
            PerspectiveResults = perspectiveResults.ToList(),
            Synthesis = synthesis
        };
    }
    
    private async Task<PerspectiveResult> GetPerspectiveAnalysisAsync(string perspective, string query)
    {
        // Check if we have a custom endpoint for this perspective
        if (_perspectiveEndpoints.TryGetValue(perspective, out var endpoint))
        {
            // Call custom perspective service
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { query }), 
                    Encoding.UTF8, 
                    "application/json")
            };
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PerspectiveResult>(content);

            return result ?? new PerspectiveResult { Perspective = perspective, Analysis = string.Empty, Confidence = 0.0 };
        }
        else
        {
            // Use OpenAI with perspective-specific prompt
            var systemPrompt = GetSystemPromptForPerspective(perspective);
            
            var chatCompletionOptions = new ChatCompletionsOptions
            {
                DeploymentName = _completionDeployment,
                Temperature = 0.5f,
                MaxTokens = 800,
                Messages =
                {
                    new ChatRequestSystemMessage(systemPrompt),
                    new ChatRequestUserMessage(query)
                }
            };
            
            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
            
            return new PerspectiveResult
            {
                Perspective = perspective,
                Analysis = response.Value.Choices[0].Message.Content,
                Confidence = 0.8 // Default confidence for OpenAI-generated perspectives
            };
        }
    }
    
    private string GetSystemPromptForPerspective(string perspective)
    {
        switch (perspective.ToLower())
        {
            case "analytical":
                return "You are an analytical thinker who breaks down problems into their component parts. " +
                       "Focus on logical analysis, evidence, and methodical reasoning. " +
                       "Identify key factors, relationships, and implications.";
                  
            case "creative":
                return "You are a creative thinker who generates novel ideas and connections. " +
                       "Focus on possibilities, alternatives, and innovative approaches. " +
                       "Think outside conventional boundaries and suggest unique perspectives.";
                  
            case "critical":
                return "You are a critical thinker who evaluates claims and identifies flaws. " +
                       "Focus on questioning assumptions, spotting logical fallacies, and assessing evidence quality. " +
                       "Provide balanced evaluation of strengths and weaknesses.";
                  
            case "practical":
                return "You are a practical thinker who focuses on implementation and real-world application. " +
                       "Focus on feasibility, resources required, and concrete steps. " +
                       "Consider constraints and provide actionable recommendations.";
                  
            case "ethical":
                return "You are an ethical thinker who considers moral implications and values. " +
                       "Focus on principles, stakeholder impacts, and ethical frameworks. " +
                       "Identify potential ethical dilemmas and considerations.";
                  
            default:
                return $"You are a {perspective} thinker. Analyze the query from this perspective.";
        }
    }
    
    private async Task<string> SynthesizePerspectivesAsync(
        string query, 
        List<PerspectiveResult> perspectiveResults)
    {
        // Format perspectives for synthesis
        var perspectivesText = new StringBuilder();
        foreach (var result in perspectiveResults)
        {
            perspectivesText.AppendLine($"--- {result.Perspective} Perspective ---");
            perspectivesText.AppendLine(result.Analysis);
            perspectivesText.AppendLine();
        }
        
        // Create synthesis prompt
        var systemPrompt = "You are a meta-cognitive system that synthesizes multiple perspectives into a comprehensive understanding. " +
                           "Identify common themes, resolve contradictions, and create an integrated view that incorporates insights from all perspectives.";
                          
        var userPrompt = $"Query: {query}\n\nPerspectives:\n{perspectivesText}\n\nPlease synthesize these perspectives into a coherent, integrated analysis.";
        
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
        
        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        
        return response.Value.Choices[0].Message.Content;
    }
}

/// <summary>
/// Represents the result of a single perspective analysis.
/// </summary>
public class PerspectiveResult
{
    /// <summary>Gets or sets the perspective name.</summary>
    public string Perspective { get; set; } = string.Empty;
    /// <summary>Gets or sets the analysis output for this perspective.</summary>
    public string Analysis { get; set; } = string.Empty;
    /// <summary>Gets or sets the confidence score (0-1) for this perspective.</summary>
    public double Confidence { get; set; }
}

/// <summary>
/// Represents the combined result of a multi-perspective analysis.
/// </summary>
public class MultiPerspectiveAnalysis
{
    /// <summary>Gets or sets the original query that was analyzed.</summary>
    public string Query { get; set; } = string.Empty;
    /// <summary>Gets or sets the results from each perspective.</summary>
    public List<PerspectiveResult> PerspectiveResults { get; set; } = new();
    /// <summary>Gets or sets the synthesized analysis from all perspectives.</summary>
    public string Synthesis { get; set; } = string.Empty;
}