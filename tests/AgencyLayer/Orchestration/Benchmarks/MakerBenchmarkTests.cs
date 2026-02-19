using AgencyLayer.Orchestration.Benchmarks;
using AgencyLayer.Orchestration.Checkpointing;
using AgencyLayer.Orchestration.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.AgencyLayer.Orchestration.Benchmarks;

public class MakerBenchmarkTests
{
    private readonly MakerBenchmark _sut;
    private readonly InMemoryCheckpointManager _checkpointManager;

    public MakerBenchmarkTests()
    {
        var cpLogger = new Mock<ILogger<InMemoryCheckpointManager>>();
        var wfLogger = new Mock<ILogger<DurableWorkflowEngine>>();
        var bmLogger = new Mock<ILogger<MakerBenchmark>>();

        _checkpointManager = new InMemoryCheckpointManager(cpLogger.Object);
        var workflowEngine = new DurableWorkflowEngine(_checkpointManager, wfLogger.Object);
        _sut = new MakerBenchmark(workflowEngine, _checkpointManager, bmLogger.Object);
    }

    [Fact]
    public void GenerateHanoiMoves_1Disc_Returns1Move()
    {
        var moves = MakerBenchmark.GenerateHanoiMoves(1);
        moves.Should().HaveCount(1);
        moves[0].Disc.Should().Be(1);
        moves[0].From.Should().Be('A');
        moves[0].To.Should().Be('C');
    }

    [Fact]
    public void GenerateHanoiMoves_3Discs_Returns7Moves()
    {
        var moves = MakerBenchmark.GenerateHanoiMoves(3);
        moves.Should().HaveCount(7); // 2^3 - 1
    }

    [Fact]
    public void GenerateHanoiMoves_ValidMovesOnly()
    {
        // Verify no move places a larger disc on a smaller disc
        var moves = MakerBenchmark.GenerateHanoiMoves(4);
        var pegs = new Dictionary<char, Stack<int>>
        {
            ['A'] = new Stack<int>(new[] { 4, 3, 2, 1 }),
            ['B'] = new Stack<int>(),
            ['C'] = new Stack<int>()
        };

        foreach (var move in moves)
        {
            int disc = pegs[move.From].Pop();
            disc.Should().Be(move.Disc, $"Expected disc {move.Disc} on top of peg {move.From} at move {move.MoveNumber}");

            if (pegs[move.To].Count > 0)
            {
                pegs[move.To].Peek().Should().BeGreaterThan(disc,
                    $"Move {move.MoveNumber}: Cannot place disc {disc} on smaller disc {pegs[move.To].Peek()}");
            }

            pegs[move.To].Push(disc);
        }

        // All discs should be on peg C
        pegs['A'].Should().BeEmpty();
        pegs['B'].Should().BeEmpty();
        pegs['C'].Should().HaveCount(4);
    }

    [Theory]
    [InlineData(1, 1)]    // 2^1 - 1 = 1
    [InlineData(2, 3)]    // 2^2 - 1 = 3
    [InlineData(3, 7)]    // 2^3 - 1 = 7
    [InlineData(4, 15)]   // 2^4 - 1 = 15
    [InlineData(5, 31)]   // 2^5 - 1 = 31
    [InlineData(10, 1023)] // 2^10 - 1 = 1023
    public void GenerateHanoiMoves_CorrectCount(int discs, int expectedMoves)
    {
        var moves = MakerBenchmark.GenerateHanoiMoves(discs);
        moves.Should().HaveCount(expectedMoves);
    }

    [Fact]
    public async Task RunTowerOfHanoi_1Disc_Succeeds()
    {
        var report = await _sut.RunTowerOfHanoiAsync(1);

        report.Success.Should().BeTrue();
        report.TotalStepsRequired.Should().Be(1);
        report.StepsCompleted.Should().Be(1);
        report.MakerScore.Should().BeGreaterThan(0);
        report.NumDiscs.Should().Be(1);
    }

    [Fact]
    public async Task RunTowerOfHanoi_3Discs_Succeeds()
    {
        var report = await _sut.RunTowerOfHanoiAsync(3);

        report.Success.Should().BeTrue();
        report.TotalStepsRequired.Should().Be(7);
        report.StepsCompleted.Should().Be(7);
        report.StepsFailed.Should().Be(0);
        report.CheckpointsCreated.Should().Be(7);
    }

    [Fact]
    public async Task RunTowerOfHanoi_5Discs_Succeeds()
    {
        var report = await _sut.RunTowerOfHanoiAsync(5);

        report.Success.Should().BeTrue();
        report.TotalStepsRequired.Should().Be(31);
        report.StepsCompleted.Should().Be(31);
    }

    [Fact]
    public async Task RunTowerOfHanoi_10Discs_Succeeds_1023Steps()
    {
        // This is the Gas Town benchmark: 10-disc Hanoi = 1,023 steps
        var report = await _sut.RunTowerOfHanoiAsync(10);

        report.Success.Should().BeTrue();
        report.TotalStepsRequired.Should().Be(1023);
        report.StepsCompleted.Should().Be(1023);
        report.StepsFailed.Should().Be(0);
        report.CheckpointsCreated.Should().Be(1023);
        report.MakerScore.Should().BeGreaterThan(0);
        report.BenchmarkName.Should().Be("TowerOfHanoi-10");
    }

    [Fact]
    public async Task RunTowerOfHanoi_InvalidDiscs_ThrowsArgumentOutOfRange()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.RunTowerOfHanoiAsync(0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.RunTowerOfHanoiAsync(26));
    }

    [Fact]
    public async Task RunProgressiveBenchmark_CompletesMultipleDiscs()
    {
        var report = await _sut.RunProgressiveBenchmarkAsync(maxDiscs: 5);

        report.MaxDiscsCompleted.Should().Be(5);
        report.MaxStepsCompleted.Should().Be(31); // 2^5 - 1
        report.Results.Should().HaveCount(5);
        report.Results.All(r => r.Success).Should().BeTrue();
        report.OverallMakerScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RunProgressiveBenchmark_Summary_ContainsAllResults()
    {
        var report = await _sut.RunProgressiveBenchmarkAsync(maxDiscs: 3);
        var summary = report.GetSummary();

        summary.Should().Contain("MAKER Progressive Benchmark Report");
        summary.Should().Contain("Max Discs Completed: 3/3");
        summary.Should().Contain("PASS");
    }

    [Fact]
    public async Task RunTowerOfHanoi_12Discs_Succeeds_4095Steps()
    {
        // Exceeding Gas Town's proven benchmark (10 discs)
        var report = await _sut.RunTowerOfHanoiAsync(12);

        report.Success.Should().BeTrue();
        report.TotalStepsRequired.Should().Be(4095);
        report.StepsCompleted.Should().Be(4095);
        report.StepsFailed.Should().Be(0);
    }

    [Fact]
    public async Task RunTowerOfHanoi_15Discs_Succeeds_32767Steps()
    {
        // Well beyond Gas Town's proven benchmark
        var report = await _sut.RunTowerOfHanoiAsync(15);

        report.Success.Should().BeTrue();
        report.TotalStepsRequired.Should().Be(32767);
        report.StepsCompleted.Should().Be(32767);
    }
}
