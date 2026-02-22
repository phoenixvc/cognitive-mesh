using System.Collections.Concurrent;
using System.Diagnostics;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Ports;

namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Engines;

/// <summary>
/// Engine implementing reflexion-based hallucination and contradiction detection.
/// Evaluates agent outputs against input prompts using keyword analysis,
/// length ratio checks, and known hallucination pattern detection.
/// </summary>
public sealed class ReflexionEngine : IReflexionPort
{
    private readonly ILogger<ReflexionEngine> _logger;
    private readonly ConcurrentBag<ReflexionResult> _results = [];

    /// <summary>
    /// Known phrases that indicate fabricated citations or impossible claims.
    /// </summary>
    private static readonly string[] HallucinationPatterns =
    [
        "according to the study published in",
        "research from 2099",
        "a well-known fact that",
        "it is universally accepted",
        "as everyone knows",
        "studies have conclusively proven",
        "100% guaranteed",
        "absolutely certain",
        "no possibility of",
        "impossible to fail"
    ];

    /// <summary>
    /// Minimum output-to-input length ratio below which the output is considered suspiciously short.
    /// </summary>
    public const double SuspiciousLengthRatio = 0.1;

    /// <summary>
    /// Confidence threshold above which an output is classified as a hallucination.
    /// </summary>
    public const double HallucinationThreshold = 0.6;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflexionEngine"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public ReflexionEngine(ILogger<ReflexionEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<ReflexionResult> EvaluateAsync(
        string inputText, string agentOutput, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(inputText))
        {
            throw new ArgumentException("Input text is required.", nameof(inputText));
        }

        if (string.IsNullOrWhiteSpace(agentOutput))
        {
            throw new ArgumentException("Agent output is required.", nameof(agentOutput));
        }

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Evaluating agent output ({OutputLength} chars) against input ({InputLength} chars).",
            agentOutput.Length, inputText.Length);

        var contradictions = DetectContradictions(inputText, agentOutput);
        var hallucinationScore = DetectHallucinationPatterns(agentOutput);
        var lengthRatioScore = EvaluateLengthRatio(inputText, agentOutput);

        var confidence = CalculateConfidence(contradictions.Count, hallucinationScore, lengthRatioScore);
        var isHallucination = confidence >= HallucinationThreshold;

        stopwatch.Stop();

        var result = new ReflexionResult(
            ResultId: Guid.NewGuid(),
            InputText: inputText,
            IsHallucination: isHallucination,
            Confidence: Math.Round(confidence, 3),
            Contradictions: contradictions,
            EvaluationDurationMs: stopwatch.ElapsedMilliseconds,
            EvaluatedAt: DateTimeOffset.UtcNow);

        _results.Add(result);

        _logger.LogInformation(
            "Evaluation complete: IsHallucination={IsHallucination}, Confidence={Confidence}, Contradictions={Count}.",
            isHallucination, result.Confidence, contradictions.Count);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ReflexionResult>> GetRecentResultsAsync(
        int count, CancellationToken cancellationToken)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");
        }

        _logger.LogInformation("Retrieving {Count} most recent reflexion results.", count);

        var recent = _results
            .OrderByDescending(r => r.EvaluatedAt)
            .Take(count)
            .ToList();

        return Task.FromResult<IReadOnlyList<ReflexionResult>>(recent.AsReadOnly());
    }

    /// <summary>
    /// Detects contradictions between the input text and agent output using keyword analysis.
    /// Looks for negation patterns and conflicting claims.
    /// </summary>
    private static List<string> DetectContradictions(string inputText, string agentOutput)
    {
        var contradictions = new List<string>();
        var inputLower = inputText.ToLowerInvariant();
        var outputLower = agentOutput.ToLowerInvariant();

        var negationPairs = new (string positive, string negative)[]
        {
            ("is", "is not"),
            ("can", "cannot"),
            ("will", "will not"),
            ("should", "should not"),
            ("must", "must not"),
            ("always", "never"),
            ("true", "false"),
            ("possible", "impossible"),
            ("increase", "decrease"),
            ("success", "failure")
        };

        foreach (var (positive, negative) in negationPairs)
        {
            if (inputLower.Contains(positive) && outputLower.Contains(negative))
            {
                contradictions.Add(
                    $"Input contains '{positive}' but output contains '{negative}', suggesting a contradiction.");
            }
            else if (inputLower.Contains(negative) && outputLower.Contains(positive))
            {
                contradictions.Add(
                    $"Input contains '{negative}' but output contains '{positive}', suggesting a contradiction.");
            }
        }

        return contradictions;
    }

    /// <summary>
    /// Detects known hallucination patterns in the agent output.
    /// Returns a score from 0.0 (no patterns found) to 1.0 (many patterns found).
    /// </summary>
    private static double DetectHallucinationPatterns(string agentOutput)
    {
        var outputLower = agentOutput.ToLowerInvariant();
        var matchCount = HallucinationPatterns.Count(pattern => outputLower.Contains(pattern));

        return Math.Min(1.0, matchCount * 0.4);
    }

    /// <summary>
    /// Evaluates the length ratio between output and input.
    /// Very short outputs for complex inputs may indicate hallucination.
    /// </summary>
    private static double EvaluateLengthRatio(string inputText, string agentOutput)
    {
        if (inputText.Length == 0) return 0.0;

        var ratio = (double)agentOutput.Length / inputText.Length;

        if (ratio < SuspiciousLengthRatio)
        {
            return 0.5;
        }

        return 0.0;
    }

    /// <summary>
    /// Calculates the overall confidence that the output is a hallucination.
    /// </summary>
    private static double CalculateConfidence(
        int contradictionCount, double hallucinationScore, double lengthRatioScore)
    {
        var contradictionScore = Math.Min(1.0, contradictionCount * 0.2);

        var weightedScore =
            (contradictionScore * 0.2) +
            (hallucinationScore * 0.65) +
            (lengthRatioScore * 0.15);

        return Math.Min(1.0, weightedScore);
    }
}
