using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CognitiveMesh.ReasoningLayer.AgencyRouter.Ports;
using CognitiveMesh.BusinessApplications.CognitiveMesh.Infrastructure;
using AgencyLayer.CognitiveSovereignty.Models;
using AgencyLayer.CognitiveSovereignty.Ports;
using IChainOfThoughtPort = CognitiveMesh.BusinessApplications.CognitiveMesh.Infrastructure.IChainOfThoughtPort;
using ChainOfThoughtConfiguration = CognitiveMesh.BusinessApplications.CognitiveMesh.Infrastructure.ChainOfThoughtConfiguration;
using ChainOfThoughtType = CognitiveMesh.BusinessApplications.CognitiveMesh.Infrastructure.ChainOfThoughtType;

namespace CognitiveMesh.BusinessApplications.CognitiveMesh.Controllers;

[ApiController]
[Route("api/v1/cognitive")]
[Produces("application/json")]
public class CognitiveMeshController : ControllerBase
{
    private readonly ILogger<CognitiveMeshController> _logger;
    private readonly IChainOfThoughtPort _chainOfThoughtPort;
    private readonly IAgencyRouterPort _agencyRouterPort;
    private readonly ICognitiveAssessmentPort _cognitiveAssessmentPort;

    public CognitiveMeshController(
        ILogger<CognitiveMeshController> logger,
        IChainOfThoughtPort chainOfThoughtPort,
        IAgencyRouterPort agencyRouterPort,
        ICognitiveAssessmentPort cognitiveAssessmentPort)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chainOfThoughtPort = chainOfThoughtPort ?? throw new ArgumentNullException(nameof(chainOfThoughtPort));
        _agencyRouterPort = agencyRouterPort ?? throw new ArgumentNullException(nameof(agencyRouterPort));
        _cognitiveAssessmentPort = cognitiveAssessmentPort ?? throw new ArgumentNullException(nameof(cognitiveAssessmentPort));
    }

    #region Health Endpoints

    /// <summary>
    /// Health check endpoint - returns OK if the service is running.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTimeOffset.UtcNow,
            service = "cognitive-mesh-api"
        });
    }

    #endregion

    #region Reasoning Endpoints

    /// <summary>
    /// Core reasoning endpoint supporting multiple reasoning modes.
    /// </summary>
    /// <param name="request">The reasoning request containing the question and mode.</param>
    /// <returns>The reasoning result with steps, answer, and confidence.</returns>
    [HttpPost("reason")]
    [ProducesResponseType(typeof(ReasoningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReasonAsync([FromBody] ReasoningRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorCode = "INVALID_REQUEST",
                    Message = "Question is required",
                    CorrelationId = correlationId
                });
            }

            var validModes = new[] { "chain-of-thought", "debate", "strategic", "zero-shot", "few-shot", "self-consistency", "complex", "auto-cot" };
            if (!string.IsNullOrEmpty(request.Mode) && !validModes.Contains(request.Mode.ToLowerInvariant()))
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorCode = "INVALID_MODE",
                    Message = $"Mode must be one of: {string.Join(", ", validModes)}",
                    CorrelationId = correlationId
                });
            }

            _logger.LogInformation("Reasoning request received. Mode: {Mode}, CorrelationId: {CorrelationId}", 
                request.Mode ?? "chain-of-thought", correlationId);

            var mode = (request.Mode ?? "chain-of-thought").ToLowerInvariant() switch
            {
                "chain-of-thought" or "zero-shot" => ChainOfThoughtType.ZeroShot,
                "few-shot" => ChainOfThoughtType.FewShot,
                "self-consistency" => ChainOfThoughtType.SelfConsistency,
                "complex" => ChainOfThoughtType.Complex,
                "auto-cot" => ChainOfThoughtType.AutoCoT,
                "debate" => ChainOfThoughtType.SelfConsistency,
                "strategic" => ChainOfThoughtType.Complex,
                _ => ChainOfThoughtType.ZeroShot
            };

            var config = new ChainOfThoughtConfiguration
            {
                Type = mode,
                ExtractStructuredSteps = request.ExtractSteps,
                Model = request.Model,
                SelfConsistencyPaths = request.NumPaths ?? 5,
                SamplingTemperature = request.Temperature ?? 0.7
            };

            var result = await _chainOfThoughtPort.ReasonAsync(request.Question, config);

            var steps = result.Steps.Select(s => new ThoughtStepDto
            {
                StepNumber = s.StepNumber,
                Thought = s.Thought,
                IntermediateConclusion = s.IntermediateConclusion,
                Confidence = s.Confidence
            }).ToList();

            var response = new ReasoningResponse
            {
                Success = result.Success,
                Question = request.Question,
                Answer = result.Answer ?? string.Empty,
                FullReasoning = result.FullReasoning,
                Confidence = result.Confidence,
                Steps = steps,
                Duration = result.Duration,
                CorrelationId = correlationId,
                Mode = request.Mode ?? "chain-of-thought"
            };

            _logger.LogInformation("Reasoning completed. Success: {Success}, Duration: {Duration}ms", 
                result.Success, result.Duration.TotalMilliseconds);

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Reasoning request timed out. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(StatusCodes.Status504GatewayTimeout, new ErrorResponse
            {
                ErrorCode = "TIMEOUT",
                Message = "The reasoning request timed out",
                CorrelationId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reasoning. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                ErrorCode = "REASONING_ERROR",
                Message = "An error occurred while processing the reasoning request",
                CorrelationId = correlationId
            });
        }
    }

    #endregion

    #region Agency Endpoints

    /// <summary>
    /// Routes a task to the appropriate agency mode based on context.
    /// </summary>
    /// <param name="request">The agency routing request.</param>
    /// <returns>The agency mode decision with autonomy level and justification.</returns>
    [HttpPost("agency/route")]
    [ProducesResponseType(typeof(AgencyRoutingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RouteAgencyAsync([FromBody] AgencyRoutingRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        try
        {
            if (string.IsNullOrWhiteSpace(request.TaskType))
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorCode = "INVALID_REQUEST",
                    Message = "TaskType is required",
                    CorrelationId = correlationId
                });
            }

            _logger.LogInformation("Agency routing request for task type: {TaskType}. CorrelationId: {CorrelationId}", 
                request.TaskType, correlationId);

            var tenantId = GetTenantId();
            
            var taskContext = new TaskContext
            {
                TaskId = request.TaskId ?? Guid.NewGuid().ToString(),
                TaskType = request.TaskType,
                TaskDescription = request.TaskDescription ?? string.Empty,
                CognitiveImpactAssessmentScore = request.CiaScore,
                CognitiveSovereigntyIndexScore = request.CsiScore,
                InitialPayload = request.InitialPayload ?? new object(),
                Provenance = new ProvenanceContext
                {
                    TenantId = tenantId,
                    ActorId = GetUserId(),
                    CorrelationId = correlationId
                }
            };

            var decision = await _agencyRouterPort.RouteTaskAsync(taskContext);

            var response = new AgencyRoutingResponse
            {
                DecisionId = decision.DecisionId,
                TaskId = taskContext.TaskId,
                AutonomyLevel = decision.ChosenAutonomyLevel.ToString(),
                RecommendedEngine = decision.RecommendedEngine,
                Justification = decision.Justification,
                PolicyVersion = decision.PolicyVersionApplied,
                Timestamp = decision.Timestamp,
                CorrelationId = correlationId
            };

            _logger.LogInformation("Agency routing completed. AutonomyLevel: {AutonomyLevel}, CorrelationId: {CorrelationId}", 
                decision.ChosenAutonomyLevel, correlationId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during agency routing. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                ErrorCode = "AGENCY_ROUTING_ERROR",
                Message = "An error occurred while routing the agency request",
                CorrelationId = correlationId
            });
        }
    }

    /// <summary>
    /// Routes a task using raw CIA metrics — computes CIA 2.0 and CSI scores
    /// internally, then delegates to the agency router.
    /// </summary>
    /// <param name="request">Raw interface metrics plus task context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Agency routing decision with computed CIA/CSI scores attached.</returns>
    [HttpPost("agency/route/computed")]
    [ProducesResponseType(typeof(AgencyRouteComputedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RouteAgencyComputedAsync(
        [FromBody] AgencyRouteComputedRequest request,
        CancellationToken ct = default)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            if (string.IsNullOrWhiteSpace(request.TaskType))
            {
                return BadRequest(new ErrorResponse
                {
                    ErrorCode = "INVALID_REQUEST",
                    Message = "TaskType is required",
                    CorrelationId = correlationId
                });
            }

            _logger.LogInformation(
                "Computed agency routing request for task type: {TaskType}. CorrelationId: {CorrelationId}",
                request.TaskType, correlationId);

            // Step 1 — compute CIA 2.0 and CSI from the raw metrics
            var ciaRequest = new CiaAssessmentRequest
            {
                TransparencyIndex = request.TransparencyIndex,
                AgencyPreservationScore = request.AgencyPreservationScore,
                MetacognitiveAwarenessRate = request.MetacognitiveAwarenessRate,
                AdaptiveControlRange = request.AdaptiveControlRange,
                RiskWeightedMultiplier = request.RiskWeightCia,
                SovereigntyFrictionIndex = request.SovereigntyFrictionIndex,
                SovereigntyTransparencyGap = request.TransparencyGap,
                TaskType = request.TaskType
            };

            var ciaResult = await _cognitiveAssessmentPort.AssessAsync(ciaRequest, ct);

            // Step 2 — compute fluency score from the interaction quality metrics
            var rawFluency = (request.OnboardingSpeed
                            + request.InteractionClarity
                            + request.FeedbackLoops
                            + request.ConfidenceTransfer
                            + request.ErrorRecovery
                            + request.ContextFluency
                            + request.FluencySustainability) / 7.0;

            var fluencyScore = Math.Clamp(
                rawFluency
                    * request.FluencyTrajectoryModifier
                    / request.WorkflowIntegrationPenalty
                    / request.CognitiveLoadMultiplier,
                0.0, 1.0);

            // Step 3 — route using the computed scores
            var taskContext = new TaskContext
            {
                TaskId = request.TaskId ?? Guid.NewGuid().ToString(),
                TaskType = request.TaskType,
                TaskDescription = request.TaskDescription ?? string.Empty,
                CognitiveImpactAssessmentScore = ciaResult.AdjustedCiaScore,
                CognitiveSovereigntyIndexScore = ciaResult.CollectiveSovereigntyIndex,
                InitialPayload = request.InitialPayload ?? new object(),
                Provenance = new ProvenanceContext
                {
                    TenantId = GetTenantId(),
                    ActorId = GetUserId(),
                    CorrelationId = correlationId
                }
            };

            var decision = await _agencyRouterPort.RouteTaskAsync(taskContext);

            var response = new AgencyRouteComputedResponse
            {
                DecisionId = decision.DecisionId,
                TaskId = taskContext.TaskId,
                AutonomyLevel = decision.ChosenAutonomyLevel.ToString(),
                RecommendedEngine = decision.RecommendedEngine,
                Justification = decision.Justification,
                PolicyVersion = decision.PolicyVersionApplied,
                Timestamp = decision.Timestamp,
                CorrelationId = correlationId,
                ComputedScores = new ComputedScores
                {
                    RawCia = ciaResult.RawCiaScore,
                    CiaScore = ciaResult.AdjustedCiaScore,
                    CsiScore = ciaResult.CollectiveSovereigntyIndex,
                    RawFluency = rawFluency,
                    FluencyScore = fluencyScore
                }
            };

            _logger.LogInformation(
                "Computed agency routing completed. CIA={Cia:F4} CSI={Csi:F4} Fluency={Fluency:F4} AutonomyLevel={Level}. CorrelationId: {CorrelationId}",
                ciaResult.AdjustedCiaScore, ciaResult.CollectiveSovereigntyIndex, fluencyScore,
                decision.ChosenAutonomyLevel, correlationId);

            return Ok(response);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid metric value in computed routing request. CorrelationId: {CorrelationId}", correlationId);
            return BadRequest(new ErrorResponse
            {
                ErrorCode = "INVALID_METRIC",
                Message = ex.Message,
                CorrelationId = correlationId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during computed agency routing. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                ErrorCode = "COMPUTED_ROUTING_ERROR",
                Message = "An error occurred while processing the computed routing request",
                CorrelationId = correlationId
            });
        }
    }

    #endregion

    #region Helper Methods

    private string GetUserId()
    {
        return User?.Identity?.Name ?? "anonymous";
    }

    private string GetTenantId()
    {
        return "default"; 
    }

    #endregion
}

#region Request/Response Models

public class ReasoningRequest
{
    /// <summary>The question to reason about.</summary>
    public required string Question { get; set; }
    
    /// <summary>Reasoning mode: chain-of-thought, debate, strategic, zero-shot, few-shot, self-consistency, complex, auto-cot</summary>
    public string? Mode { get; set; }
    
    /// <summary>Whether to extract structured reasoning steps.</summary>
    public bool ExtractSteps { get; set; } = true;
    
    /// <summary>Model to use for reasoning.</summary>
    public string? Model { get; set; }
    
    /// <summary>Number of reasoning paths for self-consistency mode.</summary>
    public int? NumPaths { get; set; }
    
    /// <summary>Temperature for sampling in self-consistency mode.</summary>
    public double? Temperature { get; set; }
}

public class ReasoningResponse
{
    /// <summary>Whether the reasoning succeeded.</summary>
    public bool Success { get; set; }
    
    /// <summary>The original question.</summary>
    public string Question { get; set; } = string.Empty;
    
    /// <summary>The final answer.</summary>
    public string Answer { get; set; } = string.Empty;
    
    /// <summary>The full reasoning text.</summary>
    public string FullReasoning { get; set; } = string.Empty;
    
    /// <summary>Confidence score (0.0 - 1.0).</summary>
    public double Confidence { get; set; }
    
    /// <summary>Extracted reasoning steps.</summary>
    public List<ThoughtStepDto> Steps { get; set; } = new();
    
    /// <summary>Time taken to generate reasoning.</summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>Correlation ID for tracking.</summary>
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>The reasoning mode used.</summary>
    public string Mode { get; set; } = string.Empty;
}

public class ThoughtStepDto
{
    public int StepNumber { get; set; }
    public string Thought { get; set; } = string.Empty;
    public string? IntermediateConclusion { get; set; }
    public double Confidence { get; set; }
}

public class AgencyRoutingRequest
{
    /// <summary>Unique identifier for the task (optional, auto-generated if not provided).</summary>
    public string? TaskId { get; set; }
    
    /// <summary>The type of task to route.</summary>
    public required string TaskType { get; set; }
    
    /// <summary>Description of the task.</summary>
    public string? TaskDescription { get; set; }
    
    /// <summary>Cognitive Impact Assessment score (0.0 - 1.0).</summary>
    public double CiaScore { get; set; }
    
    /// <summary>Cognitive Sovereignty Index score (0.0 - 1.0).</summary>
    public double CsiScore { get; set; }
    
    /// <summary>Initial payload for the task.</summary>
    public object? InitialPayload { get; set; }
}

public class AgencyRoutingResponse
{
    /// <summary>Unique identifier for this routing decision.</summary>
    public string DecisionId { get; set; } = string.Empty;
    
    /// <summary>The task ID.</summary>
    public string TaskId { get; set; } = string.Empty;
    
    /// <summary>The chosen autonomy level.</summary>
    public string AutonomyLevel { get; set; } = string.Empty;
    
    /// <summary>Recommended engine for execution.</summary>
    public string RecommendedEngine { get; set; } = string.Empty;
    
    /// <summary>Justification for the routing decision.</summary>
    public string Justification { get; set; } = string.Empty;
    
    /// <summary>Policy version applied.</summary>
    public string PolicyVersion { get; set; } = string.Empty;
    
    /// <summary>Timestamp of the decision.</summary>
    public DateTimeOffset Timestamp { get; set; }
    
    /// <summary>Correlation ID for tracking.</summary>
    public string CorrelationId { get; set; } = string.Empty;
}

public class ErrorResponse
{
    /// <summary>Error code.</summary>
    public string ErrorCode { get; set; } = string.Empty;
    
    /// <summary>Error message.</summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>Correlation ID for tracking.</summary>
    public string CorrelationId { get; set; } = string.Empty;
}

public class AgencyRouteComputedRequest
{
    public string? TaskId { get; set; }
    public required string TaskType { get; set; }
    public string? TaskDescription { get; set; }
    public object? InitialPayload { get; set; }
    
    public double TransparencyIndex { get; set; }
    public double AgencyPreservationScore { get; set; }
    public double MetacognitiveAwarenessRate { get; set; }
    public double AdaptiveControlRange { get; set; }
    public double RiskWeightCia { get; set; } = 1.0;
    public double SovereigntyFrictionIndex { get; set; } = 1.0;
    public double TransparencyGap { get; set; }
    
    public double OnboardingSpeed { get; set; }
    public double InteractionClarity { get; set; }
    public double FeedbackLoops { get; set; }
    public double ConfidenceTransfer { get; set; }
    public double ErrorRecovery { get; set; }
    public double ContextFluency { get; set; }
    public double FluencySustainability { get; set; }
    public double FluencyTrajectoryModifier { get; set; } = 1.0;
    public double WorkflowIntegrationPenalty { get; set; } = 1.0;
    public double CognitiveLoadMultiplier { get; set; } = 1.0;
}

public class ComputedScores
{
    public double CiaScore { get; set; }
    public double CsiScore { get; set; }
    public double FluencyScore { get; set; }
    public double RawCia { get; set; }
    public double RawFluency { get; set; }
}

public class AgencyRouteComputedResponse
{
    public string DecisionId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string AutonomyLevel { get; set; } = string.Empty;
    public string RecommendedEngine { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public string PolicyVersion { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public ComputedScores ComputedScores { get; set; } = new();
}

#endregion
