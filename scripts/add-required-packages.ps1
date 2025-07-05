# This script adds required NuGet packages to the MetacognitiveLayer project

# Navigate to the project directory
$projectPath = "src\MetacognitiveLayer\MetacognitiveLayer.csproj"

# Add required NuGet packages
$packages = @(
    "StackExchange.Redis",
    "Microsoft.ApplicationInsights",
    "Azure.AI.OpenAI",
    "Microsoft.Extensions.Logging",
    "Microsoft.Extensions.Configuration",
    "Microsoft.Extensions.DependencyInjection",
    "System.Text.Json"
)

foreach ($package in $packages) {
    Write-Host "Adding package: $package"
    dotnet add $projectPath package $package --version "*"
}

Write-Host "All required packages have been added."
