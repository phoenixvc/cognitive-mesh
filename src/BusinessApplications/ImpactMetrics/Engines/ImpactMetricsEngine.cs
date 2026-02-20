using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Models;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Ports;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Engines;

/// <summary>
/// Core engine that implements all Impact-Driven AI Metrics capabilities including
/// psychological safety scoring, mission alignment assessment, adoption telemetry,
/// and comprehensive impact reporting.
/// </summary>
public class ImpactMetricsEngine : IPsychologicalSafetyPort, IMissionAlignmentPort, IAdoptionTelemetryPort, IImpactAssessmentPort
{
    private readonly ILogger<ImpactMetricsEngine> _logger;

    // In-memory stores keyed by "{tenantId}:{teamId}" or "{tenantId}"
    private readonly ConcurrentDictionary<string, List<PsychologicalSafetyScore>> _safetyScores = new();
    private readonly ConcurrentDictionary<string, List<MissionAlignment>> _alignments = new();
    private readonly ConcurrentDictionary<string, List<AdoptionTelemetry>> _telemetryEvents = new();
    private readonly ConcurrentDictionary<string, List<ImpactAssessment>> _assessments = new();

    /// <summary>
    /// Weight applied to survey responses when calculating dimension scores.
    /// </summary>
    internal const double SurveyWeight = 0.7;

    /// <summary>
    /// Weight applied to behavioral signals when calculating dimension scores.
    /// </summary>
    internal const double BehavioralWeight = 0.3;

    /// <summary>
    /// Threshold of survey responses below which confidence is <see cref="ConfidenceLevel.Low"/>.
    /// </summary>
    internal const int LowConfidenceThreshold = 10;

    /// <summary>
    /// Threshold of survey responses above which confidence is <see cref="ConfidenceLevel.High"/>.
    /// </summary>
    internal const int HighConfidenceThreshold = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImpactMetricsEngine"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    public ImpactMetricsEngine(ILogger<ImpactMetricsEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // -----------------------------------------------------------------------
    // IPsychologicalSafetyPort
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public Task<PsychologicalSafetyScore> CalculateSafetyScoreAsync(
        string teamId,
        string tenantId,
        Dictionary<SafetyDimension, double> surveyScores,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(teamId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(surveyScores);

        _logger.LogInformation(
            "Calculating psychological safety score for team {TeamId} in tenant {TenantId}",
            teamId, tenantId);

        // Derive behavioral signals from telemetry for this tenant
        var behavioralScores = DeriveBehavioralScores(tenantId);

        // Calculate per-dimension scores using weighted average
        var dimensionScores = new Dictionary<SafetyDimension, double>();
        foreach (var dimension in Enum.GetValues<SafetyDimension>())
        {
            var surveyScore = surveyScores.TryGetValue(dimension, out var sv) ? sv : 50.0;
            var behavioralScore = behavioralScores.TryGetValue(dimension, out var bv) ? bv : 50.0;
            dimensionScores[dimension] = (surveyScore * SurveyWeight) + (behavioralScore * BehavioralWeight);
        }

        // Overall score is the average of all dimension scores
        var overallScore = dimensionScores.Values.Average();

        // Determine survey response count from the supplied scores
        var surveyResponseCount = surveyScores.Count;
        var behavioralSignalCount = behavioralScores.Count;

        // Determine confidence level
        var totalResponses = surveyResponseCount + behavioralSignalCount;
        var confidence = totalResponses switch
        {
            < LowConfidenceThreshold => ConfidenceLevel.Low,
            <= HighConfidenceThreshold => ConfidenceLevel.Medium,
            _ => ConfidenceLevel.High
        };

        var score = new PsychologicalSafetyScore(
            ScoreId: Guid.NewGuid().ToString(),
            TeamId: teamId,
            TenantId: tenantId,
            OverallScore: Math.Round(overallScore, 2),
            Dimensions: dimensionScores,
            SurveyResponseCount: surveyResponseCount,
            BehavioralSignalCount: behavioralSignalCount,
            CalculatedAt: DateTimeOffset.UtcNow,
            ConfidenceLevel: confidence);

        // Persist in memory
        var key = BuildTeamKey(tenantId, teamId);
        _safetyScores.AddOrUpdate(
            key,
            _ => [score],
            (_, existing) => { existing.Add(score); return existing; });

        _logger.LogInformation(
            "Psychological safety score calculated: {OverallScore} with confidence {Confidence} for team {TeamId}",
            score.OverallScore, confidence, teamId);

        return Task.FromResult(score);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PsychologicalSafetyScore>> GetHistoricalScoresAsync(
        string teamId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(teamId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var key = BuildTeamKey(tenantId, teamId);
        if (_safetyScores.TryGetValue(key, out var scores))
        {
            return Task.FromResult<IReadOnlyList<PsychologicalSafetyScore>>(
                scores.OrderBy(s => s.CalculatedAt).ToList().AsReadOnly());
        }

        return Task.FromResult<IReadOnlyList<PsychologicalSafetyScore>>(
            Array.Empty<PsychologicalSafetyScore>());
    }

    /// <inheritdoc />
    public Task<Dictionary<SafetyDimension, double>?> GetDimensionBreakdownAsync(
        string teamId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(teamId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var key = BuildTeamKey(tenantId, teamId);
        if (_safetyScores.TryGetValue(key, out var scores) && scores.Count > 0)
        {
            var latest = scores.OrderByDescending(s => s.CalculatedAt).First();
            return Task.FromResult<Dictionary<SafetyDimension, double>?>(
                new Dictionary<SafetyDimension, double>(latest.Dimensions));
        }

        return Task.FromResult<Dictionary<SafetyDimension, double>?>(null);
    }

    // -----------------------------------------------------------------------
    // IMissionAlignmentPort
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public Task<MissionAlignment> AssessAlignmentAsync(
        string decisionId,
        string decisionContext,
        string missionStatement,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(missionStatement);

        _logger.LogInformation(
            "Assessing mission alignment for decision {DecisionId}", decisionId);

        // Tokenize both the decision context and mission statement for keyword matching
        var missionKeywords = ExtractKeywords(missionStatement);
        var decisionKeywords = ExtractKeywords(decisionContext);

        // Find matches — keywords that appear in both
        var matches = missionKeywords
            .Intersect(decisionKeywords, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Find conflicts — simple heuristic: known negative prefixes in decision context
        var conflicts = DetectConflicts(decisionContext, missionStatement);

        // Score: ratio of matched keywords to total mission keywords, minus conflict penalty
        var matchRatio = missionKeywords.Count > 0
            ? (double)matches.Count / missionKeywords.Count
            : 0.0;

        var conflictPenalty = conflicts.Count * 0.15;
        var alignmentScore = Math.Max(0.0, Math.Min(1.0, matchRatio - conflictPenalty));

        // Hash the mission statement for reference
        var hash = ComputeHash(missionStatement);

        var alignment = new MissionAlignment(
            AlignmentId: Guid.NewGuid().ToString(),
            DecisionId: decisionId,
            MissionStatementHash: hash,
            AlignmentScore: Math.Round(alignmentScore, 4),
            ValueMatches: matches,
            Conflicts: conflicts,
            AssessedAt: DateTimeOffset.UtcNow);

        // Store per decision id (we use decision id as a simple tenant-like key)
        _alignments.AddOrUpdate(
            decisionId,
            _ => [alignment],
            (_, existing) => { existing.Add(alignment); return existing; });

        _logger.LogInformation(
            "Mission alignment assessed for decision {DecisionId}: score={Score}, matches={Matches}, conflicts={Conflicts}",
            decisionId, alignment.AlignmentScore, matches.Count, conflicts.Count);

        return Task.FromResult(alignment);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MissionAlignment>> GetAlignmentTrendAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        // Aggregate all alignments (in a real system, these would be filtered by tenant)
        var all = _alignments.Values
            .SelectMany(list => list)
            .OrderBy(a => a.AssessedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<MissionAlignment>>(all.AsReadOnly());
    }

    // -----------------------------------------------------------------------
    // IAdoptionTelemetryPort
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public Task RecordActionAsync(
        AdoptionTelemetry telemetry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        _logger.LogInformation(
            "Recording telemetry action {Action} for user {UserId} in tenant {TenantId}",
            telemetry.Action, telemetry.UserId, telemetry.TenantId);

        _telemetryEvents.AddOrUpdate(
            telemetry.TenantId,
            _ => [telemetry],
            (_, existing) => { existing.Add(telemetry); return existing; });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AdoptionTelemetry>> GetUsageSummaryAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        if (_telemetryEvents.TryGetValue(tenantId, out var events))
        {
            return Task.FromResult<IReadOnlyList<AdoptionTelemetry>>(
                events.OrderBy(e => e.Timestamp).ToList().AsReadOnly());
        }

        return Task.FromResult<IReadOnlyList<AdoptionTelemetry>>(
            Array.Empty<AdoptionTelemetry>());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ResistanceIndicator>> DetectResistancePatternsAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        _logger.LogInformation("Detecting resistance patterns for tenant {TenantId}", tenantId);

        var indicators = new List<ResistanceIndicator>();

        if (!_telemetryEvents.TryGetValue(tenantId, out var events) || events.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<ResistanceIndicator>>(indicators.AsReadOnly());
        }

        // Detect high override rate
        var totalActions = events.Count;
        var overrideCount = events.Count(e => e.Action == AdoptionAction.Override);
        if (totalActions > 0)
        {
            var overrideRate = (double)overrideCount / totalActions;
            if (overrideRate > 0.3)
            {
                var affectedUsers = events
                    .Where(e => e.Action == AdoptionAction.Override)
                    .Select(e => e.UserId)
                    .Distinct()
                    .Count();

                indicators.Add(new ResistanceIndicator(
                    IndicatorType: ResistanceType.Override,
                    Severity: Math.Min(1.0, overrideRate),
                    AffectedUserCount: affectedUsers,
                    FirstDetectedAt: events
                        .Where(e => e.Action == AdoptionAction.Override)
                        .Min(e => e.Timestamp),
                    Description: $"High override rate detected: {overrideRate:P0} of all actions are overrides."));
            }
        }

        // Detect help request spike
        var helpCount = events.Count(e => e.Action == AdoptionAction.HelpRequest);
        if (totalActions > 0)
        {
            var helpRate = (double)helpCount / totalActions;
            if (helpRate > 0.25)
            {
                var affectedUsers = events
                    .Where(e => e.Action == AdoptionAction.HelpRequest)
                    .Select(e => e.UserId)
                    .Distinct()
                    .Count();

                indicators.Add(new ResistanceIndicator(
                    IndicatorType: ResistanceType.HelpSpike,
                    Severity: Math.Min(1.0, helpRate),
                    AffectedUserCount: affectedUsers,
                    FirstDetectedAt: events
                        .Where(e => e.Action == AdoptionAction.HelpRequest)
                        .Min(e => e.Timestamp),
                    Description: $"Help request spike detected: {helpRate:P0} of all actions are help requests."));
            }
        }

        // Detect declining usage (feature ignore > feature use)
        var ignoreCount = events.Count(e => e.Action == AdoptionAction.FeatureIgnore);
        var useCount = events.Count(e => e.Action == AdoptionAction.FeatureUse);
        if (ignoreCount > useCount && ignoreCount > 0)
        {
            var affectedUsers = events
                .Where(e => e.Action == AdoptionAction.FeatureIgnore)
                .Select(e => e.UserId)
                .Distinct()
                .Count();

            var severity = useCount > 0
                ? Math.Min(1.0, (double)ignoreCount / (ignoreCount + useCount))
                : 1.0;

            indicators.Add(new ResistanceIndicator(
                IndicatorType: ResistanceType.Avoidance,
                Severity: severity,
                AffectedUserCount: affectedUsers,
                FirstDetectedAt: events
                    .Where(e => e.Action == AdoptionAction.FeatureIgnore)
                    .Min(e => e.Timestamp),
                Description: $"Feature avoidance detected: {ignoreCount} ignores vs {useCount} uses."));
        }

        // Detect negative feedback pattern
        var feedbackEvents = events.Where(e => e.Action == AdoptionAction.Feedback).ToList();
        var negativeFeedback = feedbackEvents
            .Where(e => e.Context != null &&
                        e.Context.Contains("negative", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (negativeFeedback.Count > 0 && feedbackEvents.Count > 0)
        {
            var negativeRate = (double)negativeFeedback.Count / feedbackEvents.Count;
            if (negativeRate > 0.5)
            {
                var affectedUsers = negativeFeedback
                    .Select(e => e.UserId)
                    .Distinct()
                    .Count();

                indicators.Add(new ResistanceIndicator(
                    IndicatorType: ResistanceType.NegativeFeedback,
                    Severity: Math.Min(1.0, negativeRate),
                    AffectedUserCount: affectedUsers,
                    FirstDetectedAt: negativeFeedback.Min(e => e.Timestamp),
                    Description: $"Negative feedback pattern detected: {negativeRate:P0} of feedback is negative."));
            }
        }

        return Task.FromResult<IReadOnlyList<ResistanceIndicator>>(indicators.AsReadOnly());
    }

    // -----------------------------------------------------------------------
    // IImpactAssessmentPort
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<ImpactAssessment> GenerateAssessmentAsync(
        string tenantId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        _logger.LogInformation(
            "Generating impact assessment for tenant {TenantId} from {PeriodStart} to {PeriodEnd}",
            tenantId, periodStart, periodEnd);

        // Get resistance patterns
        var resistanceIndicators = await DetectResistancePatternsAsync(tenantId, cancellationToken);

        // Calculate adoption rate from telemetry
        var adoptionRate = CalculateAdoptionRate(tenantId);

        // Calculate productivity and quality deltas (simulated from telemetry patterns)
        var (productivityDelta, qualityDelta, timeToDecisionDelta) = CalculateDeltas(tenantId);

        // User satisfaction from feedback telemetry
        var userSatisfactionScore = CalculateUserSatisfaction(tenantId);

        var assessment = new ImpactAssessment(
            AssessmentId: Guid.NewGuid().ToString(),
            TenantId: tenantId,
            PeriodStart: periodStart,
            PeriodEnd: periodEnd,
            ProductivityDelta: Math.Round(productivityDelta, 4),
            QualityDelta: Math.Round(qualityDelta, 4),
            TimeToDecisionDelta: Math.Round(timeToDecisionDelta, 4),
            UserSatisfactionScore: Math.Round(userSatisfactionScore, 2),
            AdoptionRate: Math.Round(adoptionRate, 4),
            ResistanceIndicators: resistanceIndicators.ToList());

        _assessments.AddOrUpdate(
            tenantId,
            _ => [assessment],
            (_, existing) => { existing.Add(assessment); return existing; });

        return assessment;
    }

    /// <inheritdoc />
    public async Task<ImpactReport> GenerateReportAsync(
        string tenantId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        _logger.LogInformation(
            "Generating impact report for tenant {TenantId} from {PeriodStart} to {PeriodEnd}",
            tenantId, periodStart, periodEnd);

        // Generate or retrieve the latest assessment
        var assessment = await GenerateAssessmentAsync(tenantId, periodStart, periodEnd, cancellationToken);

        // Get the latest safety score across all teams for this tenant
        var safetyScore = GetLatestSafetyScoreForTenant(tenantId);

        // Get the latest alignment score
        var alignmentScore = GetLatestAlignmentScore();

        // Calculate overall impact score using weighted formula:
        // safety * 0.3 + alignment * 0.2 + adoption * 0.3 + productivity * 0.2
        var normalizedProductivity = Math.Max(0, Math.Min(100, (assessment.ProductivityDelta + 1) * 50));
        var overallImpactScore =
            (safetyScore * 0.3) +
            (alignmentScore * 100.0 * 0.2) +
            (assessment.AdoptionRate * 100.0 * 0.3) +
            (normalizedProductivity * 0.2);

        overallImpactScore = Math.Round(Math.Max(0, Math.Min(100, overallImpactScore)), 2);

        // Generate recommendations based on weak areas
        var recommendations = GenerateRecommendations(
            safetyScore, alignmentScore, assessment.AdoptionRate,
            assessment.ProductivityDelta, assessment.ResistanceIndicators);

        var report = new ImpactReport(
            ReportId: Guid.NewGuid().ToString(),
            TenantId: tenantId,
            PeriodStart: periodStart,
            PeriodEnd: periodEnd,
            SafetyScore: Math.Round(safetyScore, 2),
            AlignmentScore: Math.Round(alignmentScore, 4),
            AdoptionRate: Math.Round(assessment.AdoptionRate, 4),
            OverallImpactScore: overallImpactScore,
            Recommendations: recommendations,
            GeneratedAt: DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "Impact report generated for tenant {TenantId}: overall score={OverallImpactScore}",
            tenantId, report.OverallImpactScore);

        return report;
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private static string BuildTeamKey(string tenantId, string teamId) => $"{tenantId}:{teamId}";

    private Dictionary<SafetyDimension, double> DeriveBehavioralScores(string tenantId)
    {
        var scores = new Dictionary<SafetyDimension, double>();

        if (!_telemetryEvents.TryGetValue(tenantId, out var events) || events.Count == 0)
        {
            return scores;
        }

        var totalActions = events.Count;
        var overrideRate = (double)events.Count(e => e.Action == AdoptionAction.Override) / totalActions;
        var helpRate = (double)events.Count(e => e.Action == AdoptionAction.HelpRequest) / totalActions;
        var featureUseRate = (double)events.Count(e => e.Action == AdoptionAction.FeatureUse) / totalActions;
        var ignoreRate = (double)events.Count(e => e.Action == AdoptionAction.FeatureIgnore) / totalActions;

        // Higher feature use -> higher trust, lower override -> higher trust
        scores[SafetyDimension.TrustInAI] = Math.Max(0, Math.Min(100, (1 - overrideRate) * 100));
        scores[SafetyDimension.FearOfReplacement] = Math.Max(0, Math.Min(100, (1 - ignoreRate) * 100));
        scores[SafetyDimension.ComfortWithAutomation] = Math.Max(0, Math.Min(100, featureUseRate * 100));
        scores[SafetyDimension.WillingnessToExperiment] = Math.Max(0, Math.Min(100, featureUseRate * 100));
        scores[SafetyDimension.TransparencyPerception] = Math.Max(0, Math.Min(100, (1 - helpRate) * 100));
        scores[SafetyDimension.ErrorTolerance] = Math.Max(0, Math.Min(100, (1 - overrideRate) * 100));

        return scores;
    }

    private static List<string> ExtractKeywords(string text)
    {
        // Simple keyword extraction: split on whitespace and punctuation, filter short words
        var separators = new[] { ' ', ',', '.', ';', ':', '!', '?', '\n', '\r', '\t', '(', ')', '[', ']', '{', '}', '"', '\'' };
        return text
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Select(w => w.ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    private static List<string> DetectConflicts(string decisionContext, string missionStatement)
    {
        var conflicts = new List<string>();

        // Simple conflict detection: look for negation patterns near mission keywords
        var negationPrefixes = new[] { "not ", "never ", "against ", "despite ", "ignoring ", "violating ", "contradicting " };
        var missionKeywords = ExtractKeywords(missionStatement);
        var lowerDecision = decisionContext.ToLowerInvariant();

        foreach (var keyword in missionKeywords)
        {
            foreach (var prefix in negationPrefixes)
            {
                if (lowerDecision.Contains($"{prefix}{keyword}", StringComparison.OrdinalIgnoreCase))
                {
                    conflicts.Add($"Potential conflict: '{prefix}{keyword}' found in decision context");
                }
            }
        }

        return conflicts;
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16];
    }

    private double CalculateAdoptionRate(string tenantId)
    {
        if (!_telemetryEvents.TryGetValue(tenantId, out var events) || events.Count == 0)
        {
            return 0.0;
        }

        var totalActions = events.Count;
        var activeUse = events.Count(e =>
            e.Action == AdoptionAction.FeatureUse ||
            e.Action == AdoptionAction.WorkflowComplete ||
            e.Action == AdoptionAction.Login);

        return totalActions > 0 ? (double)activeUse / totalActions : 0.0;
    }

    private (double productivity, double quality, double timeToDecision) CalculateDeltas(string tenantId)
    {
        if (!_telemetryEvents.TryGetValue(tenantId, out var events) || events.Count == 0)
        {
            return (0.0, 0.0, 0.0);
        }

        var completedWorkflows = events.Count(e => e.Action == AdoptionAction.WorkflowComplete);
        var totalActions = events.Count;

        // Productivity correlates with workflow completion rate
        var productivityDelta = totalActions > 0
            ? Math.Min(1.0, (double)completedWorkflows / totalActions * 2 - 0.5)
            : 0.0;

        // Quality inversely correlates with override rate
        var overrideRate = (double)events.Count(e => e.Action == AdoptionAction.Override) / totalActions;
        var qualityDelta = 0.5 - overrideRate;

        // Time-to-decision improves with higher feature use (negative = faster = better)
        var featureUseRate = (double)events.Count(e => e.Action == AdoptionAction.FeatureUse) / totalActions;
        var timeToDecisionDelta = -(featureUseRate * 0.5);

        return (productivityDelta, qualityDelta, timeToDecisionDelta);
    }

    private double CalculateUserSatisfaction(string tenantId)
    {
        if (!_telemetryEvents.TryGetValue(tenantId, out var events) || events.Count == 0)
        {
            return 50.0; // Neutral baseline
        }

        var feedbackEvents = events.Where(e => e.Action == AdoptionAction.Feedback).ToList();
        if (feedbackEvents.Count == 0)
        {
            return 50.0;
        }

        var positive = feedbackEvents.Count(e =>
            e.Context != null && e.Context.Contains("positive", StringComparison.OrdinalIgnoreCase));
        var negative = feedbackEvents.Count(e =>
            e.Context != null && e.Context.Contains("negative", StringComparison.OrdinalIgnoreCase));
        var total = feedbackEvents.Count;

        return total > 0 ? ((double)positive / total) * 100.0 : 50.0;
    }

    private double GetLatestSafetyScoreForTenant(string tenantId)
    {
        var tenantScores = _safetyScores
            .Where(kvp => kvp.Key.StartsWith($"{tenantId}:", StringComparison.Ordinal))
            .SelectMany(kvp => kvp.Value)
            .ToList();

        if (tenantScores.Count == 0)
        {
            return 50.0; // Neutral baseline when no scores exist
        }

        return tenantScores
            .OrderByDescending(s => s.CalculatedAt)
            .First()
            .OverallScore;
    }

    private double GetLatestAlignmentScore()
    {
        var allAlignments = _alignments.Values.SelectMany(a => a).ToList();
        if (allAlignments.Count == 0)
        {
            return 0.5; // Neutral baseline
        }

        return allAlignments
            .OrderByDescending(a => a.AssessedAt)
            .First()
            .AlignmentScore;
    }

    private static List<string> GenerateRecommendations(
        double safetyScore,
        double alignmentScore,
        double adoptionRate,
        double productivityDelta,
        List<ResistanceIndicator> resistanceIndicators)
    {
        var recommendations = new List<string>();

        if (safetyScore < 60)
        {
            recommendations.Add("Psychological safety score is below target. Consider team workshops on AI collaboration and transparent communication about AI's role.");
        }

        if (safetyScore < 80)
        {
            recommendations.Add("Safety score has room for improvement. Encourage experimentation with AI tools in low-risk scenarios.");
        }

        if (alignmentScore < 0.5)
        {
            recommendations.Add("Mission alignment is low. Review AI decision-making processes to ensure they reflect organizational values.");
        }

        if (adoptionRate < 0.3)
        {
            recommendations.Add("Adoption rate is below expectations. Consider targeted training sessions and identifying AI champions within teams.");
        }

        if (productivityDelta < 0)
        {
            recommendations.Add("Productivity has declined. Investigate whether AI tools are creating friction in existing workflows.");
        }

        foreach (var indicator in resistanceIndicators)
        {
            switch (indicator.IndicatorType)
            {
                case ResistanceType.Override:
                    recommendations.Add($"High override rate affecting {indicator.AffectedUserCount} users. Review AI recommendation quality and calibration.");
                    break;
                case ResistanceType.HelpSpike:
                    recommendations.Add($"Help request spike affecting {indicator.AffectedUserCount} users. Improve onboarding materials and in-app guidance.");
                    break;
                case ResistanceType.Avoidance:
                    recommendations.Add($"Feature avoidance detected for {indicator.AffectedUserCount} users. Gather qualitative feedback on barriers to adoption.");
                    break;
                case ResistanceType.NegativeFeedback:
                    recommendations.Add($"Negative feedback pattern from {indicator.AffectedUserCount} users. Conduct user interviews to understand pain points.");
                    break;
                case ResistanceType.Disengagement:
                    recommendations.Add($"User disengagement detected for {indicator.AffectedUserCount} users. Consider re-engagement campaigns and value demonstrations.");
                    break;
            }
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("All metrics are within healthy ranges. Continue monitoring and consider expanding AI capabilities.");
        }

        return recommendations;
    }
}
