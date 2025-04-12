using Azure.Messaging.EventGrid;
using Azure.Core;
using System.Text.Json;

public class CognitiveMeshCoordinator
{
    private readonly EnhancedRAGSystem _ragSystem;
    private readonly MultiPerspectiveCognition _mpcSystem;
    private readonly ArgenticAgentComponent _agentSystem;
    private readonly EventGridPublisherClient _eventGridClient;
    private readonly ILogger _logger;
    
    public CognitiveMeshCoordinator(
        EnhancedRAGSystem ragSystem,
        MultiPerspectiveCognition mpcSystem,
        ArgenticAgentComponent agentSystem,
        string eventGridEndpoint,
        string eventGridKey,
        ILogger<CognitiveMeshCoordinator> logger)
    {
        _ragSystem = ragSystem;
        _mpcSystem = mpcSystem;
        _agentSystem = agentSystem;
        _eventGridClient = new EventGridPublisherClient(
            new Uri(eventGridEndpoint),
            new AzureKeyCredential(eventGridKey));
        _logger = logger;
    }
    
    public async Task<CognitiveMeshResponse> ProcessQueryAsync(string query, QueryOptions options = null)
    {
        options ??= new QueryOptions();
        
        // Create execution context
        var context = new ExecutionContext
        {
            QueryId = Guid.NewGuid().ToString(),
            Query = query,
            Options = options,
            StartTime = DateTimeOffset.UtcNow
        };
        
        try
        {
            // Log query start
            _logger.LogInformation("Processing query {QueryId}: {Query}", context.QueryId, query);
            
            // Publish query received event
            await PublishEventAsync("QueryReceived", context);
            
            // Step 1: Retrieve relevant knowledge
            var knowledgeTask = _ragSystem.SearchAsync(query, options.MaxKnowledgeItems);
            
            // Step 2: Generate multi-perspective analysis
            var perspectivesTask = _mpcSystem.AnalyzeFromMultiplePerspectivesAsync(
                query, options.Perspectives);
                
            // Wait for both tasks to complete
            await Task.WhenAll(knowledgeTask, perspectivesTask);
            
            var knowledgeResults = knowledgeTask.Result;
            var perspectiveAnalysis = perspectivesTask.Result;
            
            // Update context with results
            context.KnowledgeResults = knowledgeResults;
            context.PerspectiveAnalysis = perspectiveAnalysis;
            
            // Publish intermediate results event
            await PublishEventAsync("IntermediateResultsGenerated", context);
            
            // Step 3: Determine if agent execution is needed
            if (options.EnableAgentExecution && RequiresAgentExecution(query, perspectiveAnalysis))
            {
                // Create agent context
                var agentContext = new Dictionary<string, string>
                {
                    { "query", query },
                    { "knowledge", FormatKnowledgeForContext(knowledgeResults) },
                    { "perspectives", FormatPerspectivesForContext(perspectiveAnalysis) }
                };
                
                // Create and execute plan
                var plan = await _agentSystem.CreatePlanAsync(query, agentContext);
                var executionResult = await _agentSystem.ExecutePlanAsync(plan);
                
                // Update context with agent results
                context.AgentPlan = plan;
                context.AgentResult = executionResult;
                
                // Publish agent execution event
                await PublishEventAsync("AgentExecutionCompleted", context);
            }
            
            // Step 4: Generate final response
            string finalResponse;
            
            if (context.AgentResult != null)
            {
                // Use agent result as primary response
                finalResponse = await GenerateFinalResponseWithAgentResultAsync(
                    query, knowledgeResults, perspectiveAnalysis, context.AgentResult);
            }
            else
            {
                // Generate response from knowledge and perspectives
                finalResponse = await GenerateFinalResponseAsync(
                    query, knowledgeResults, perspectiveAnalysis);
            }
            
            // Create response object
            var response = new CognitiveMeshResponse
            {
                QueryId = context.QueryId,
                Query = query,
                Response = finalResponse,
                KnowledgeResults = knowledgeResults,
                PerspectiveAnalysis = perspectiveAnalysis,
                AgentResult = context.AgentResult,
                ProcessingTime = DateTimeOffset.UtcNow - context.StartTime
            };
            
            // Publish response generated event
            await PublishEventAsync("ResponseGenerated", context);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query {QueryId}: {Message}", context.QueryId, ex.Message);
            
            // Publish error event
            context.Error = ex.Message;
            await PublishEventAsync("QueryError", context);
            
            // Return error response
            return new CognitiveMeshResponse
            {
                QueryId = context.QueryId,
                Query = query,
                Error = ex.Message,
                ProcessingTime = DateTimeOffset.UtcNow - context.StartTime
            };
        }
    }
    
    private bool RequiresAgentExecution(string query, MultiPerspectiveAnalysis perspectiveAnalysis)
    {
        // In a real implementation, this would use more sophisticated logic
        // For now, check if the query contains action-oriented keywords
        var actionKeywords = new[] { "find", "get", "search", "analyze", "create", "generate", "calculate" };
        
        return actionKeywords.Any(keyword => query.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
    
    private string FormatKnowledgeForContext(List<KnowledgeDocument> documents)
    {
        var formattedKnowledge = new StringBuilder();
        
        foreach (var doc in documents)
        {
            formattedKnowledge.AppendLine($"--- {doc.Title} ---");
            formattedKnowledge.AppendLine($"Source: {doc.Source}");
            formattedKnowledge.AppendLine(doc.Content);
            formattedKnowledge.AppendLine();
        }
        
        return formattedKnowledge.ToString();
    }
    
    private string FormatPerspectivesForContext(MultiPerspectiveAnalysis analysis)
    {
        var formattedPerspectives = new StringBuilder();
        
        foreach (var perspective in analysis.PerspectiveResults)
        {
            formattedPerspectives.AppendLine($"--- {perspective.Perspective} Perspective ---");
            formattedPerspectives.AppendLine(perspective.Analysis);
            formattedPerspectives.AppendLine();
        }
        
        formattedPerspectives.AppendLine("--- Synthesis ---");
        formattedPerspectives.AppendLine(analysis.Synthesis);
        
        return formattedPerspectives.ToString();
    }
    
    private async Task<string> GenerateFinalResponseAsync(
        string query, 
        List<KnowledgeDocument> knowledgeResults, 
        MultiPerspectiveAnalysis perspectiveAnalysis)
    {
        // Use RAG system to generate response
        var systemPrompt = "You are an advanced AI assistant that integrates factual knowledge with multiple analytical perspectives. " +
                          "Provide a comprehensive, balanced response that incorporates relevant facts and considers different viewpoints.";
                          
        // Format knowledge and perspectives as context
        var context = new StringBuilder();
        
        // Add knowledge
        context.AppendLine("--- Relevant Knowledge ---");
        foreach (var doc in knowledgeResults)
        {
            context.AppendLine($"Source: {doc.Source} - {doc.Title}");
            context.AppendLine(doc.Content);
            context.AppendLine();
        }
        
        // Add perspectives
        context.AppendLine("--- Analytical Perspectives ---");
        foreach (var perspective in perspectiveAnalysis.PerspectiveResults)
        {
            context.AppendLine($"{perspective.Perspective} Perspective:");
            context.AppendLine(perspective.Analysis);
            context.AppendLine();
        }
        
        // Add synthesis
        context.AppendLine("--- Synthesis of Perspectives ---");
        context.AppendLine(perspectiveAnalysis.Synthesis);
        
        // Generate response using RAG
        return await _ragSystem.GenerateResponseWithRAGAsync(
            query, 
            systemPrompt, 
            context.ToString());
    }
    
    private async Task<string> GenerateFinalResponseWithAgentResultAsync(
        string query,
        List<KnowledgeDocument> knowledgeResults,
        MultiPerspectiveAnalysis perspectiveAnalysis,
        ExecutionResult agentResult)
    {
        // Use RAG system to generate response
        var systemPrompt = "You are an advanced AI assistant that integrates factual knowledge, multiple analytical perspectives, " +
                          "and the results of autonomous actions. Provide a comprehensive response that synthesizes all available information.";
                          
        // Format knowledge, perspectives, and agent results as context
        var context = new StringBuilder();
        
        // Add knowledge
        context.AppendLine("--- Relevant Knowledge ---");
        foreach (var doc in knowledgeResults.Take(2)) // Limit to most relevant
        {
            context.AppendLine($"Source: {doc.Source} - {doc.Title}");
            context.AppendLine(doc.Content);
            context.AppendLine();
        }
        
        // Add perspectives synthesis
        context.AppendLine("--- Analytical Perspective Synthesis ---");
        context.AppendLine(perspectiveAnalysis.Synthesis);
        context.AppendLine();
        
        // Add agent results
        context.AppendLine("--- Agent Execution Results ---");
        context.AppendLine($"Task: {agentResult.Plan.Task}");
        context.AppendLine($"Success: {agentResult.Success}");
        context.AppendLine($"Summary: {agentResult.Summary}");
        context.AppendLine();
        
        // Add key step results
        context.AppendLine("Key Results:");
        foreach (var stepResult in agentResult.StepResults.Where(r => r.Success))
        {
            context.AppendLine($"- {stepResult.Step}: {TruncateResult(stepResult.Result, 200)}");
        }
        
        // Generate response using RAG
        return await _ragSystem.GenerateResponseWithRAGAsync(
            query, 
            systemPrompt, 
            context.ToString());
    }
    
    private string TruncateResult(string result, int maxLength)
    {
        if (result.Length <= maxLength)
            return result;
            
        return result.Substring(0, maxLength - 3) + "...";
    }
    
    private async Task PublishEventAsync(string eventType, ExecutionContext context)
    {
        // Create event data
        var eventData = new EventGridEvent(
            subject: $"CognitiveMesh/Query/{context.QueryId}",
            eventType: $"CognitiveMesh.{eventType}",
            dataVersion: "1.0",
            data: new BinaryData(JsonSerializer.Serialize(new
            {
                queryId = context.QueryId,
                query = context.Query,
                timestamp = DateTimeOffset.UtcNow,
                eventType = eventType
            })));
            
        // Publish event
        await _eventGridClient.SendEventAsync(eventData);
    }
}

public class ExecutionContext
{
    public string QueryId { get; set; }
    public string Query { get; set; }
    public QueryOptions Options { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public List<KnowledgeDocument> KnowledgeResults { get; set; }
    public MultiPerspectiveAnalysis PerspectiveAnalysis { get; set; }
    public PlanningResult AgentPlan { get; set; }
    public ExecutionResult AgentResult { get; set; }
    public string Error { get; set; }
}

public class QueryOptions
{
    public int MaxKnowledgeItems { get; set; } = 5;
    public List<string> Perspectives { get; set; } = new List<string> { "analytical", "creative", "critical", "practical" };
    public bool EnableAgentExecution { get; set; } = true;
}

public class CognitiveMeshResponse
{
    public string QueryId { get; set; }
    public string Query { get; set; }
    public string Response { get; set; }
    public List<KnowledgeDocument> KnowledgeResults { get; set; }
    public MultiPerspectiveAnalysis PerspectiveAnalysis { get; set; }
    public ExecutionResult AgentResult { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string Error { get; set; }
}
