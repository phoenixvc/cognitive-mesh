@echo off
setlocal

echo Running tests...
dotnet test tests\AgencyLayer\ToolIntegration\ToolIntegrationTests.csproj --filter "FullyQualifiedName=ClassificationToolTests.ExecuteAsync_ValidData_ReturnsResults" --logger "console;verbosity=detailed" > test_output.txt 2>&1
echo Test execution completed with exit code %ERRORLEVEL%
type test_output.txt
