using System.ComponentModel.DataAnnotations;
using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Models;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Controllers;

/// <summary>
/// API controller for Impact-Driven AI Metrics, providing endpoints for
/// psychological safety scoring, mission alignment assessment, adoption telemetry,
/// and comprehensive impact reporting.
/// </summary>
[ApiController]
[Route("api/v1/impact-metrics")]
[Produces("application/json")]
public class ImpactMetricsController : ControllerBase
{
    private readonly IPsychologicalSafetyPort _safetyPort;
    private readonly IMissionAlignmentPort _alignmentPort;
    private readonly IAdoptionTelemetryPort _telemetryPort;
    private readonly IImpactAssessmentPort _assessmentPort;
    private readonly ILogger<ImpactMetricsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImpactMetricsController"/> class.
    /// </summary>
    /// <param name="safetyPort">Port for psychological safety scoring.</param>
    /// <param name="alignmentPort">Port for mission alignment assessment.</param>
    /// <param name="telemetryPort">Port for adoption telemetry tracking.</param>
    /// <param name="assessmentPort">Port for impact assessment and report generation.</param>
    /// <param name="logger">Logger instance for structured logging.</param>
    public ImpactMetricsController(
        IPsychologicalSafetyPort safetyPort,
        IMissionAlignmentPort alignmentPort,
        IAdoptionTelemetryPort telemetryPort,
        IImpactAssessmentPort assessmentPort,
        ILogger<ImpactMetricsController> logger)
    {
        _safetyPort = safetyPort ?? throw new ArgumentNullException(nameof(safetyPort));
        _alignmentPort = alignmentPort ?? throw new ArgumentNullException(nameof(alignmentPort));
        _telemetryPort = telemetryPort ?? throw new ArgumentNullException(nameof(telemetryPort));
        _assessmentPort = assessmentPort ?? throw new ArgumentNullException(nameof(assessmentPort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Psychological Safety Endpoints

    /// <summary>
    /// Calculates the psychological safety score for a team.
    /// </summary>
    /// <param name="teamId">The identifier of the team to assess.</param>
    /// <param name="request">The request containing survey scores and tenant context.</param>
    /// <returns>The calculated psychological safety score.</returns>
    /// <response code="200">Returns the calculated safety score.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpPost("safety-score/{teamId}")]
    [ProducesResponseType(typeof(PsychologicalSafetyScore), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PsychologicalSafetyScore>> CalculateSafetyScoreAsync(
        string teamId,
        [FromBody] CalculateSafetyScoreRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation(
            "Safety score calculation requested for team {TeamId} with correlation ID {CorrelationId}",
            teamId, correlationId);

        try
        {
            if (string.IsNullOrWhiteSpace(teamId))
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Team ID is required", correlationId));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Invalid request payload", correlationId));
            }

            var result = await _safetyPort.CalculateSafetyScoreAsync(
                teamId, request.TenantId, request.SurveyScores);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating safety score for team {TeamId}", teamId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while calculating safety score", correlationId));
        }
    }

    /// <summary>
    /// Retrieves historical psychological safety scores for a team.
    /// </summary>
    /// <param name="teamId">The identifier of the team.</param>
    /// <param name="tenantId">The tenant to which the team belongs.</param>
    /// <returns>A list of historical safety scores.</returns>
    /// <response code="200">Returns the historical safety scores.</response>
    /// <response code="400">If the parameters are invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet("safety-score/{teamId}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<PsychologicalSafetyScore>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<PsychologicalSafetyScore>>> GetHistoricalScoresAsync(
        string teamId,
        [FromQuery] string tenantId)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            if (string.IsNullOrWhiteSpace(teamId) || string.IsNullOrWhiteSpace(tenantId))
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Team ID and tenant ID are required", correlationId));
            }

            var result = await _safetyPort.GetHistoricalScoresAsync(teamId, tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving historical safety scores for team {TeamId}", teamId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while retrieving historical scores", correlationId));
        }
    }

    #endregion

    #region Mission Alignment Endpoints

    /// <summary>
    /// Assesses the alignment of a decision with the organisation's mission statement.
    /// </summary>
    /// <param name="request">The request containing the decision context and mission statement.</param>
    /// <returns>The mission alignment assessment result.</returns>
    /// <response code="200">Returns the alignment assessment.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpPost("alignment")]
    [ProducesResponseType(typeof(MissionAlignment), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MissionAlignment>> AssessAlignmentAsync(
        [FromBody] AssessAlignmentRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation(
            "Alignment assessment requested for decision {DecisionId} with correlation ID {CorrelationId}",
            request.DecisionId, correlationId);

        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Invalid request payload", correlationId));
            }

            var result = await _alignmentPort.AssessAlignmentAsync(
                request.DecisionId, request.DecisionContext, request.MissionStatement);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing alignment for decision {DecisionId}", request.DecisionId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while assessing alignment", correlationId));
        }
    }

    #endregion

    #region Adoption Telemetry Endpoints

    /// <summary>
    /// Records an adoption telemetry event.
    /// </summary>
    /// <param name="request">The telemetry event to record.</param>
    /// <returns>A confirmation that the event was recorded.</returns>
    /// <response code="200">The telemetry event was recorded successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpPost("telemetry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RecordTelemetryAsync(
        [FromBody] RecordTelemetryRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation(
            "Telemetry event recording requested for user {UserId} in tenant {TenantId}",
            request.UserId, request.TenantId);

        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Invalid request payload", correlationId));
            }

            var telemetry = new AdoptionTelemetry(
                TelemetryId: Guid.NewGuid().ToString(),
                UserId: request.UserId,
                TenantId: request.TenantId,
                ToolId: request.ToolId,
                Action: request.Action,
                Timestamp: DateTimeOffset.UtcNow,
                DurationMs: request.DurationMs,
                Context: request.Context);

            await _telemetryPort.RecordActionAsync(telemetry);

            return Ok(new { telemetry.TelemetryId, Message = "Telemetry event recorded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording telemetry for user {UserId}", request.UserId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while recording telemetry", correlationId));
        }
    }

    /// <summary>
    /// Retrieves AI tool usage summary for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant whose usage to summarise.</param>
    /// <returns>A list of telemetry events.</returns>
    /// <response code="200">Returns the usage summary.</response>
    /// <response code="400">If the tenant ID is invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet("telemetry/{tenantId}/summary")]
    [ProducesResponseType(typeof(IReadOnlyList<AdoptionTelemetry>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<AdoptionTelemetry>>> GetUsageSummaryAsync(
        string tenantId)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Tenant ID is required", correlationId));
            }

            var result = await _telemetryPort.GetUsageSummaryAsync(tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage summary for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while retrieving usage summary", correlationId));
        }
    }

    /// <summary>
    /// Detects resistance patterns in AI adoption for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant to analyse for resistance patterns.</param>
    /// <returns>A list of detected resistance indicators.</returns>
    /// <response code="200">Returns the resistance patterns.</response>
    /// <response code="400">If the tenant ID is invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet("telemetry/{tenantId}/resistance")]
    [ProducesResponseType(typeof(IReadOnlyList<ResistanceIndicator>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<ResistanceIndicator>>> GetResistancePatternsAsync(
        string tenantId)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Tenant ID is required", correlationId));
            }

            var result = await _telemetryPort.DetectResistancePatternsAsync(tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting resistance patterns for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while detecting resistance patterns", correlationId));
        }
    }

    #endregion

    #region Impact Assessment Endpoints

    /// <summary>
    /// Generates an impact assessment for a tenant over a specified time period.
    /// </summary>
    /// <param name="tenantId">The tenant to assess.</param>
    /// <param name="request">The request containing the assessment period.</param>
    /// <returns>The generated impact assessment.</returns>
    /// <response code="200">Returns the impact assessment.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpPost("assessment/{tenantId}")]
    [ProducesResponseType(typeof(ImpactAssessment), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImpactAssessment>> GenerateAssessmentAsync(
        string tenantId,
        [FromBody] GenerateAssessmentRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation(
            "Impact assessment requested for tenant {TenantId} with correlation ID {CorrelationId}",
            tenantId, correlationId);

        try
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Tenant ID is required", correlationId));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Invalid request payload", correlationId));
            }

            var result = await _assessmentPort.GenerateAssessmentAsync(
                tenantId, request.PeriodStart, request.PeriodEnd);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating impact assessment for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while generating the assessment", correlationId));
        }
    }

    /// <summary>
    /// Generates a comprehensive impact report for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant for which to generate the report.</param>
    /// <param name="periodStart">Start of the reporting period.</param>
    /// <param name="periodEnd">End of the reporting period.</param>
    /// <returns>The comprehensive impact report.</returns>
    /// <response code="200">Returns the impact report.</response>
    /// <response code="400">If the parameters are invalid.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet("report/{tenantId}")]
    [ProducesResponseType(typeof(ImpactReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorEnvelope), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImpactReport>> GenerateReportAsync(
        string tenantId,
        [FromQuery] DateTimeOffset? periodStart,
        [FromQuery] DateTimeOffset? periodEnd)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation(
            "Impact report requested for tenant {TenantId} with correlation ID {CorrelationId}",
            tenantId, correlationId);

        try
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return BadRequest(ErrorEnvelope.InvalidPayload("Tenant ID is required", correlationId));
            }

            var start = periodStart ?? DateTimeOffset.UtcNow.AddDays(-30);
            var end = periodEnd ?? DateTimeOffset.UtcNow;

            var result = await _assessmentPort.GenerateReportAsync(tenantId, start, end);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating impact report for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorEnvelope.Create("INTERNAL_ERROR", "An unexpected error occurred while generating the report", correlationId));
        }
    }

    #endregion
}

#region API Request Models

/// <summary>
/// Request payload for calculating a psychological safety score.
/// </summary>
public class CalculateSafetyScoreRequest
{
    /// <summary>
    /// The tenant to which the team belongs.
    /// </summary>
    [Required]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Survey-based scores per safety dimension, each between 0 and 100.
    /// </summary>
    [Required]
    public Dictionary<SafetyDimension, double> SurveyScores { get; set; } = new();
}

/// <summary>
/// Request payload for assessing mission alignment of a decision.
/// </summary>
public class AssessAlignmentRequest
{
    /// <summary>
    /// The identifier of the decision being assessed.
    /// </summary>
    [Required]
    public string DecisionId { get; set; } = string.Empty;

    /// <summary>
    /// A description of the decision and its context.
    /// </summary>
    [Required]
    public string DecisionContext { get; set; } = string.Empty;

    /// <summary>
    /// The organisation's mission statement to compare against.
    /// </summary>
    [Required]
    public string MissionStatement { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for recording a telemetry event.
/// </summary>
public class RecordTelemetryRequest
{
    /// <summary>
    /// The identifier of the user who performed the action.
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The tenant to which the user belongs.
    /// </summary>
    [Required]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the AI tool being used.
    /// </summary>
    [Required]
    public string ToolId { get; set; } = string.Empty;

    /// <summary>
    /// The type of action performed.
    /// </summary>
    [Required]
    public AdoptionAction Action { get; set; }

    /// <summary>
    /// Duration of the action in milliseconds, if applicable.
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Additional contextual information about the action.
    /// </summary>
    public string? Context { get; set; }
}

/// <summary>
/// Request payload for generating an impact assessment.
/// </summary>
public class GenerateAssessmentRequest
{
    /// <summary>
    /// Start of the assessment period.
    /// </summary>
    [Required]
    public DateTimeOffset PeriodStart { get; set; }

    /// <summary>
    /// End of the assessment period.
    /// </summary>
    [Required]
    public DateTimeOffset PeriodEnd { get; set; }
}

#endregion
