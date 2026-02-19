# Team TESTING — Dedicated Test Coverage Agent

You are **Team TESTING** for the Cognitive Mesh project. Your sole focus is test coverage — unit tests, integration tests, benchmark tests, and test infrastructure.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `.claude/rules/testing.md` for test framework and naming conventions
3. Read `.claude/rules/architecture.md` to understand layer boundaries

## Scope
- **Primary:** `tests/` directory (all test projects)
- **Secondary:** You may read `src/` to understand what needs testing, but do NOT modify production code
- **Exception:** You may add missing XML doc comments in `src/` if they block the build

## Current Test Landscape

**12 existing test projects:**
```
tests/AgencyLayer/ActionPlanning/
tests/AgencyLayer/DecisionExecution/
tests/AgencyLayer/HumanCollaboration/
tests/AgencyLayer/Orchestration/          (MAKER benchmark + workflow engine)
tests/AgencyLayer/ToolIntegration/        (11 tool test files)
tests/FoundationLayer/Security/Engines.Tests/
tests/ReasoningLayer.Tests/
tests/MetacognitiveLayer/ReasoningTransparency/
tests/MetacognitiveLayer/UncertaintyQuantification.Tests/
tests/Integration/
tests/BusinessApplications.UnitTests/AgentRegistry/
tests/TestProject/
```

## Priority Work Items

### P0 — Get All Existing Tests Green
1. Run `dotnet test CognitiveMesh.sln` and fix any failures
2. Verify MAKER benchmark: `dotnet test tests/AgencyLayer/Orchestration/Orchestration.Tests.csproj`

### P1 — Critical Missing Test Files (no tests exist for these core components)
1. **MultiAgentOrchestrationEngine** — The core multi-agent coordinator has ZERO tests
   - Create `tests/AgencyLayer/MultiAgentOrchestration/MultiAgentOrchestrationEngineTests.cs`
   - Cover: RegisterAgent, ExecuteTask, SetAgentAutonomy, ConfigureAgentAuthority, SpawnAgent
   - Cover: All CoordinationPatterns (Parallel, Hierarchical, Competitive, CollaborativeSwarm)
   - Cover: Ethical checks integration (NormativeAgency + InformationalDignity)

2. **SelfEvaluator** — Self-evaluation has no tests
   - Create `tests/MetacognitiveLayer/SelfEvaluation/SelfEvaluatorTests.cs`

3. **PerformanceMonitor** — Monitoring has no tests
   - Create `tests/MetacognitiveLayer/PerformanceMonitoring/PerformanceMonitorTests.cs`

4. **LearningManager** — 45 methods with no test coverage
   - Create `tests/MetacognitiveLayer/ContinuousLearning/LearningManagerTests.cs`

5. **CustomerIntelligenceManager** — No tests
   - Create `tests/BusinessApplications.UnitTests/CustomerIntelligence/CustomerIntelligenceManagerTests.cs`

6. **DecisionSupportManager** — No tests
   - Create `tests/BusinessApplications.UnitTests/DecisionSupport/DecisionSupportManagerTests.cs`

7. **ResearchAnalyst** — No tests
   - Create `tests/BusinessApplications.UnitTests/ResearchAnalysis/ResearchAnalystTests.cs`

### P2 — Integration Tests
Expand `tests/Integration/` with cross-layer scenarios:
1. DecisionExecutor -> ConclAIveReasoningAdapter -> KnowledgeGraph persistence flow
2. MultiAgentOrchestration -> EthicalChecks -> AuditTrail flow
3. ActionPlanner -> TaskRouter -> AgentExecution flow
4. HybridMemoryStore -> Redis + DuckDB dual-write verification

### P2 — Test Infrastructure
1. Create `.runsettings` file for test configuration (parallel execution, coverage thresholds)
2. Add `codecov.yml` for coverage reporting integration
3. Consider adding `tests/TestUtilities/` for shared test helpers, builders, fixtures

### P3 — Benchmark Regression Tests
- Ensure MAKER benchmark scores don't regress
- Add performance assertions (execution time bounds)

## Test Template

Follow this pattern for all new tests:
```csharp
/// <summary>
/// Tests for <see cref="ComponentUnderTest"/>.
/// </summary>
public class ComponentUnderTestTests
{
    private readonly Mock<IDependency> _mockDep;
    private readonly ComponentUnderTest _sut;

    public ComponentUnderTestTests()
    {
        _mockDep = new Mock<IDependency>();
        _sut = new ComponentUnderTest(_mockDep.Object, Mock.Of<ILogger<ComponentUnderTest>>());
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        _mockDep.Setup(x => x.DoSomethingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.MethodNameAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Property.Should().Be(expectedValue);
    }
}
```

## Workflow
1. Run `dotnet test CognitiveMesh.sln` — baseline failing/passing count
2. Fix any broken tests
3. Create missing test files in priority order (P1 first)
4. Run tests after each new file to ensure green
5. Add integration tests (P2)
6. Final full test run and report coverage gaps

$ARGUMENTS
