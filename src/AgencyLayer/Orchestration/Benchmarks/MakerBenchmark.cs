using System.Diagnostics;
using AgencyLayer.Orchestration.Checkpointing;
using AgencyLayer.Orchestration.Execution;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.Orchestration.Benchmarks;

/// <summary>
/// MAKER Benchmark: Measures a system's ability to execute long-horizon
/// sequential tasks. Uses Tower of Hanoi as the canonical test — research
/// shows LLMs fail after a few hundred sequential steps.
///
/// Gas Town claims 10-disc (1,023 steps) in minutes, 20-disc (~1M steps, ~30h).
/// This benchmark measures Cognitive Mesh's ability to match or exceed that.
/// </summary>
public class MakerBenchmark
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ICheckpointManager _checkpointManager;
    private readonly ILogger<MakerBenchmark> _logger;

    public MakerBenchmark(
        IWorkflowEngine workflowEngine,
        ICheckpointManager checkpointManager,
        ILogger<MakerBenchmark> logger)
    {
        _workflowEngine = workflowEngine ?? throw new ArgumentNullException(nameof(workflowEngine));
        _checkpointManager = checkpointManager ?? throw new ArgumentNullException(nameof(checkpointManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the Tower of Hanoi MAKER benchmark for a given number of discs.
    /// N discs = 2^N - 1 sequential steps.
    /// </summary>
    public async Task<MakerScoreReport> RunTowerOfHanoiAsync(int numDiscs, CancellationToken cancellationToken = default)
    {
        if (numDiscs < 1 || numDiscs > 25)
            throw new ArgumentOutOfRangeException(nameof(numDiscs), "Must be between 1 and 25");

        int totalSteps = (1 << numDiscs) - 1; // 2^N - 1
        _logger.LogInformation(
            "Starting MAKER benchmark: Tower of Hanoi with {NumDiscs} discs ({TotalSteps} steps)",
            numDiscs, totalSteps);

        // Generate the deterministic move sequence
        var moves = GenerateHanoiMoves(numDiscs);

        // Build a workflow with one step per Hanoi move
        var workflow = BuildHanoiWorkflow(numDiscs, moves);

        var stopwatch = Stopwatch.StartNew();
        var result = await _workflowEngine.ExecuteWorkflowAsync(workflow, cancellationToken);
        stopwatch.Stop();

        var checkpoints = await _checkpointManager.GetWorkflowCheckpointsAsync(workflow.WorkflowId, cancellationToken);
        var checkpointList = checkpoints.ToList();

        var report = new MakerScoreReport
        {
            BenchmarkName = $"TowerOfHanoi-{numDiscs}",
            NumDiscs = numDiscs,
            TotalStepsRequired = totalSteps,
            StepsCompleted = result.CompletedSteps,
            StepsFailed = result.FailedSteps,
            Success = result.Success,
            TotalDuration = stopwatch.Elapsed,
            AverageStepDuration = result.CompletedSteps > 0
                ? TimeSpan.FromTicks(stopwatch.Elapsed.Ticks / result.CompletedSteps)
                : TimeSpan.Zero,
            CheckpointsCreated = checkpointList.Count,
            MakerScore = CalculateMakerScore(totalSteps, result.CompletedSteps, result.Success, stopwatch.Elapsed),
            WorkflowId = workflow.WorkflowId,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation(
            "MAKER benchmark complete: {BenchmarkName}, Score={MakerScore:F1}, Steps={StepsCompleted}/{TotalSteps}, Duration={Duration}",
            report.BenchmarkName, report.MakerScore, report.StepsCompleted, report.TotalStepsRequired, report.TotalDuration);

        return report;
    }

    /// <summary>
    /// Runs a progressive MAKER benchmark: starts with 1 disc and increases
    /// until failure, returning the maximum successful disc count.
    /// </summary>
    public async Task<MakerProgressiveReport> RunProgressiveBenchmarkAsync(
        int maxDiscs = 15,
        CancellationToken cancellationToken = default)
    {
        var results = new List<MakerScoreReport>();
        int maxSuccessfulDiscs = 0;

        for (int discs = 1; discs <= maxDiscs; discs++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Progressive MAKER: Testing {Discs} discs...", discs);
            var report = await RunTowerOfHanoiAsync(discs, cancellationToken);
            results.Add(report);

            if (report.Success)
            {
                maxSuccessfulDiscs = discs;
            }
            else
            {
                _logger.LogInformation("Progressive MAKER: Failed at {Discs} discs. Max successful: {MaxDiscs}", discs, maxSuccessfulDiscs);
                break;
            }
        }

        return new MakerProgressiveReport
        {
            MaxDiscsAttempted = results.Count,
            MaxDiscsCompleted = maxSuccessfulDiscs,
            MaxStepsCompleted = maxSuccessfulDiscs > 0 ? (1 << maxSuccessfulDiscs) - 1 : 0,
            Results = results,
            OverallMakerScore = CalculateOverallMakerScore(results),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates the optimal move sequence for N-disc Tower of Hanoi.
    /// Uses the iterative algorithm (not recursive) for memory efficiency.
    /// </summary>
    internal static List<HanoiMove> GenerateHanoiMoves(int numDiscs)
    {
        int totalMoves = (1 << numDiscs) - 1;
        var moves = new List<HanoiMove>(totalMoves);

        // Iterative Tower of Hanoi
        // For odd number of discs: cycle is A→C, A→B, B→C
        // For even number of discs: cycle is A→B, A→C, B→C
        char[] pegs = numDiscs % 2 == 0
            ? new[] { 'A', 'B', 'C' }
            : new[] { 'A', 'C', 'B' };

        var pegStacks = new Dictionary<char, Stack<int>>
        {
            ['A'] = new Stack<int>(Enumerable.Range(1, numDiscs).Reverse()),
            ['B'] = new Stack<int>(),
            ['C'] = new Stack<int>()
        };

        for (int move = 1; move <= totalMoves; move++)
        {
            char from, to;

            if (move % 3 == 1)
            {
                (from, to) = GetLegalMove(pegStacks, pegs[0], pegs[2]);
            }
            else if (move % 3 == 2)
            {
                (from, to) = GetLegalMove(pegStacks, pegs[0], pegs[1]);
            }
            else
            {
                (from, to) = GetLegalMove(pegStacks, pegs[1], pegs[2]);
            }

            int disc = pegStacks[from].Pop();
            pegStacks[to].Push(disc);

            moves.Add(new HanoiMove
            {
                MoveNumber = move,
                Disc = disc,
                From = from,
                To = to
            });
        }

        return moves;
    }

    private static (char from, char to) GetLegalMove(Dictionary<char, Stack<int>> pegs, char peg1, char peg2)
    {
        bool p1Empty = pegs[peg1].Count == 0;
        bool p2Empty = pegs[peg2].Count == 0;

        if (p1Empty) return (peg2, peg1);
        if (p2Empty) return (peg1, peg2);

        return pegs[peg1].Peek() < pegs[peg2].Peek()
            ? (peg1, peg2)
            : (peg2, peg1);
    }

    private WorkflowDefinition BuildHanoiWorkflow(int numDiscs, List<HanoiMove> moves)
    {
        var workflow = new WorkflowDefinition
        {
            Name = $"TowerOfHanoi-{numDiscs}",
            Description = $"Tower of Hanoi with {numDiscs} discs ({moves.Count} moves)",
            IsPreApproved = true, // Deterministic — governance hot path
            MaxRetryPerStep = 1,
            StepTimeout = TimeSpan.FromSeconds(30),
            InitialContext = new Dictionary<string, object>
            {
                ["NumDiscs"] = numDiscs,
                ["PegA"] = Enumerable.Range(1, numDiscs).ToList(),
                ["PegB"] = new List<int>(),
                ["PegC"] = new List<int>()
            }
        };

        for (int i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            workflow.Steps.Add(new WorkflowStepDefinition
            {
                StepNumber = i,
                Name = $"Move disc {move.Disc}: {move.From}→{move.To}",
                Description = $"Move disc {move.Disc} from peg {move.From} to peg {move.To}",
                RequiresGovernanceCheck = false, // Pre-approved deterministic step
                ExecuteFunc = CreateHanoiStepFunc(move)
            });
        }

        return workflow;
    }

    private static Func<WorkflowStepContext, CancellationToken, Task<WorkflowStepResult>> CreateHanoiStepFunc(HanoiMove move)
    {
        return (context, ct) =>
        {
            // Validate and execute the move against the state
            var pegAKey = $"Peg{move.From}";
            var pegBKey = $"Peg{move.To}";

            var fromPeg = GetPegFromState(context.State, pegAKey);
            var toPeg = GetPegFromState(context.State, pegBKey);

            if (fromPeg.Count == 0)
            {
                return Task.FromResult(new WorkflowStepResult
                {
                    Success = false,
                    ErrorMessage = $"Move {move.MoveNumber}: Peg {move.From} is empty"
                });
            }

            int disc = fromPeg[0]; // Top disc (smallest index = top)
            if (disc != move.Disc)
            {
                return Task.FromResult(new WorkflowStepResult
                {
                    Success = false,
                    ErrorMessage = $"Move {move.MoveNumber}: Expected disc {move.Disc} on top of peg {move.From}, found disc {disc}"
                });
            }

            if (toPeg.Count > 0 && toPeg[0] < disc)
            {
                return Task.FromResult(new WorkflowStepResult
                {
                    Success = false,
                    ErrorMessage = $"Move {move.MoveNumber}: Cannot place disc {disc} on top of smaller disc {toPeg[0]}"
                });
            }

            // Execute the move
            fromPeg.RemoveAt(0);
            toPeg.Insert(0, disc);

            return Task.FromResult(new WorkflowStepResult
            {
                Success = true,
                Output = new { Move = move.MoveNumber, Disc = disc, From = move.From, To = move.To },
                StateUpdates = new Dictionary<string, object>
                {
                    [pegAKey] = fromPeg,
                    [pegBKey] = toPeg
                }
            });
        };
    }

    private static List<int> GetPegFromState(Dictionary<string, object> state, string key)
    {
        if (!state.TryGetValue(key, out var pegObj))
            return new List<int>();

        if (pegObj is List<int> list) return list;
        if (pegObj is System.Text.Json.JsonElement jsonElement)
        {
            return jsonElement.EnumerateArray().Select(e => e.GetInt32()).ToList();
        }

        return new List<int>();
    }

    private static double CalculateMakerScore(int totalSteps, int completedSteps, bool success, TimeSpan duration)
    {
        if (totalSteps == 0) return 0;

        double completionRatio = (double)completedSteps / totalSteps;
        double baseScore = completionRatio * 100;

        // Bonus for full completion
        if (success) baseScore = Math.Max(baseScore, 100);

        // Scale by log of total steps (harder problems get more credit)
        double complexityMultiplier = Math.Log2(totalSteps + 1);
        double finalScore = baseScore * complexityMultiplier / 10.0;

        return Math.Round(finalScore, 1);
    }

    private static double CalculateOverallMakerScore(List<MakerScoreReport> results)
    {
        if (!results.Any()) return 0;
        var lastSuccess = results.LastOrDefault(r => r.Success);
        if (lastSuccess == null) return 0;
        return lastSuccess.MakerScore;
    }
}

public class HanoiMove
{
    public int MoveNumber { get; set; }
    public int Disc { get; set; }
    public char From { get; set; }
    public char To { get; set; }
}

public class MakerScoreReport
{
    public string BenchmarkName { get; set; } = string.Empty;
    public int NumDiscs { get; set; }
    public int TotalStepsRequired { get; set; }
    public int StepsCompleted { get; set; }
    public int StepsFailed { get; set; }
    public bool Success { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageStepDuration { get; set; }
    public int CheckpointsCreated { get; set; }
    public double MakerScore { get; set; }
    public string WorkflowId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class MakerProgressiveReport
{
    public int MaxDiscsAttempted { get; set; }
    public int MaxDiscsCompleted { get; set; }
    public int MaxStepsCompleted { get; set; }
    public double OverallMakerScore { get; set; }
    public List<MakerScoreReport> Results { get; set; } = new();
    public DateTime Timestamp { get; set; }

    public string GetSummary()
    {
        return $"""
            MAKER Progressive Benchmark Report
            ===================================
            Max Discs Completed: {MaxDiscsCompleted}/{MaxDiscsAttempted}
            Max Steps Completed: {MaxStepsCompleted:N0}
            Overall MAKER Score: {OverallMakerScore:F1}

            Disc-by-Disc Results:
            {string.Join("\n", Results.Select(r =>
                $"  {r.NumDiscs} discs ({r.TotalStepsRequired:N0} steps): " +
                $"{(r.Success ? "PASS" : "FAIL")} in {r.TotalDuration.TotalSeconds:F2}s " +
                $"(score: {r.MakerScore:F1})"))}
            """;
    }
}
