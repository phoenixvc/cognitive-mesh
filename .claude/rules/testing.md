---
paths:
  - "tests/**/*.cs"
---

# Testing Conventions

## Framework
- xUnit for test framework
- Moq for mocking
- FluentAssertions for assertions (prefer `.Should()` syntax)
- coverlet for code coverage

## Test Structure
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var sut = new SystemUnderTest(mockDep.Object);

    // Act
    var result = await sut.DoSomethingAsync();

    // Assert
    result.Should().BeTrue();
}
```

## Test Project Layout
- Mirror `src/` structure: `tests/AgencyLayer/Orchestration/` tests `src/AgencyLayer/Orchestration/`
- One test class per production class
- Test csproj references the production csproj + Moq + FluentAssertions

## Test csproj Template
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" />
<PackageReference Include="xunit" />
<PackageReference Include="xunit.runner.visualstudio" />
<PackageReference Include="coverlet.collector" />
<PackageReference Include="Moq" />
<PackageReference Include="FluentAssertions" />
```

## MAKER Benchmark Tests
- Location: `tests/AgencyLayer/Orchestration/Benchmarks/`
- Must include gap analysis tests that verify execution pipeline completeness
- Tower of Hanoi tests must validate move correctness (not just step count)
