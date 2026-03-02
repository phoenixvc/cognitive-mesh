using System.Text.Json;
using AgencyLayer.MultiAgentOrchestration.Ports;
using AgencyLayer.RefactoringAgents.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.RefactoringAgents;

/// <summary>
/// An autonomous agent that performs SOLID/DRY code analysis on submitted source code.
/// Integrates with the Multi-Agent Orchestration system through the handler registration
/// pattern of <see cref="AgencyLayer.MultiAgentOrchestration.Adapters.InProcessAgentRuntimeAdapter"/>.
/// </summary>
public class SolidDryRefactoringAgent
{
    /// <summary>
    /// The agent type identifier used for orchestrator registration and task routing.
    /// </summary>
    public const string AgentType = "SolidDryRefactoring";

    private readonly ILogger<SolidDryRefactoringAgent> _logger;
    private readonly ICodeRefactoringPort _refactoringEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidDryRefactoringAgent"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="refactoringEngine">The code refactoring analysis engine.</param>
    public SolidDryRefactoringAgent(
        ILogger<SolidDryRefactoringAgent> logger,
        ICodeRefactoringPort refactoringEngine)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _refactoringEngine = refactoringEngine ?? throw new ArgumentNullException(nameof(refactoringEngine));
    }

    /// <summary>
    /// Handles an <see cref="AgentTask"/> dispatched by the orchestrator.
    /// Expects the task context to contain a "sourceCode" key with the code to analyze.
    /// </summary>
    /// <param name="task">The agent task containing source code and analysis parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The analysis result as an object for the orchestrator.</returns>
    public async Task<object> HandleTaskAsync(AgentTask task, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{AgentType} handling task '{TaskId}': {Goal}",
            AgentType, task.TaskId, task.Goal);

        if (!TryExtractParameters(task.Context, out var analysisRequest, out var error))
        {
            _logger.LogError("Invalid parameters for task '{TaskId}': {Error}", task.TaskId, error);
            return new { IsSuccess = false, Error = error };
        }

        try
        {
            var result = await _refactoringEngine.AnalyzeCodeAsync(analysisRequest, cancellationToken);

            _logger.LogInformation("{AgentType} completed task '{TaskId}': {Summary}",
                AgentType, task.TaskId, result.Summary);

            return new
            {
                IsSuccess = true,
                result.SolidViolations,
                result.DryViolations,
                result.Suggestions,
                result.OverallScore,
                result.Summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{AgentType} failed on task '{TaskId}'", AgentType, task.TaskId);
            return new { IsSuccess = false, Error = $"Analysis failed: {ex.Message}" };
        }
    }

    private static bool TryExtractParameters(
        Dictionary<string, object> context,
        out CodeAnalysisRequest request,
        out string error)
    {
        request = new CodeAnalysisRequest();
        error = string.Empty;

        // Extract sourceCode (required)
        string? sourceCode = null;
        if (context.TryGetValue("sourceCode", out var sourceCodeObj))
        {
            sourceCode = sourceCodeObj switch
            {
                string s => s,
                JsonElement { ValueKind: JsonValueKind.String } je => je.GetString(),
                _ => sourceCodeObj?.ToString()
            };
        }

        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            error = "Parameter 'sourceCode' is required and must be a non-empty string.";
            return false;
        }

        request.SourceCode = sourceCode;

        // Extract language (optional, default: csharp)
        if (context.TryGetValue("language", out var langObj))
        {
            request.Language = ExtractString(langObj) ?? "csharp";
        }

        // Extract filePath (optional)
        if (context.TryGetValue("filePath", out var fpObj))
        {
            request.FilePath = ExtractString(fpObj) ?? string.Empty;
        }

        // Extract scope (optional, default: Both)
        if (context.TryGetValue("scope", out var scopeObj))
        {
            var scopeStr = ExtractString(scopeObj);
            if (Enum.TryParse<AnalysisScope>(scopeStr, ignoreCase: true, out var scope))
            {
                request.Scope = scope;
            }
        }

        return true;
    }

    private static string? ExtractString(object? obj)
    {
        if (obj is null) return null;

        return obj switch
        {
            string s => s,
            JsonElement { ValueKind: JsonValueKind.String } je => je.GetString(),
            _ => obj.ToString()
        };
    }
}
