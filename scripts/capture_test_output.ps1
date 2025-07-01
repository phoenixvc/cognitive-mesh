# Create output directory if it doesn't exist
if (!(Test-Path -Path ".\tests\output")) { New-Item -Path ".\tests\output" -ItemType Directory | Out-Null }

# Start transcript
Start-Transcript -Path ".\tests\output\test_transcript.txt" -Force

# Run the test
try {
    $process = Start-Process -FilePath "dotnet" -ArgumentList "test tests\AgencyLayer\ToolIntegration\ToolIntegrationTests.csproj --filter `"FullyQualifiedName=ClassificationToolTests.ExecuteAsync_ValidData_ReturnsResults`" --logger `"console;verbosity=detailed`"" -NoNewWindow -Wait -PassThru
    
    Write-Output "Test process completed with exit code: $($process.ExitCode)"
    
    # Display the output file if it exists
    if (Test-Path ".\tests\output\test_output.txt") {
        Write-Output "=== Test Output ==="
        Get-Content -Path ".\tests\output\test_output.txt"
    }
} 
catch {
    Write-Output "Error running tests: $_"
}
finally {
    # Stop transcript
    Stop-Transcript
    
    # Display the transcript file
    if (Test-Path ".\test_transcript.txt") {
        Write-Output "=== Transcript ==="
        Get-Content -Path ".\test_transcript.txt" -Tail 50
    }
}
