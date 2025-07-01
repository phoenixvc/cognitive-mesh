# This script adds required test NuGet packages to the ToolIntegrationTests project

# Navigate to the test project directory
$testProjectPath = "tests\AgencyLayer\ToolIntegration\ToolIntegrationTests.csproj"

# Add required test NuGet packages
$testPackages = @(
    "Microsoft.NET.Test.Sdk",
    "xunit",
    "xunit.runner.visualstudio",
    "Moq",
    "coverlet.collector"
)

foreach ($package in $testPackages) {
    Write-Host "Adding test package: $package"
    dotnet add $testProjectPath package $package --version "*"
}

Write-Host "All required test packages have been added."
