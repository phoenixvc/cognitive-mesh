using System.Collections.Concurrent;
using CognitiveMesh.BusinessApplications.NISTCompliance.Models;
using CognitiveMesh.BusinessApplications.NISTCompliance.Ports;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.NISTCompliance.Services;

/// <summary>
/// In-memory implementation of the NIST AI RMF compliance service,
/// providing evidence management, checklist tracking, scoring, reviews,
/// roadmap generation, and audit logging capabilities.
/// </summary>
public class NISTComplianceService : INISTComplianceServicePort
{
    private readonly ILogger<NISTComplianceService> _logger;
    private readonly ConcurrentDictionary<Guid, EvidenceRecord> _evidence = new();
    private readonly ConcurrentDictionary<string, List<NISTAuditEntry>> _auditLogs = new();

    private static readonly List<(string PillarId, string PillarName, List<(string Id, string Description)> Statements)> DefaultPillars =
    [
        ("GOVERN", "Govern", [
            ("GOV-1", "Policies and procedures are in place to govern AI risk management."),
            ("GOV-2", "Accountability structures are defined for AI systems."),
            ("GOV-3", "Organizational AI risk tolerances are established and documented.")
        ]),
        ("MAP", "Map", [
            ("MAP-1", "AI system context and intended purpose are clearly defined."),
            ("MAP-2", "Interdependencies and potential impacts are identified."),
            ("MAP-3", "Risks related to third-party AI components are assessed.")
        ]),
        ("MEASURE", "Measure", [
            ("MEASURE-1", "Metrics for AI system performance and trustworthiness are established."),
            ("MEASURE-2", "AI system outputs are evaluated for bias and fairness."),
            ("MEASURE-3", "Continuous monitoring processes are in place for AI systems.")
        ]),
        ("MANAGE", "Manage", [
            ("MANAGE-1", "Risk response strategies are defined and implemented."),
            ("MANAGE-2", "AI system risks are prioritized and addressed."),
            ("MANAGE-3", "Processes for decommissioning AI systems are documented.")
        ])
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="NISTComplianceService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for structured logging.</param>
    public NISTComplianceService(ILogger<NISTComplianceService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<NISTEvidenceResponse> SubmitEvidenceAsync(NISTEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var evidenceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var record = new EvidenceRecord
        {
            EvidenceId = evidenceId,
            StatementId = request.StatementId,
            ArtifactType = request.ArtifactType,
            Content = request.Content,
            SubmittedBy = request.SubmittedBy,
            Tags = request.Tags,
            FileSizeBytes = request.FileSizeBytes,
            SubmittedAt = now,
            ReviewStatus = "Pending"
        };

        _evidence[evidenceId] = record;

        AddAuditEntry("default-org", new NISTAuditEntry
        {
            EntryId = Guid.NewGuid(),
            Action = "EvidenceSubmitted",
            PerformedBy = request.SubmittedBy,
            PerformedAt = now,
            Details = $"Evidence '{evidenceId}' submitted for statement '{request.StatementId}' with artifact type '{request.ArtifactType}'."
        });

        _logger.LogInformation(
            "Evidence {EvidenceId} submitted for statement {StatementId} by {SubmittedBy}",
            evidenceId, request.StatementId, request.SubmittedBy);

        return Task.FromResult(new NISTEvidenceResponse
        {
            EvidenceId = evidenceId,
            StatementId = request.StatementId,
            SubmittedAt = now,
            ReviewStatus = "Pending",
            Message = "Evidence submitted successfully and is pending review."
        });
    }

    /// <inheritdoc />
    public Task<NISTChecklistResponse> GetChecklistAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        cancellationToken.ThrowIfCancellationRequested();

        var pillars = new List<NISTChecklistPillar>();
        var totalStatements = 0;
        var completedStatements = 0;

        foreach (var (pillarId, pillarName, statements) in DefaultPillars)
        {
            var checklistStatements = new List<NISTChecklistStatement>();

            foreach (var (stmtId, description) in statements)
            {
                var evidenceCount = _evidence.Values.Count(e => e.StatementId == stmtId);
                var approvedCount = _evidence.Values.Count(e => e.StatementId == stmtId && e.ReviewStatus == "Approved");
                var isComplete = approvedCount > 0;
                int? currentScore = approvedCount > 0 ? Math.Min(approvedCount + 1, 5) : null;

                checklistStatements.Add(new NISTChecklistStatement
                {
                    StatementId = stmtId,
                    Description = description,
                    IsComplete = isComplete,
                    EvidenceCount = evidenceCount,
                    CurrentScore = currentScore
                });

                totalStatements++;
                if (isComplete)
                {
                    completedStatements++;
                }
            }

            pillars.Add(new NISTChecklistPillar
            {
                PillarId = pillarId,
                PillarName = pillarName,
                Statements = checklistStatements
            });
        }

        _logger.LogInformation(
            "Checklist retrieved for organization {OrganizationId}: {Completed}/{Total} statements completed",
            organizationId, completedStatements, totalStatements);

        return Task.FromResult(new NISTChecklistResponse
        {
            OrganizationId = organizationId,
            Pillars = pillars,
            TotalStatements = totalStatements,
            CompletedStatements = completedStatements
        });
    }

    /// <inheritdoc />
    public Task<NISTScoreResponse> GetScoreAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        cancellationToken.ThrowIfCancellationRequested();

        var pillarScores = new List<NISTChecklistPillarScore>();
        var overallTotal = 0.0;
        var overallCount = 0;

        foreach (var (pillarId, pillarName, statements) in DefaultPillars)
        {
            var pillarTotal = 0.0;

            foreach (var (stmtId, _) in statements)
            {
                var approvedCount = _evidence.Values.Count(e => e.StatementId == stmtId && e.ReviewStatus == "Approved");
                var score = approvedCount > 0 ? Math.Min(approvedCount + 1, 5) : 0;
                pillarTotal += score;
            }

            var averageScore = statements.Count > 0 ? pillarTotal / statements.Count : 0.0;
            overallTotal += pillarTotal;
            overallCount += statements.Count;

            pillarScores.Add(new NISTChecklistPillarScore
            {
                PillarId = pillarId,
                PillarName = pillarName,
                AverageScore = Math.Round(averageScore, 2),
                StatementCount = statements.Count
            });
        }

        var overall = overallCount > 0 ? Math.Round(overallTotal / overallCount, 2) : 0.0;

        _logger.LogInformation(
            "Score calculated for organization {OrganizationId}: overall {OverallScore}",
            organizationId, overall);

        return Task.FromResult(new NISTScoreResponse
        {
            OrganizationId = organizationId,
            OverallScore = overall,
            PillarScores = pillarScores,
            AssessedAt = DateTimeOffset.UtcNow
        });
    }

    /// <inheritdoc />
    public Task<NISTReviewResponse> SubmitReviewAsync(NISTReviewRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        if (!_evidence.TryGetValue(request.EvidenceId, out var record))
        {
            throw new KeyNotFoundException($"Evidence with ID '{request.EvidenceId}' was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        record.ReviewStatus = request.Decision;
        record.ReviewedBy = request.ReviewerId;
        record.ReviewedAt = now;
        record.ReviewNotes = request.Notes;

        AddAuditEntry("default-org", new NISTAuditEntry
        {
            EntryId = Guid.NewGuid(),
            Action = "ReviewCompleted",
            PerformedBy = request.ReviewerId,
            PerformedAt = now,
            Details = $"Evidence '{request.EvidenceId}' reviewed with decision '{request.Decision}'."
                    + (request.Notes != null ? $" Notes: {request.Notes}" : string.Empty)
        });

        _logger.LogInformation(
            "Review completed for evidence {EvidenceId}: {Decision} by {ReviewerId}",
            request.EvidenceId, request.Decision, request.ReviewerId);

        return Task.FromResult(new NISTReviewResponse
        {
            EvidenceId = request.EvidenceId,
            NewStatus = request.Decision,
            ReviewedAt = now,
            Message = $"Evidence review completed with decision '{request.Decision}'."
        });
    }

    /// <inheritdoc />
    public Task<NISTRoadmapResponse> GetRoadmapAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        cancellationToken.ThrowIfCancellationRequested();

        var gaps = new List<NISTGapItem>();
        const int targetScore = 4;

        foreach (var (_, _, statements) in DefaultPillars)
        {
            foreach (var (stmtId, _) in statements)
            {
                var approvedCount = _evidence.Values.Count(e => e.StatementId == stmtId && e.ReviewStatus == "Approved");
                var currentScore = approvedCount > 0 ? Math.Min(approvedCount + 1, 5) : 0;

                if (currentScore < targetScore)
                {
                    var priority = currentScore switch
                    {
                        0 => "Critical",
                        1 => "High",
                        2 => "Medium",
                        _ => "Low"
                    };

                    gaps.Add(new NISTGapItem
                    {
                        StatementId = stmtId,
                        CurrentScore = currentScore,
                        TargetScore = targetScore,
                        Priority = priority,
                        RecommendedActions = GenerateRecommendedActions(stmtId, currentScore)
                    });
                }
            }
        }

        _logger.LogInformation(
            "Roadmap generated for organization {OrganizationId}: {GapCount} gaps identified",
            organizationId, gaps.Count);

        return Task.FromResult(new NISTRoadmapResponse
        {
            OrganizationId = organizationId,
            Gaps = gaps,
            GeneratedAt = DateTimeOffset.UtcNow
        });
    }

    /// <inheritdoc />
    public Task<NISTAuditLogResponse> GetAuditLogAsync(string organizationId, int maxResults, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);

        cancellationToken.ThrowIfCancellationRequested();

        var entries = _auditLogs.GetOrAdd(organizationId, _ => new List<NISTAuditEntry>());

        List<NISTAuditEntry> snapshot;
        lock (entries)
        {
            snapshot = entries.OrderByDescending(e => e.PerformedAt).Take(maxResults).ToList();
        }

        _logger.LogInformation(
            "Audit log retrieved for organization {OrganizationId}: {EntryCount} entries (max {MaxResults})",
            organizationId, snapshot.Count, maxResults);

        return Task.FromResult(new NISTAuditLogResponse
        {
            OrganizationId = organizationId,
            Entries = snapshot,
            TotalCount = entries.Count
        });
    }

    private void AddAuditEntry(string organizationId, NISTAuditEntry entry)
    {
        var entries = _auditLogs.GetOrAdd(organizationId, _ => new List<NISTAuditEntry>());
        lock (entries)
        {
            entries.Add(entry);
        }
    }

    private static List<string> GenerateRecommendedActions(string statementId, int currentScore)
    {
        var actions = new List<string>();

        if (currentScore == 0)
        {
            actions.Add($"Initiate assessment for statement {statementId}.");
            actions.Add("Assign an owner to gather initial evidence.");
            actions.Add("Schedule a kickoff meeting with relevant stakeholders.");
        }
        else if (currentScore <= 2)
        {
            actions.Add($"Collect additional evidence for statement {statementId}.");
            actions.Add("Review existing documentation for completeness.");
            actions.Add("Engage subject matter experts for gap analysis.");
        }
        else
        {
            actions.Add($"Refine existing evidence for statement {statementId}.");
            actions.Add("Conduct peer review of submitted artifacts.");
        }

        return actions;
    }

    /// <summary>
    /// Internal record for tracking evidence submissions and their review status.
    /// </summary>
    internal class EvidenceRecord
    {
        /// <summary>The unique identifier of the evidence.</summary>
        public Guid EvidenceId { get; set; }

        /// <summary>The NIST statement this evidence is for.</summary>
        public string StatementId { get; set; } = string.Empty;

        /// <summary>The artifact type.</summary>
        public string ArtifactType { get; set; } = string.Empty;

        /// <summary>The evidence content.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Who submitted the evidence.</summary>
        public string SubmittedBy { get; set; } = string.Empty;

        /// <summary>Optional tags.</summary>
        public List<string>? Tags { get; set; }

        /// <summary>File size in bytes.</summary>
        public long FileSizeBytes { get; set; }

        /// <summary>When the evidence was submitted.</summary>
        public DateTimeOffset SubmittedAt { get; set; }

        /// <summary>Current review status.</summary>
        public string ReviewStatus { get; set; } = "Pending";

        /// <summary>Who reviewed the evidence.</summary>
        public string? ReviewedBy { get; set; }

        /// <summary>When the evidence was reviewed.</summary>
        public DateTimeOffset? ReviewedAt { get; set; }

        /// <summary>Review notes.</summary>
        public string? ReviewNotes { get; set; }
    }
}
