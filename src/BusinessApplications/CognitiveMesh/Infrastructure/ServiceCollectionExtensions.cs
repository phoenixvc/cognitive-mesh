using Microsoft.Extensions.DependencyInjection;
using CognitiveMesh.ReasoningLayer.AgencyRouter.Ports;
using CognitiveMesh.BusinessApplications.CognitiveMesh.Infrastructure;
using AgencyLayer.CognitiveSovereignty.Ports;
using AgencyLayer.CognitiveSovereignty.Engines;

namespace CognitiveMesh.BusinessApplications.CognitiveMesh.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCognitiveMeshServices(this IServiceCollection services)
    {
        services.AddScoped<IChainOfThoughtPort, ChainOfThoughtEngine>();
        services.AddScoped<IAgencyRouterPort, AgencyRouter>();
        services.AddScoped<ICognitiveAssessmentPort, CognitiveAssessmentEngine>();

        return services;
    }
}

public class ChainOfThoughtEngine : IChainOfThoughtPort
{
    public Task<ChainOfThoughtResult> ReasonAsync(
        string question,
        ChainOfThoughtConfiguration? configuration = null,
        CancellationToken cancellationToken = default)
    {
        return ReasonZeroShotAsync(question, configuration?.PromptPrefix, cancellationToken);
    }

    public Task<ChainOfThoughtResult> ReasonZeroShotAsync(
        string question,
        string? promptPrefix = null,
        CancellationToken cancellationToken = default)
    {
        var prefix = promptPrefix ?? "Let's think step by step.";
        var result = new ChainOfThoughtResult
        {
            Success = true,
            Answer = "Reasoning service not yet implemented. Configure LLM client to enable.",
            FullReasoning = $"{prefix}\n\nQuestion: {question}\n\n[Reasoning would be generated here with an LLM client]",
            Confidence = 0.0,
            Steps = new List<ThoughtStep>
            {
                new() { StepNumber = 1, Thought = "Analyzing the question...", Confidence = 0.5 },
                new() { StepNumber = 2, Thought = "Breaking down the problem...", Confidence = 0.5 },
                new() { StepNumber = 3, Thought = "Formulating the answer...", Confidence = 0.5 }
            },
            Duration = TimeSpan.Zero
        };

        return Task.FromResult(result);
    }

    public Task<ChainOfThoughtResult> ReasonFewShotAsync(
        string question,
        IReadOnlyList<ChainOfThoughtExample> examples,
        CancellationToken cancellationToken = default)
    {
        return ReasonZeroShotAsync(question, cancellationToken: cancellationToken);
    }

    public Task<ChainOfThoughtResult> ReasonWithSelfConsistencyAsync(
        string question,
        int numPaths = 5,
        double temperature = 0.7,
        CancellationToken cancellationToken = default)
    {
        return ReasonZeroShotAsync(question, cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<ChainOfThoughtExample>> GenerateExamplesAsync(
        string domain,
        IReadOnlyList<string> sampleQuestions,
        int numExamples = 5,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ChainOfThoughtExample>>(Array.Empty<ChainOfThoughtExample>());
    }

    public Task<IReadOnlyList<ThoughtStep>> ExtractStepsAsync(
        string reasoning,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ThoughtStep>>(Array.Empty<ThoughtStep>());
    }

    public Task<(bool IsConsistent, IReadOnlyList<string> Issues)> VerifyReasoningAsync(
        IReadOnlyList<ThoughtStep> steps,
        string answer,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<(bool, IReadOnlyList<string>)>((true, Array.Empty<string>()));
    }
}

public enum ChainOfThoughtType
{
    ZeroShot,
    FewShot,
    SelfConsistency,
    Complex,
    AutoCoT
}

public class ThoughtStep
{
    public int StepNumber { get; init; }
    public required string Thought { get; init; }
    public string? IntermediateConclusion { get; init; }
    public double Confidence { get; init; }
}

public class ChainOfThoughtExample
{
    public required string Question { get; init; }
    public required string Reasoning { get; init; }
    public required string Answer { get; init; }
}

public class ChainOfThoughtConfiguration
{
    public ChainOfThoughtType Type { get; init; } = ChainOfThoughtType.ZeroShot;
    public IReadOnlyList<ChainOfThoughtExample> Examples { get; init; } = Array.Empty<ChainOfThoughtExample>();
    public int SelfConsistencyPaths { get; init; } = 5;
    public double SamplingTemperature { get; init; } = 0.7;
    public string? PromptPrefix { get; init; }
    public string? Model { get; init; }
    public bool ExtractStructuredSteps { get; init; } = true;
}

public class ChainOfThoughtResult
{
    public required bool Success { get; init; }
    public string? Answer { get; init; }
    public required string FullReasoning { get; init; }
    public IReadOnlyList<ThoughtStep> Steps { get; init; } = Array.Empty<ThoughtStep>();
    public double Confidence { get; init; }
    public IReadOnlyList<string> AlternativePaths { get; init; } = Array.Empty<string>();
    public Dictionary<string, int> AnswerVotes { get; init; } = new();
    public TimeSpan Duration { get; init; }
}

public interface IChainOfThoughtPort
{
    Task<ChainOfThoughtResult> ReasonAsync(
        string question,
        ChainOfThoughtConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    Task<ChainOfThoughtResult> ReasonZeroShotAsync(
        string question,
        string? promptPrefix = null,
        CancellationToken cancellationToken = default);

    Task<ChainOfThoughtResult> ReasonFewShotAsync(
        string question,
        IReadOnlyList<ChainOfThoughtExample> examples,
        CancellationToken cancellationToken = default);

    Task<ChainOfThoughtResult> ReasonWithSelfConsistencyAsync(
        string question,
        int numPaths = 5,
        double temperature = 0.7,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChainOfThoughtExample>> GenerateExamplesAsync(
        string domain,
        IReadOnlyList<string> sampleQuestions,
        int numExamples = 5,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThoughtStep>> ExtractStepsAsync(
        string reasoning,
        CancellationToken cancellationToken = default);

    Task<(bool IsConsistent, IReadOnlyList<string> Issues)> VerifyReasoningAsync(
        IReadOnlyList<ThoughtStep> steps,
        string answer,
        CancellationToken cancellationToken = default);
}

public class AgencyRouter : IAgencyRouterPort
{
    private readonly Dictionary<string, AutonomyLevel> _taskTypeDefaults = new()
    {
        { "CreativeWriting", AutonomyLevel.SovereigntyFirst },
        { "ContentCreation", AutonomyLevel.SovereigntyFirst },
        { "Writing", AutonomyLevel.SovereigntyFirst },
        { "DataAnalysis", AutonomyLevel.FullyAutonomous },
        { "Analysis", AutonomyLevel.FullyAutonomous },
        { "CodeGeneration", AutonomyLevel.FullyAutonomous },
        { "Coding", AutonomyLevel.FullyAutonomous },
        { "Research", AutonomyLevel.ActWithConfirmation },
        { "Summarization", AutonomyLevel.FullyAutonomous },
        { "Translation", AutonomyLevel.ActWithConfirmation },
        { "General", AutonomyLevel.RecommendOnly }
    };

    public Task<AgencyModeDecision> RouteTaskAsync(TaskContext context)
    {
        var baseLevel = GetBaseAutonomyLevel(context.TaskType);
        var ciaAdjusted = AdjustForCia(baseLevel, context.CognitiveImpactAssessmentScore);
        var finalLevel = AdjustForCsi(ciaAdjusted, context.CognitiveSovereigntyIndexScore);

        var engine = GetRecommendedEngine(finalLevel, context.TaskType);
        var justification = BuildJustification(context, baseLevel, finalLevel);

        var decision = new AgencyModeDecision
        {
            DecisionId = Guid.NewGuid().ToString(),
            CorrelationId = context.Provenance.CorrelationId,
            ChosenAutonomyLevel = finalLevel,
            RecommendedEngine = engine,
            Justification = justification,
            PolicyVersionApplied = "1.0.0"
        };

        return Task.FromResult(decision);
    }

    private static AutonomyLevel GetBaseAutonomyLevel(string taskType)
    {
        var normalized = NormalizeTaskType(taskType);
        return normalized switch
        {
            "CreativeWriting" or "ContentCreation" or "Writing" => AutonomyLevel.SovereigntyFirst,
            "DataAnalysis" or "Analysis" or "CodeGeneration" or "Coding" => AutonomyLevel.FullyAutonomous,
            "Research" or "Translation" => AutonomyLevel.ActWithConfirmation,
            _ => AutonomyLevel.RecommendOnly
        };
    }

    private static string NormalizeTaskType(string taskType)
    {
        if (string.IsNullOrWhiteSpace(taskType)) return "General";
        var normalized = taskType.Replace(" ", "").Replace("_", "");
        return char.ToUpper(normalized[0]) + normalized[1..];
    }

    private static AutonomyLevel AdjustForCia(AutonomyLevel baseLevel, double ciaScore)
    {
        if (ciaScore >= 0.8) return AutonomyLevel.RecommendOnly;
        if (ciaScore >= 0.6) return AutonomyLevel.ActWithConfirmation;
        return baseLevel;
    }

    private static AutonomyLevel AdjustForCsi(AutonomyLevel currentLevel, double csiScore)
    {
        if (csiScore <= 0.3) return AutonomyLevel.SovereigntyFirst;
        if (csiScore <= 0.5 && currentLevel > AutonomyLevel.ActWithConfirmation)
            return AutonomyLevel.ActWithConfirmation;
        return currentLevel;
    }

    private static string GetRecommendedEngine(AutonomyLevel level, string taskType)
    {
        var normalized = NormalizeTaskType(taskType);
        return level switch
        {
            AutonomyLevel.SovereigntyFirst => "HumanAuthoredWorkflow",
            AutonomyLevel.RecommendOnly => "HumanInTheLoopWorkflow",
            AutonomyLevel.ActWithConfirmation => "GuidedAutonomousAgent",
            AutonomyLevel.FullyAutonomous => normalized switch
            {
                "CodeGeneration" or "Coding" => "CodeGenAgent",
                "DataAnalysis" or "Analysis" => "AnalyticsAgent",
                _ => "FullyAutonomousAgent"
            },
            _ => "HybridWorkflow"
        };
    }

    private static string BuildJustification(TaskContext context, AutonomyLevel baseLevel, AutonomyLevel finalLevel)
    {
        var parts = new List<string>();

        parts.Add($"Task type '{context.TaskType}' maps to base autonomy level: {baseLevel}.");

        if (context.CognitiveImpactAssessmentScore > 0)
        {
            var ciaImpact = context.CognitiveImpactAssessmentScore switch
            {
                >= 0.8 => "high CIA score triggers RecommendOnly for human oversight",
                >= 0.6 => "elevated CIA score reduces autonomy to ActWithConfirmation",
                _ => $"CIA score ({context.CognitiveImpactAssessmentScore:F2}) allows full autonomy"
            };
            parts.Add(ciaImpact);
        }

        if (context.CognitiveSovereigntyIndexScore > 0)
        {
            var csiImpact = context.CognitiveSovereigntyIndexScore switch
            {
                <= 0.3 => "low CSI score triggers SovereigntyFirst mode",
                <= 0.5 => "moderate CSI score encourages human authorship",
                _ => $"CSI score ({context.CognitiveSovereigntyIndexScore:F2}) supports autonomous operation"
            };
            parts.Add(csiImpact);
        }

        if (baseLevel != finalLevel)
        {
            parts.Add($"Autonomy adjusted from {baseLevel} to {finalLevel} based on cognitive scores.");
        }

        return string.Join(" ", parts);
    }

    public Task<bool> ApplyOverrideAsync(OverrideRequest request)
    {
        return Task.FromResult(true);
    }

    public Task<PolicyConfiguration> GetPolicyAsync(string tenantId)
    {
        return Task.FromResult(new PolicyConfiguration
        {
            PolicyId = "default",
            TenantId = tenantId,
            PolicyVersion = "1.0.0",
            Rules = new List<RoutingRule>()
        });
    }

    public Task<bool> UpdatePolicyAsync(PolicyConfiguration policy)
    {
        return Task.FromResult(true);
    }

    public Task<TaskContext> GetIntrospectionDataAsync(string taskId, string tenantId)
    {
        return Task.FromResult(new TaskContext
        {
            TaskId = taskId,
            TaskType = "Unknown",
            TaskDescription = "No context available",
            Provenance = new ProvenanceContext
            {
                TenantId = tenantId,
                CorrelationId = Guid.NewGuid().ToString()
            }
        });
    }
}
