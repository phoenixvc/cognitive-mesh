# This script replaces Newtonsoft.Json usages with System.Text.Json

# Files to update
$files = @(
    "src\MetacognitiveLayer\Protocols\MCP\Models\MCPResponse.cs",
    "src\MetacognitiveLayer\Protocols\MCP\Models\MCPError.cs",
    "src\MetacognitiveLayer\Protocols\MCP\Models\MCPContext.cs",
    "src\MetacognitiveLayer\Protocols\MCP\MCPHandler.cs",
    "src\MetacognitiveLayer\Protocols\Integration\AgencyLayerAdapter.cs",
    "src\MetacognitiveLayer\Protocols\Common\Tools\NodeToolRunner.cs",
    "src\MetacognitiveLayer\Protocols\Common\ToolRegistry.cs",
    "src\MetacognitiveLayer\Protocols\Common\Memory\RedisVectorMemoryStore.cs",
    "src\MetacognitiveLayer\Protocols\Common\Memory\DuckDbMemoryStore.cs"
)

foreach ($file in $files) {
    $fullPath = Join-Path -Path $PSScriptRoot -ChildPath $file
    if (Test-Path $fullPath) {
        $content = Get-Content -Path $fullPath -Raw
        
        # Replace using statements
        $content = $content -replace 'using Newtonsoft\.Json;', 'using System.Text.Json.Serialization;'
        $content = $content -replace 'using Newtonsoft\.Json\.Linq;', 'using System.Text.Json.Nodes;'
        
        # Replace JsonProperty attributes
        $content = $content -replace '\[JsonProperty\(([^)]+)\)\]', '[JsonPropertyName($1)]'
        
        # Add JsonSerializerOptions if not present
        if ($content -notmatch 'JsonSerializerOptions') {
            $content = $content -replace 'using System;', "using System;
using System.Text.Json;"
        }
        
        # Save the changes
        $content | Set-Content -Path $fullPath -NoNewline
        Write-Host "Updated $file"
    } else {
        Write-Warning "File not found: $file"
    }
}

Write-Host "Newtonsoft.Json to System.Text.Json replacement complete."
