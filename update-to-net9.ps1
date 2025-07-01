# This script updates all .csproj files to target .NET 9.0

# Get all .csproj files
$projectFiles = Get-ChildItem -Path . -Recurse -Filter "*.csproj"

foreach ($file in $projectFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Update TargetFramework to net9.0 if it exists
    if ($content -match '<TargetFramework>[^<]+</TargetFramework>') {
        $updatedContent = $content -replace '<TargetFramework>[^<]+</TargetFramework>', '<TargetFramework>net9.0</TargetFramework>'
        $updatedContent | Set-Content -Path $file.FullName
        Write-Host "Updated $($file.FullName) to target .NET 9.0"
    }
    
    # Update TargetFrameworks to net9.0 if it exists
    if ($content -match '<TargetFrameworks>[^<]+</TargetFrameworks>') {
        $updatedContent = $content -replace '<TargetFrameworks>[^<]+</TargetFrameworks>', '<TargetFrameworks>net9.0</TargetFrameworks>'
        $updatedContent | Set-Content -Path $file.FullName
        Write-Host "Updated $($file.FullName) to target .NET 9.0 (multi-targeting)"
    }
}

Write-Host "All .csproj files have been updated to target .NET 9.0"
