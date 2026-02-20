using System.Collections.Concurrent;
using AgencyLayer.CognitiveSandwich.Models;
using AgencyLayer.CognitiveSandwich.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.CognitiveSandwich.Engines;

/// <summary>
/// Core engine implementing the Cognitive Sandwich workflow pattern.
/// Manages phase-based processes with pre/postcondition validation,
/// step-back capability, cognitive debt monitoring, and full audit trails.
/// </summary>
public class CognitiveSandwichEngine : IPhaseManagerPort
{
    private readonly IPhaseConditionPort _conditionPort;
    private readonly ICognitiveDebtPort _cognitiveDebtPort;
    private readonly IAuditLoggingAdapter _auditLoggingAdapter;
    private readonly ILogger<CognitiveSandwichEngine> _logger;

    private readonly ConcurrentDictionary<string, SandwichProcess> _processes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CognitiveSandwichEngine"/> class.
    /// </summary>
    /// <param name="conditionPort">Port for evaluating phase pre/postconditions.</param>
    /// <param name="cognitiveDebtPort">Port for assessing cognitive debt.</param>
    /// <param name="auditLoggingAdapter">Adapter for persisting audit trail entries.</param>
    /// <param name="logger">Logger instance.</param>
    public CognitiveSandwichEngine(
        IPhaseConditionPort conditionPort,
        ICognitiveDebtPort cognitiveDebtPort,
        IAuditLoggingAdapter auditLoggingAdapter,
        ILogger<CognitiveSandwichEngine> logger)
    {
        _conditionPort = conditionPort ?? throw new ArgumentNullException(nameof(conditionPort));
        _cognitiveDebtPort = cognitiveDebtPort ?? throw new ArgumentNullException(nameof(cognitiveDebtPort));
        _auditLoggingAdapter = auditLoggingAdapter ?? throw new ArgumentNullException(nameof(auditLoggingAdapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SandwichProcess> CreateProcessAsync(SandwichProcessConfig config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        ValidateConfig(config);

        var phases = config.Phases
            .OrderBy(p => p.Order)
            .Select(pd => new Phase
            {
                PhaseId = Guid.NewGuid().ToString(),
                Name = pd.Name,
                Description = pd.Description,
                Order = pd.Order,
                Preconditions = pd.Preconditions,
                Postconditions = pd.Postconditions,
                RequiresHumanValidation = pd.RequiresHumanValidation,
                Status = PhaseStatus.Pending
            })
            .ToList();

        var process = new SandwichProcess
        {
            ProcessId = Guid.NewGuid().ToString(),
            TenantId = config.TenantId,
            Name = config.Name,
            CreatedAt = DateTime.UtcNow,
            CurrentPhaseIndex = 0,
            Phases = phases,
            State = SandwichProcessState.Created,
            MaxStepBacks = config.MaxStepBacks,
            StepBackCount = 0,
            CognitiveDebtThreshold = config.CognitiveDebtThreshold
        };

        _processes[process.ProcessId] = process;

        var auditEntry = new PhaseAuditEntry
        {
            ProcessId = process.ProcessId,
            PhaseId = phases[0].PhaseId,
            EventType = PhaseAuditEventType.ProcessCreated,
            UserId = "system",
            Details = $"Process '{config.Name}' created with {phases.Count} phases, max step-backs: {config.MaxStepBacks}, cognitive debt threshold: {config.CognitiveDebtThreshold}"
        };
        await _auditLoggingAdapter.LogAuditEntryAsync(auditEntry, ct);

        _logger.LogInformation(
            "Created Cognitive Sandwich process {ProcessId} '{ProcessName}' with {PhaseCount} phases",
            process.ProcessId, process.Name, phases.Count);

        return process;
    }

    /// <inheritdoc />
    public Task<SandwichProcess> GetProcessAsync(string processId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);

        if (!_processes.TryGetValue(processId, out var process))
        {
            throw new InvalidOperationException($"Process '{processId}' not found.");
        }

        return Task.FromResult(process);
    }

    /// <inheritdoc />
    public async Task<PhaseResult> TransitionToNextPhaseAsync(string processId, PhaseTransitionContext context, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);
        ArgumentNullException.ThrowIfNull(context);

        var process = await GetProcessAsync(processId, ct);

        if (process.State == SandwichProcessState.Completed)
        {
            return CreateBlockedResult(process, "Process is already completed.");
        }

        if (process.State == SandwichProcessState.Failed)
        {
            return CreateBlockedResult(process, "Process is in a failed state.");
        }

        var currentPhase = process.Phases[process.CurrentPhaseIndex];

        // Log the transition attempt
        var transitionStartEntry = new PhaseAuditEntry
        {
            ProcessId = processId,
            PhaseId = currentPhase.PhaseId,
            EventType = PhaseAuditEventType.PhaseTransitionStarted,
            UserId = context.UserId,
            Details = $"Transition from phase '{currentPhase.Name}' (index {process.CurrentPhaseIndex}) requested. Reason: {context.TransitionReason}"
        };
        await _auditLoggingAdapter.LogAuditEntryAsync(transitionStartEntry, ct);

        // Check postconditions of current phase
        if (context.PhaseOutput != null)
        {
            var postconditionResult = await _conditionPort.CheckPostconditionsAsync(processId, currentPhase.PhaseId, context.PhaseOutput, ct);
            if (!postconditionResult.AllMet)
            {
                var failedConditions = postconditionResult.Results
                    .Where(r => !r.Met)
                    .Select(r => $"{r.ConditionId}: {r.Reason}")
                    .ToList();

                var blockedEntry = new PhaseAuditEntry
                {
                    ProcessId = processId,
                    PhaseId = currentPhase.PhaseId,
                    EventType = PhaseAuditEventType.PhaseTransitionBlocked,
                    UserId = context.UserId,
                    Details = $"Postconditions not met: {string.Join("; ", failedConditions)}"
                };
                await _auditLoggingAdapter.LogAuditEntryAsync(blockedEntry, ct);

                _logger.LogWarning(
                    "Transition blocked for process {ProcessId} at phase {PhaseId}: postconditions not met",
                    processId, currentPhase.PhaseId);

                return new PhaseResult
                {
                    Success = false,
                    PhaseId = currentPhase.PhaseId,
                    NextPhaseId = null,
                    ValidationErrors = failedConditions,
                    AuditEntry = blockedEntry
                };
            }
        }

        // Check cognitive debt
        var debtBreached = await _cognitiveDebtPort.IsThresholdBreachedAsync(processId, ct);
        if (debtBreached)
        {
            var debtAssessment = await _cognitiveDebtPort.AssessDebtAsync(processId, currentPhase.PhaseId, ct);

            var debtEntry = new PhaseAuditEntry
            {
                ProcessId = processId,
                PhaseId = currentPhase.PhaseId,
                EventType = PhaseAuditEventType.CognitiveDebtBreached,
                UserId = context.UserId,
                Details = $"Cognitive debt threshold breached (score: {debtAssessment.DebtScore:F1}). Recommendations: {string.Join("; ", debtAssessment.Recommendations)}"
            };
            await _auditLoggingAdapter.LogAuditEntryAsync(debtEntry, ct);

            _logger.LogWarning(
                "Transition blocked for process {ProcessId}: cognitive debt threshold breached (score: {DebtScore})",
                processId, debtAssessment.DebtScore);

            return new PhaseResult
            {
                Success = false,
                PhaseId = currentPhase.PhaseId,
                NextPhaseId = null,
                ValidationErrors = [$"Cognitive debt threshold breached (score: {debtAssessment.DebtScore:F1}). Human review required."],
                AuditEntry = debtEntry
            };
        }

        // Mark current phase as completed
        var mutableCurrentPhase = (Phase)currentPhase;
        mutableCurrentPhase.Status = PhaseStatus.Completed;

        // Check if this was the last phase
        if (process.CurrentPhaseIndex >= process.Phases.Count - 1)
        {
            process.State = SandwichProcessState.Completed;

            var completedEntry = new PhaseAuditEntry
            {
                ProcessId = processId,
                PhaseId = currentPhase.PhaseId,
                EventType = PhaseAuditEventType.ProcessCompleted,
                UserId = context.UserId,
                Details = $"All {process.Phases.Count} phases completed successfully."
            };
            await _auditLoggingAdapter.LogAuditEntryAsync(completedEntry, ct);

            _logger.LogInformation("Process {ProcessId} completed all phases successfully", processId);

            return new PhaseResult
            {
                Success = true,
                PhaseId = currentPhase.PhaseId,
                NextPhaseId = null,
                ValidationErrors = [],
                AuditEntry = completedEntry
            };
        }

        // Advance to next phase
        var nextPhaseIndex = process.CurrentPhaseIndex + 1;
        var nextPhase = process.Phases[nextPhaseIndex];

        // Check preconditions of the next phase
        var preconditionResult = await _conditionPort.CheckPreconditionsAsync(processId, nextPhase.PhaseId, ct);
        if (!preconditionResult.AllMet)
        {
            var failedPreconditions = preconditionResult.Results
                .Where(r => !r.Met)
                .Select(r => $"{r.ConditionId}: {r.Reason}")
                .ToList();

            var precondBlockedEntry = new PhaseAuditEntry
            {
                ProcessId = processId,
                PhaseId = nextPhase.PhaseId,
                EventType = PhaseAuditEventType.PhaseTransitionBlocked,
                UserId = context.UserId,
                Details = $"Preconditions for next phase '{nextPhase.Name}' not met: {string.Join("; ", failedPreconditions)}"
            };
            await _auditLoggingAdapter.LogAuditEntryAsync(precondBlockedEntry, ct);

            // Roll back the current phase status since we can't actually advance
            mutableCurrentPhase.Status = PhaseStatus.InProgress;

            _logger.LogWarning(
                "Transition blocked for process {ProcessId}: preconditions for phase {NextPhaseId} not met",
                processId, nextPhase.PhaseId);

            return new PhaseResult
            {
                Success = false,
                PhaseId = currentPhase.PhaseId,
                NextPhaseId = nextPhase.PhaseId,
                ValidationErrors = failedPreconditions,
                AuditEntry = precondBlockedEntry
            };
        }

        // Perform the transition
        process.CurrentPhaseIndex = nextPhaseIndex;
        var mutableNextPhase = (Phase)nextPhase;
        mutableNextPhase.Status = PhaseStatus.InProgress;
        process.State = SandwichProcessState.InProgress;

        // Check if next phase requires human validation
        if (nextPhase.RequiresHumanValidation)
        {
            mutableNextPhase.Status = PhaseStatus.AwaitingReview;
            process.State = SandwichProcessState.AwaitingHumanReview;

            var humanEntry = new PhaseAuditEntry
            {
                ProcessId = processId,
                PhaseId = nextPhase.PhaseId,
                EventType = PhaseAuditEventType.HumanValidationRequested,
                UserId = context.UserId,
                Details = $"Phase '{nextPhase.Name}' requires human validation before proceeding."
            };
            await _auditLoggingAdapter.LogAuditEntryAsync(humanEntry, ct);

            _logger.LogInformation(
                "Process {ProcessId} awaiting human review at phase {PhaseId} '{PhaseName}'",
                processId, nextPhase.PhaseId, nextPhase.Name);
        }

        var transitionEntry = new PhaseAuditEntry
        {
            ProcessId = processId,
            PhaseId = nextPhase.PhaseId,
            EventType = PhaseAuditEventType.PhaseTransitionCompleted,
            UserId = context.UserId,
            Details = $"Transitioned from phase '{currentPhase.Name}' (index {process.CurrentPhaseIndex - 1}) to phase '{nextPhase.Name}' (index {process.CurrentPhaseIndex})."
        };
        await _auditLoggingAdapter.LogAuditEntryAsync(transitionEntry, ct);

        _logger.LogInformation(
            "Process {ProcessId} transitioned to phase {PhaseIndex} '{PhaseName}'",
            processId, process.CurrentPhaseIndex, nextPhase.Name);

        return new PhaseResult
        {
            Success = true,
            PhaseId = currentPhase.PhaseId,
            NextPhaseId = nextPhase.PhaseId,
            ValidationErrors = [],
            AuditEntry = transitionEntry
        };
    }

    /// <inheritdoc />
    public async Task<PhaseResult> StepBackAsync(string processId, string targetPhaseId, StepBackReason reason, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPhaseId);
        ArgumentNullException.ThrowIfNull(reason);

        var process = await GetProcessAsync(processId, ct);

        // Validate step-back count
        if (process.StepBackCount >= process.MaxStepBacks)
        {
            var blockedEntry = new PhaseAuditEntry
            {
                ProcessId = processId,
                PhaseId = targetPhaseId,
                EventType = PhaseAuditEventType.PhaseTransitionBlocked,
                UserId = reason.InitiatedBy,
                Details = $"Step-back blocked: maximum step-backs ({process.MaxStepBacks}) exceeded. Current count: {process.StepBackCount}."
            };
            await _auditLoggingAdapter.LogAuditEntryAsync(blockedEntry, ct);

            _logger.LogWarning(
                "Step-back blocked for process {ProcessId}: max step-backs ({MaxStepBacks}) exceeded",
                processId, process.MaxStepBacks);

            return new PhaseResult
            {
                Success = false,
                PhaseId = process.Phases[process.CurrentPhaseIndex].PhaseId,
                NextPhaseId = targetPhaseId,
                ValidationErrors = [$"Maximum step-back count ({process.MaxStepBacks}) has been reached."],
                AuditEntry = blockedEntry
            };
        }

        // Find target phase
        var targetPhaseIndex = -1;
        for (int i = 0; i < process.Phases.Count; i++)
        {
            if (process.Phases[i].PhaseId == targetPhaseId)
            {
                targetPhaseIndex = i;
                break;
            }
        }

        if (targetPhaseIndex < 0)
        {
            throw new InvalidOperationException($"Target phase '{targetPhaseId}' not found in process '{processId}'.");
        }

        if (targetPhaseIndex >= process.CurrentPhaseIndex)
        {
            throw new InvalidOperationException(
                $"Step-back target phase (index {targetPhaseIndex}) must be before the current phase (index {process.CurrentPhaseIndex}).");
        }

        // Roll back all phases between current and target (inclusive of current)
        for (int i = process.CurrentPhaseIndex; i > targetPhaseIndex; i--)
        {
            var phaseToRollBack = (Phase)process.Phases[i];
            phaseToRollBack.Status = PhaseStatus.RolledBack;
        }

        // Set target phase back to InProgress
        var targetPhase = (Phase)process.Phases[targetPhaseIndex];
        targetPhase.Status = PhaseStatus.InProgress;

        process.CurrentPhaseIndex = targetPhaseIndex;
        process.StepBackCount++;
        process.State = SandwichProcessState.SteppedBack;

        var auditEntry = new PhaseAuditEntry
        {
            ProcessId = processId,
            PhaseId = targetPhaseId,
            EventType = PhaseAuditEventType.StepBackPerformed,
            UserId = reason.InitiatedBy,
            Details = $"Stepped back to phase '{targetPhase.Name}' (index {targetPhaseIndex}). Reason: {reason.Reason}. Step-back count: {process.StepBackCount}/{process.MaxStepBacks}."
        };
        await _auditLoggingAdapter.LogAuditEntryAsync(auditEntry, ct);

        _logger.LogInformation(
            "Process {ProcessId} stepped back to phase {TargetPhaseIndex} '{TargetPhaseName}' (step-back {StepBackCount}/{MaxStepBacks}). Reason: {Reason}",
            processId, targetPhaseIndex, targetPhase.Name, process.StepBackCount, process.MaxStepBacks, reason.Reason);

        return new PhaseResult
        {
            Success = true,
            PhaseId = process.Phases[process.CurrentPhaseIndex].PhaseId,
            NextPhaseId = targetPhaseId,
            ValidationErrors = [],
            AuditEntry = auditEntry
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PhaseAuditEntry>> GetAuditTrailAsync(string processId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);

        // Validate process exists
        await GetProcessAsync(processId, ct);

        return await _auditLoggingAdapter.GetAuditEntriesAsync(processId, ct);
    }

    private static void ValidateConfig(SandwichProcessConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.TenantId))
        {
            throw new ArgumentException("TenantId is required.", nameof(config));
        }

        if (string.IsNullOrWhiteSpace(config.Name))
        {
            throw new ArgumentException("Process name is required.", nameof(config));
        }

        if (config.Phases.Count < 3 || config.Phases.Count > 7)
        {
            throw new ArgumentException(
                $"Process must have between 3 and 7 phases, but {config.Phases.Count} were provided.",
                nameof(config));
        }

        if (config.MaxStepBacks < 0)
        {
            throw new ArgumentException("MaxStepBacks must be non-negative.", nameof(config));
        }

        if (config.CognitiveDebtThreshold is < 0 or > 100)
        {
            throw new ArgumentException("CognitiveDebtThreshold must be between 0 and 100.", nameof(config));
        }
    }

    private static PhaseResult CreateBlockedResult(SandwichProcess process, string reason)
    {
        return new PhaseResult
        {
            Success = false,
            PhaseId = process.Phases[process.CurrentPhaseIndex].PhaseId,
            NextPhaseId = null,
            ValidationErrors = [reason]
        };
    }
}
