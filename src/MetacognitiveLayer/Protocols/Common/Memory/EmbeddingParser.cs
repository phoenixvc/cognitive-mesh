using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Utility class for safely parsing vector embeddings.
    /// </summary>
    public static class EmbeddingParser
    {
        /// <summary>
        /// Attempts to parse a JSON string into a float array representing a vector embedding.
        /// </summary>
        /// <param name="json">The JSON string containing the embedding data.</param>
        /// <param name="logger">Optional logger for warning on parse failures.</param>
        /// <param name="key">Optional key identifier used in log messages.</param>
        /// <returns>The parsed float array, or null if parsing fails.</returns>
        public static float[] TryParse(string json, ILogger? logger = null, string? key = null)
        {
            try
            {
                return JsonSerializer.Deserialize<float[]>(json)!;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to parse embedding for key {Key}", key ?? "unknown");
                return null!;
            }
        }
    }
}
