using AgencyLayer.CognitiveSandwich.Models;
using AgencyLayer.CognitiveSandwich.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static CognitiveMesh.Shared.LogSanitizer;

namespace AgencyLayer.CognitiveSandwich.Controllers;

/// <summary>
/// REST API controller for managing Cognitive Sandwich processes.
/// Provides endpoints for creating, querying, advancing, stepping back,
/// and auditing phase-based workflows with cognitive debt monitoring.
/// </summary>
[ApiController]
[Route("api/v1/cognitive-sandwich")]
public class CognitiveSandwichController : ControllerBase
{
    private readonly IPhaseManagerPort _phaseManager;
    private readonly ICognitiveDebtPort _cognitiveDebtPort;
    private readonly ILogger<CognitiveSandwichController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CognitiveSandwichController"/> class.
    /// </summary>
    /// <param name="phaseManager">Port for managing Cognitive Sandwich processes.</param>
    /// <param name="cognitiveDebtPort">Port for assessing cognitive debt.</param>
    /// <param name="logger">Logger instance.</param>
    public CognitiveSandwichController(
        IPhaseManagerPort phaseManager,
        ICognitiveDebtPort cognitiveDebtPort,
        ILogger<CognitiveSandwichController> logger)
    {
        _phaseManager = phaseManager ?? throw new ArgumentNullException(nameof(phaseManager));
        _cognitiveDebtPort = cognitiveDebtPort ?? throw new ArgumentNullException(nameof(cognitiveDebtPort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new Cognitive Sandwich process from the given configuration.
    /// </summary>
    /// <param name="config">Configuration specifying phases, step-back limits, and thresholds.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created process.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SandwichProcess), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSandwichProcess(
        [FromBody] SandwichProcessConfig config,
        CancellationToken ct)
    {
        try
        {
            if (config == null)
            {
                return BadRequest(new { error = "Request body is required." });
            }

            var process = await _phaseManager.CreateProcessAsync(config, ct);

            _logger.LogInformation(
                "Created Cognitive Sandwich process {ProcessId} via API",
                process.ProcessId);

            return CreatedAtAction(
                nameof(GetSandwichProcess),
                new { processId = process.ProcessId },
                process);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration provided for process creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Cognitive Sandwich process");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred while creating the process." });
        }
    }

    /// <summary>
    /// Retrieves an existing Cognitive Sandwich process by its unique identifier.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The process details.</returns>
    [HttpGet("{processId}")]
    [ProducesResponseType(typeof(SandwichProcess), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSandwichProcess(
        string processId,
        CancellationToken ct)
    {
        try
        {
            var process = await _phaseManager.GetProcessAsync(processId, ct);
            return Ok(process);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Process {ProcessId} not found", Sanitize(processId));
            return NotFound(new { error = $"Process '{processId}' not found." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid processId provided");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving process {ProcessId}", Sanitize(processId));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred while retrieving the process." });
        }
    }

    /// <summary>
    /// Advances the process to its next phase after validating conditions and cognitive debt.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="context">Context for the transition including user and optional phase output.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the phase transition attempt.</returns>
    [HttpPost("{processId}/advance")]
    [ProducesResponseType(typeof(PhaseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AdvancePhase(
        string processId,
        [FromBody] PhaseTransitionContext context,
        CancellationToken ct)
    {
        try
        {
            if (context == null)
            {
                return BadRequest(new { error = "Transition context is required." });
            }

            var result = await _phaseManager.TransitionToNextPhaseAsync(processId, context, ct);

            _logger.LogInformation(
                "Phase advance for process {ProcessId}: success={Success}",
                Sanitize(processId), result.Success);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Process {ProcessId} not found during advance", Sanitize(processId));
            return NotFound(new { error = $"Process '{processId}' not found." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments for phase advance on process {ProcessId}", Sanitize(processId));
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error advancing phase for process {ProcessId}", Sanitize(processId));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred while advancing the phase." });
        }
    }

    /// <summary>
    /// Steps the process back to a prior phase, rolling back intermediate phases.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="request">The step-back request containing target phase and reason.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the step-back attempt.</returns>
    [HttpPost("{processId}/step-back")]
    [ProducesResponseType(typeof(PhaseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StepBack(
        string processId,
        [FromBody] StepBackRequest request,
        CancellationToken ct)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "Step-back request is required." });
            }

            if (string.IsNullOrWhiteSpace(request.TargetPhaseId))
            {
                return BadRequest(new { error = "TargetPhaseId is required." });
            }

            var reason = new StepBackReason
            {
                Reason = request.Reason,
                InitiatedBy = request.InitiatedBy
            };

            var result = await _phaseManager.StepBackAsync(processId, request.TargetPhaseId, reason, ct);

            _logger.LogInformation(
                "Step-back for process {ProcessId} to phase {TargetPhaseId}: success={Success}",
                Sanitize(processId), Sanitize(request.TargetPhaseId), result.Success);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Process or phase not found during step-back for process {ProcessId}", Sanitize(processId));
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("must be before"))
        {
            _logger.LogWarning(ex, "Invalid step-back target for process {ProcessId}", Sanitize(processId));
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid arguments for step-back on process {ProcessId}", Sanitize(processId));
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during step-back for process {ProcessId}", Sanitize(processId));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred during the step-back operation." });
        }
    }

    /// <summary>
    /// Retrieves the complete audit trail for a process, ordered chronologically.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ordered list of audit entries for the process.</returns>
    [HttpGet("{processId}/audit")]
    [ProducesResponseType(typeof(IReadOnlyList<PhaseAuditEntry>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditTrail(
        string processId,
        CancellationToken ct)
    {
        try
        {
            var auditTrail = await _phaseManager.GetAuditTrailAsync(processId, ct);
            return Ok(auditTrail);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Process {ProcessId} not found when retrieving audit trail", Sanitize(processId));
            return NotFound(new { error = $"Process '{processId}' not found." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid processId provided for audit trail");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving audit trail for process {ProcessId}", Sanitize(processId));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred while retrieving the audit trail." });
        }
    }

    /// <summary>
    /// Retrieves the cognitive debt assessment for a process.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cognitive debt assessment for the current phase.</returns>
    [HttpGet("{processId}/debt")]
    [ProducesResponseType(typeof(CognitiveDebtAssessment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCognitiveDebtAssessment(
        string processId,
        CancellationToken ct)
    {
        try
        {
            // Validate process exists first
            var process = await _phaseManager.GetProcessAsync(processId, ct);
            var currentPhase = process.Phases[process.CurrentPhaseIndex];

            var assessment = await _cognitiveDebtPort.AssessDebtAsync(processId, currentPhase.PhaseId, ct);
            return Ok(assessment);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Process {ProcessId} not found when retrieving cognitive debt", Sanitize(processId));
            return NotFound(new { error = $"Process '{processId}' not found." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid processId provided for debt assessment");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving cognitive debt for process {ProcessId}", Sanitize(processId));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred while assessing cognitive debt." });
        }
    }
}

/// <summary>
/// Request model for the step-back endpoint, containing the target phase and reason.
/// </summary>
public class StepBackRequest
{
    /// <summary>
    /// The identifier of the phase to step back to.
    /// </summary>
    public string TargetPhaseId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable explanation of why the step-back was initiated.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the user or system component that initiated the step-back.
    /// </summary>
    public string InitiatedBy { get; set; } = string.Empty;
}
