$ErrorActionPreference = "Stop"

# Change to the solution directory
Set-Location $PSScriptRoot

# Run the tests and capture all output
$output = & dotnet test tests\AgencyLayer\ToolIntegration\ToolIntegrationTests.csproj --filter "FullyQualifiedName=ClassificationToolTests.ExecuteAsync_ValidData_ReturnsResults" --logger "console;verbosity=detailed" 2>&1

# Save the output to a file
$output | Out-File -FilePath "tests/output/test_output.txt" -Encoding utf8

# Display the output
Write-Output "Test output saved to tests/output/test_output.txt"
Write-Output "Last 20 lines of output:"
Get-Content "tests/output/test_output.txt" -Tail 20
