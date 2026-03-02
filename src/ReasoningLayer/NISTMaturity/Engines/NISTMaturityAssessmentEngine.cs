using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.NISTMaturity.Models;
using CognitiveMesh.ReasoningLayer.NISTMaturity.Ports;

namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Engines;

/// <summary>
/// Engine implementing the NIST AI RMF maturity assessment logic.
/// Scores organizational maturity across the four NIST pillars (Govern, Map, Measure, Manage)
/// based on submitted evidence, and generates improvement roadmaps for identified gaps.
/// </summary>
public sealed class NISTMaturityAssessmentEngine : INISTMaturityAssessmentPort
{
    private readonly ILogger<NISTMaturityAssessmentEngine> _logger;
    private readonly INISTEvidenceStorePort _evidenceStore;
    private readonly ConcurrentDictionary<string, List<MaturityScore>> _statementScores = new();

    /// <summary>
    /// Maximum allowed file size for evidence artifacts (10 MB).
    /// </summary>
    public const long MaxFileSizeBytes = 10_485_760;

    /// <summary>
    /// The target maturity score used for roadmap gap analysis.
    /// </summary>
    public const int TargetScore = 4;

    /// <summary>
    /// Mapping from pillar identifiers to human-readable pillar names.
    /// </summary>
    private static readonly Dictionary<string, string> PillarNames = new()
    {
        ["GOV"] = "Govern",
        ["MAP"] = "Map",
        ["MEA"] = "Measure",
        ["MAN"] = "Manage"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="NISTMaturityAssessmentEngine"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured logging.</param>
    /// <param name="evidenceStore">Port for persisting and retrieving evidence artifacts.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is <c>null</c>.</exception>
    public NISTMaturityAssessmentEngine(
        ILogger<NISTMaturityAssessmentEngine> logger,
        INISTEvidenceStorePort evidenceStore)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _evidenceStore = evidenceStore ?? throw new ArgumentNullException(nameof(evidenceStore));
    }

    /// <inheritdoc />
    public async Task<NISTEvidence> SubmitEvidenceAsync(NISTEvidence evidence, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        if (string.IsNullOrWhiteSpace(evidence.StatementId))
        {
            throw new ArgumentException("StatementId is required.", nameof(evidence));
        }

        if (string.IsNullOrWhiteSpace(evidence.ArtifactType))
        {
            throw new ArgumentException("ArtifactType is required.", nameof(evidence));
        }

        if (string.IsNullOrWhiteSpace(evidence.Content))
        {
            throw new ArgumentException("Content is required.", nameof(evidence));
        }

        if (string.IsNullOrWhiteSpace(evidence.SubmittedBy))
        {
            throw new ArgumentException("SubmittedBy is required.", nameof(evidence));
        }

        if (evidence.FileSizeBytes > MaxFileSizeBytes)
        {
            throw new ArgumentException(
                $"File size {evidence.FileSizeBytes} bytes exceeds maximum allowed size of {MaxFileSizeBytes} bytes.",
                nameof(evidence));
        }

        if (evidence.FileSizeBytes < 0)
        {
            throw new ArgumentException("File size cannot be negative.", nameof(evidence));
        }

        _logger.LogInformation(
            "Submitting evidence {EvidenceId} for statement {StatementId} by {SubmittedBy}.",
            evidence.EvidenceId, evidence.StatementId, evidence.SubmittedBy);

        await _evidenceStore.StoreEvidenceAsync(evidence, cancellationToken).ConfigureAwait(false);

        return evidence;
    }

    /// <inheritdoc />
    public async Task<MaturityScore> ScoreStatementAsync(string statementId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(statementId))
        {
            throw new ArgumentException("StatementId is required.", nameof(statementId));
        }

        _logger.LogInformation("Scoring statement {StatementId}.", statementId);

        var evidenceList = await _evidenceStore.GetEvidenceForStatementAsync(statementId, cancellationToken)
            .ConfigureAwait(false);

        var evidenceCount = evidenceList.Count;
        var (score, rationale) = CalculateScore(evidenceCount);

        var maturityScore = new MaturityScore(
            StatementId: statementId,
            Score: score,
            Rationale: rationale,
            ScoredAt: DateTimeOffset.UtcNow,
            ScoredBy: "NISTMaturityAssessmentEngine");

        _statementScores.AddOrUpdate(
            statementId,
            _ => [maturityScore],
            (_, existing) =>
            {
                existing.Add(maturityScore);
                return existing;
            });

        _logger.LogInformation(
            "Statement {StatementId} scored {Score}/5 based on {EvidenceCount} evidence items.",
            statementId, score, evidenceCount);

        return maturityScore;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PillarScore>> GetPillarScoresAsync(
        string organizationId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
        {
            throw new ArgumentException("OrganizationId is required.", nameof(organizationId));
        }

        _logger.LogInformation("Getting pillar scores for organization {OrganizationId}.", organizationId);

        var pillarScores = new List<PillarScore>();

        foreach (var (pillarId, pillarName) in PillarNames)
        {
            var pillarStatements = _statementScores
                .Where(kvp => kvp.Key.StartsWith(pillarId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (pillarStatements.Count == 0)
            {
                pillarScores.Add(new PillarScore(
                    PillarId: pillarId,
                    PillarName: pillarName,
                    AverageScore: 0.0,
                    StatementCount: 0,
                    EvidenceCount: 0,
                    LastUpdated: DateTimeOffset.UtcNow));
                continue;
            }

            var latestScores = pillarStatements
                .Select(kvp => kvp.Value.Last())
                .ToList();

            var averageScore = latestScores.Average(s => s.Score);

            var totalEvidence = 0;
            foreach (var kvp in pillarStatements)
            {
                var evidence = await _evidenceStore.GetEvidenceForStatementAsync(kvp.Key, cancellationToken)
                    .ConfigureAwait(false);
                totalEvidence += evidence.Count;
            }

            var lastUpdated = latestScores.Max(s => s.ScoredAt);

            pillarScores.Add(new PillarScore(
                PillarId: pillarId,
                PillarName: pillarName,
                AverageScore: Math.Round(averageScore, 2),
                StatementCount: pillarStatements.Count,
                EvidenceCount: totalEvidence,
                LastUpdated: lastUpdated));
        }

        return pillarScores.AsReadOnly();
    }

    /// <inheritdoc />
    public Task<ImprovementRoadmap> GenerateRoadmapAsync(
        string organizationId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
        {
            throw new ArgumentException("OrganizationId is required.", nameof(organizationId));
        }

        _logger.LogInformation("Generating improvement roadmap for organization {OrganizationId}.", organizationId);

        var gaps = new List<MaturityGap>();

        foreach (var (statementId, scores) in _statementScores)
        {
            var latestScore = scores.Last();

            if (latestScore.Score < TargetScore)
            {
                var priority = latestScore.Score switch
                {
                    1 => GapPriority.Critical,
                    2 => GapPriority.High,
                    3 => GapPriority.Medium,
                    _ => GapPriority.Low
                };

                var actions = GenerateRecommendedActions(statementId, latestScore.Score);

                gaps.Add(new MaturityGap(
                    StatementId: statementId,
                    CurrentScore: latestScore.Score,
                    TargetScore: TargetScore,
                    Priority: priority,
                    RecommendedActions: actions));
            }
        }

        var roadmap = new ImprovementRoadmap(
            RoadmapId: Guid.NewGuid(),
            OrganizationId: organizationId,
            Gaps: gaps.OrderBy(g => g.Priority).ToList(),
            GeneratedAt: DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "Generated roadmap {RoadmapId} with {GapCount} gaps for organization {OrganizationId}.",
            roadmap.RoadmapId, gaps.Count, organizationId);

        return Task.FromResult(roadmap);
    }

    /// <inheritdoc />
    public Task<MaturityAssessment> GetAssessmentAsync(
        string organizationId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
        {
            throw new ArgumentException("OrganizationId is required.", nameof(organizationId));
        }

        _logger.LogInformation("Compiling assessment for organization {OrganizationId}.", organizationId);

        var allScores = _statementScores
            .Select(kvp => kvp.Value.Last())
            .ToList();

        var overallScore = allScores.Count > 0
            ? Math.Round(allScores.Average(s => s.Score), 2)
            : 0.0;

        var pillarScores = new List<PillarScore>();
        foreach (var (pillarId, pillarName) in PillarNames)
        {
            var pillarStatementScores = allScores
                .Where(s => s.StatementId.StartsWith(pillarId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var avgScore = pillarStatementScores.Count > 0
                ? Math.Round(pillarStatementScores.Average(s => s.Score), 2)
                : 0.0;

            pillarScores.Add(new PillarScore(
                PillarId: pillarId,
                PillarName: pillarName,
                AverageScore: avgScore,
                StatementCount: pillarStatementScores.Count,
                EvidenceCount: 0,
                LastUpdated: pillarStatementScores.Count > 0
                    ? pillarStatementScores.Max(s => s.ScoredAt)
                    : DateTimeOffset.UtcNow));
        }

        var assessment = new MaturityAssessment
        {
            AssessmentId = Guid.NewGuid(),
            OrganizationId = organizationId,
            PillarScores = pillarScores,
            OverallScore = overallScore,
            AssessedAt = DateTimeOffset.UtcNow,
            StatementScores = allScores
        };

        return Task.FromResult(assessment);
    }

    /// <summary>
    /// Calculates the maturity score based on evidence count.
    /// </summary>
    private static (int Score, string Rationale) CalculateScore(int evidenceCount)
    {
        return evidenceCount switch
        {
            0 => (1, "No evidence submitted. Initial maturity level assigned."),
            1 or 2 => (2, $"Limited evidence ({evidenceCount} items). Basic maturity level with room for improvement."),
            3 or 4 => (3, $"Moderate evidence ({evidenceCount} items). Defined processes are in place."),
            >= 5 and <= 7 => (4, $"Strong evidence ({evidenceCount} items). Managed and measurable processes demonstrated."),
            _ => (5, $"Comprehensive evidence ({evidenceCount} items). Optimized maturity level achieved.")
        };
    }

    /// <summary>
    /// Generates recommended actions for closing a maturity gap.
    /// </summary>
    private static List<string> GenerateRecommendedActions(string statementId, int currentScore)
    {
        var actions = new List<string>();

        var pillarId = statementId.Split('-')[0].ToUpperInvariant();

        switch (currentScore)
        {
            case 1:
                actions.Add($"Establish initial governance documentation for {statementId}.");
                actions.Add($"Assign ownership for {statementId} compliance.");
                actions.Add("Conduct baseline assessment and define initial processes.");
                break;
            case 2:
                actions.Add($"Expand evidence collection for {statementId} with additional artifacts.");
                actions.Add("Document existing processes and identify gaps in coverage.");
                break;
            case 3:
                actions.Add($"Formalize and standardize processes for {statementId}.");
                actions.Add("Implement continuous monitoring and metrics collection.");
                break;
        }

        switch (pillarId)
        {
            case "GOV":
                actions.Add("Review and update AI governance policies and accountability structures.");
                break;
            case "MAP":
                actions.Add("Enhance AI system context documentation and risk identification processes.");
                break;
            case "MEA":
                actions.Add("Strengthen AI risk measurement capabilities and monitoring frameworks.");
                break;
            case "MAN":
                actions.Add("Improve AI risk treatment, response, and recovery procedures.");
                break;
        }

        return actions;
    }
}
