# Security Policy Engine Testing

This document outlines the testing strategy, patterns, and guidelines for the Security Policy Engine.

## Test Structure

Tests are organized using the following pattern:

```
Security.Policy.Engine.Tests/
├── SecurityPolicyEnforcementEngineTests.cs  # Main test class
└── TestData/                               # Test data files (if any)
```

## Running Tests

### Prerequisites

- .NET 7.0 SDK or later
- Visual Studio 2022 or VS Code with C# extensions

### Running All Tests

```bash
dotnet test
```

### Running Tests with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutput="../../../../coverage/"
```

### Viewing Coverage Report

After running tests with coverage, open the HTML report:

```bash
start coverage/report/index.html
```

## Test Categories

### 1. Authentication Tests
- Verify JWT token validation
- Test token expiration
- Handle invalid tokens

### 2. Authorization Tests
- Role-based access control
- Action-specific permissions
- Access denial scenarios

### 3. Policy Validation Tests
- JSON policy validation
- Error handling for invalid policies
- Unsupported policy types

### 4. Compliance Report Tests
- GDPR report generation
- Least privilege violation reports
- Error handling for invalid report types

## Test Patterns

### Test Naming Convention

```csharp
[MethodUnderTest]_[Scenario]_[ExpectedResult]
```

Example:
```csharp
[Fact]
public async Task VerifyAuthenticationAsync_WithExpiredToken_ReturnsNotAuthenticated()
```

### Test Structure

Each test follows the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task TestName()
{
    // Arrange - Set up test data and expectations
    
    // Act - Execute the code under test
    
    // Assert - Verify the results
}
```

### Mocking

Use Moq for mocking dependencies:

```csharp
var mockLogger = new Mock<ILogger<SecurityPolicyEnforcementEngine>>();
var mockConfig = new Mock<IConfiguration>();

// Setup mock behavior
mockConfig.Setup(x => x[It.IsAny<string>()])
    .Returns("expected-value");
```

## Coverage Reports

Coverage reports are generated in multiple formats:

- **HTML**: Detailed coverage report in `coverage/report`
- **LCOV**: For CI/CD integration
- **OpenCover**: For build pipelines

## CI/CD Integration

Add these steps to your CI/CD pipeline:

```yaml
- name: Run Tests with Coverage
  run: |
    dotnet test /p:CollectCoverage=true
    /p:CoverletOutput="../coverage/"
    /p:CoverletOutputFormat=opencover

- name: Generate Report
  run: |
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator
      -reports:"**/coverage.opencover.xml"
      -targetdir:"coverage/report"
      -reporttypes:Html
```

## Best Practices

1. **Isolation**: Each test should be independent
2. **Deterministic**: Tests should produce the same results every time
3. **Readable**: Clear test names and structure
4. **Fast**: Keep tests fast to enable quick feedback
5. **Maintainable**: Use helper methods for common setups

## Troubleshooting

### Common Issues

1. **Missing Dependencies**
   - Run `dotnet restore`
   - Check package versions in `.csproj`

2. **Test Failures**
   - Check test output for detailed error messages
   - Verify mock setups
   - Ensure test data is correct

3. **Coverage Not Generating**
   - Ensure `coverlet.collector` is installed
   - Check for any exceptions during test execution

## Code Coverage Thresholds

Minimum coverage requirements:
- Line Coverage: 80%
- Branch Coverage: 80%
- Method Coverage: 80%

These thresholds are enforced in the build pipeline.
