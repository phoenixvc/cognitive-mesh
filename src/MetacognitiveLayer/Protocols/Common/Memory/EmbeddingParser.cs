using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Utility class for safely parsing vector embeddings.
    /// </summary>
    public static class EmbeddingParser
    {
        public static float[] TryParse(string json, ILogger logger = null, string key = null)
        {
            try
            {
                return JsonSerializer.Deserialize<float[]>(json);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to parse embedding for key {Key}", key ?? "unknown");
                return null;
            }
        }
    }
}
