# ConclAIve Integration Guide

## How to Integrate ConclAIve into Your Application

ConclAIve is designed to seamlessly integrate with the existing Cognitive Mesh architecture. This guide shows you how to use it in your services.

## Step 1: Register Services with Dependency Injection

First, register the ConclAIve services in your DI container. This is typically done in your startup or configuration file.

```csharp
using Microsoft.Extensions.DependencyInjection;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Engines;
using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;

public static class ConclAIveServiceRegistration
{
    public static IServiceCollection AddConclAIveReasoning(this IServiceCollection services)
    {
        // Register the reasoning engines
        services.AddScoped<IDebateReasoningPort, DebateReasoningEngine>();
        services.AddScoped<ISequentialReasoningPort, SequentialReasoningEngine>();
        services.AddScoped<IStrategicSimulationPort, StrategicSimulationEngine>();
        
        // Register the main orchestrator
        services.AddScoped<IConclAIveOrchestratorPort, ConclAIveOrchestrator>();
        
        return services;
    }
}

// In your Startup.cs or Program.cs:
services.AddConclAIveReasoning();
```

## Step 2: Use ConclAIve in Your Services

### Example 1: Business Strategy Analysis

```csharp
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;

public class StrategyAnalysisService
{
    private readonly IConclAIveOrchestratorPort _conclAIve;
    private readonly ILogger<StrategyAnalysisService> _logger;

    public StrategyAnalysisService(
        IConclAIveOrchestratorPort conclAIve,
        ILogger<StrategyAnalysisService> logger)
    {
        _conclAIve = conclAIve;
        _logger = logger;
    }

    public async Task<string> AnalyzeMarketEntry(string market, string product)
    {
        var query = $"Should we enter the {market} market with our {product} product?";
        
        var context = new Dictionary<string, string>
        {
            ["market"] = market,
            ["product"] = product,
            ["timeframe"] = "2-3 years",
            ["riskTolerance"] = "moderate"
        };

        // Let ConclAIve auto-select the best reasoning approach
        var result = await _conclAIve.ReasonAsync(query, context: context);

        _logger.LogInformation(
            "Strategy analysis completed with {Confidence}% confidence using {Recipe}",
            result.Confidence * 100,
            result.RecipeType
        );

        return result.Conclusion;
    }
}
```

### Example 2: Ethical Decision Making with Debate

```csharp
public class EthicalDecisionService
{
    private readonly IDebateReasoningPort _debateReasoning;

    public EthicalDecisionService(IDebateReasoningPort debateReasoning)
    {
        _debateReasoning = debateReasoning;
    }

    public async Task<ReasoningOutput> EvaluateEthicalDilemma(string dilemma)
    {
        var request = new DebateRequest
        {
            Question = dilemma,
            Perspectives = new List<string>
            {
                "Utilitarian Ethics (Greatest good for greatest number)",
                "Deontological Ethics (Duty and rules-based)",
                "Virtue Ethics (Character and moral excellence)",
                "Care Ethics (Relationships and empathy)"
            },
            Context = new Dictionary<string, string>
            {
                { "domain", "Healthcare" },
                { "stakeholders", "Patients, Doctors, Society" }
            },
            VotingMechanism = "consensus"
        };

        return await _debateReasoning.ExecuteDebateAsync(request);
    }
}
```

### Example 3: Complex Problem Decomposition

```csharp
public class ProductDevelopmentService
{
    private readonly ISequentialReasoningPort _sequentialReasoning;

    public ProductDevelopmentService(ISequentialReasoningPort sequentialReasoning)
    {
        _sequentialReasoning = sequentialReasoning;
    }

    public async Task<ReasoningOutput> PlanProductLaunch(string productName)
    {
        var request = new SequentialReasoningRequest
        {
            Question = $"Create a comprehensive go-to-market plan for {productName}",
            Phases = new List<string>
            {
                "Market Research and Target Audience Analysis",
                "Competitive Landscape Assessment",
                "Value Proposition and Messaging",
                "Channel Strategy and Distribution",
                "Pricing Strategy",
                "Launch Timeline and Milestones"
            },
            Context = new Dictionary<string, string>
            {
                { "productName", productName },
                { "industry", "SaaS" },
                { "budget", "$500K" }
            }
        };

        return await _sequentialReasoning.ExecuteSequentialReasoningAsync(request);
    }
}
```

### Example 4: Risk Analysis with Strategic Simulation

```csharp
public class RiskManagementService
{
    private readonly IStrategicSimulationPort _strategicSimulation;

    public RiskManagementService(IStrategicSimulationPort strategicSimulation)
    {
        _strategicSimulation = strategicSimulation;
    }

    public async Task<ReasoningOutput> AnalyzeRegulatoryRisks()
    {
        var request = new StrategicSimulationRequest
        {
            Scenario = "Impact of new AI regulations on our business model",
            Patterns = new List<string>
            {
                "PESTEL Analysis",
                "Risk Assessment Framework",
                "Scenario Planning"
            },
            Constraints = new List<string>
            {
                "Must comply with GDPR",
                "Cannot relocate operations",
                "Limited budget for compliance"
            },
            DataPoints = new Dictionary<string, string>
            {
                { "currentRevenue", "$10M ARR" },
                { "aiIntensity", "High - 70% of features use AI" },
                { "geography", "EU and US" }
            },
            ScenarioCount = 4
        };

        return await _strategicSimulation.ExecuteStrategicSimulationAsync(request);
    }
}
```

## Step 3: Access and Use Reasoning Outputs

All reasoning methods return a `ReasoningOutput` object with full auditability:

```csharp
public async Task ProcessReasoningResult(ReasoningOutput result)
{
    // Get the final conclusion
    Console.WriteLine($"Conclusion: {result.Conclusion}");
    Console.WriteLine($"Confidence: {result.Confidence:P0}");
    Console.WriteLine($"Recipe Used: {result.RecipeType}");

    // Audit the reasoning process
    Console.WriteLine("\n=== Reasoning Trace ===");
    foreach (var step in result.ReasoningTrace)
    {
        Console.WriteLine($"\nStep {step.StepNumber}: {step.StepName}");
        Console.WriteLine($"Timestamp: {step.Timestamp}");
        Console.WriteLine($"Output: {step.Output.Substring(0, Math.Min(100, step.Output.Length))}...");
        
        if (step.Metadata.Any())
        {
            Console.WriteLine($"Metadata: {string.Join(", ", step.Metadata.Select(kv => $"{kv.Key}={kv.Value}"))}");
        }
    }

    // Store for compliance and audit
    await StoreReasoningAuditTrail(result);
}
```

## Integration with MetacognitiveLayer

ConclAIve can be monitored by the MetacognitiveLayer for quality assurance:

```csharp
public class ReasoningQualityMonitor
{
    private readonly IConclAIveOrchestratorPort _conclAIve;
    private readonly ILogger<ReasoningQualityMonitor> _logger;

    public async Task<ReasoningOutput> ReasonWithMonitoring(string query)
    {
        var startTime = DateTimeOffset.UtcNow;
        
        try
        {
            var result = await _conclAIve.ReasonAsync(query);
            
            var duration = DateTimeOffset.UtcNow - startTime;
            
            // Monitor quality metrics
            _logger.LogInformation(
                "Reasoning completed: Confidence={Confidence:P0}, Steps={Steps}, Duration={Duration}ms",
                result.Confidence,
                result.ReasoningTrace.Count,
                duration.TotalMilliseconds
            );

            // Flag low confidence results for review
            if (result.Confidence < 0.5)
            {
                _logger.LogWarning(
                    "Low confidence reasoning detected: {Confidence:P0} for query: {Query}",
                    result.Confidence,
                    query
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reasoning failed for query: {Query}", query);
            throw;
        }
    }
}
```

## API Controller Example

Expose ConclAIve through a REST API:

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/reasoning")]
public class ReasoningController : ControllerBase
{
    private readonly IConclAIveOrchestratorPort _conclAIve;

    public ReasoningController(IConclAIveOrchestratorPort conclAIve)
    {
        _conclAIve = conclAIve;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<ReasoningOutput>> Analyze(
        [FromBody] ReasoningRequest request)
    {
        var result = await _conclAIve.ReasonAsync(
            request.Query,
            request.RecipeType,
            request.Context
        );

        return Ok(result);
    }

    [HttpPost("debate")]
    public async Task<ActionResult<ReasoningOutput>> Debate(
        [FromBody] DebateRequest request)
    {
        var result = await _conclAIve.ReasonAsync(
            request.Question,
            ReasoningRecipeType.DebateAndVote,
            request.Context
        );

        return Ok(result);
    }
}

public class ReasoningRequest
{
    public string Query { get; set; } = string.Empty;
    public ReasoningRecipeType? RecipeType { get; set; }
    public Dictionary<string, string> Context { get; set; } = new();
}
```

## Best Practices

### 1. Choose the Right Recipe

- **Debate & Vote**: Use for controversial topics, ethical dilemmas, or multi-stakeholder decisions
- **Sequential**: Use for complex problems that benefit from step-by-step analysis
- **Strategic Simulation**: Use for future planning, risk analysis, or exploring multiple scenarios

### 2. Provide Rich Context

Always provide relevant context to improve reasoning quality:

```csharp
var context = new Dictionary<string, string>
{
    ["industry"] = "Healthcare",
    ["constraints"] = "Budget limited, HIPAA compliance required",
    ["stakeholders"] = "Patients, Doctors, Insurance",
    ["timeframe"] = "Q1 2025",
    ["priority"] = "High"
};
```

### 3. Monitor and Audit

Always log and audit reasoning outputs for compliance:

```csharp
await _auditLogger.LogReasoningAsync(new AuditEvent
{
    EventType = "StructuredReasoning",
    RecipeType = result.RecipeType.ToString(),
    Confidence = result.Confidence,
    StepCount = result.ReasoningTrace.Count,
    SessionId = result.SessionId
});
```

### 4. Handle Low Confidence Results

Implement fallback strategies for low-confidence results:

```csharp
if (result.Confidence < 0.6)
{
    // Request human review
    await _humanReviewService.QueueForReview(result);
    
    // Or try a different recipe
    var alternateResult = await _conclAIve.ReasonAsync(
        query,
        ReasoningRecipeType.DebateAndVote // Try debate for more perspectives
    );
}
```

## Troubleshooting

### Issue: Reasoning takes too long

**Solution**: Reduce the number of perspectives, scenarios, or phases:

```csharp
var request = new DebateRequest
{
    Question = query,
    Perspectives = perspectives.Take(3).ToList(), // Limit to 3 perspectives
    // ...
};
```

### Issue: Low confidence results

**Solution**: Provide more context or use a different recipe:

```csharp
// Add more context
context["additionalInfo"] = "Historical data shows...";
context["expertOpinion"] = "Industry experts suggest...";

// Or use Debate for multiple perspectives
var result = await _conclAIve.ReasonAsync(
    query,
    ReasoningRecipeType.DebateAndVote,
    context
);
```

### Issue: Inconsistent results

**Solution**: Enable result caching and use the same context:

```csharp
// Cache results by query + context hash
var cacheKey = GenerateCacheKey(query, context);
if (_cache.TryGetValue(cacheKey, out ReasoningOutput cachedResult))
{
    return cachedResult;
}

var result = await _conclAIve.ReasonAsync(query, context: context);
_cache.Set(cacheKey, result, TimeSpan.FromHours(1));
return result;
```
