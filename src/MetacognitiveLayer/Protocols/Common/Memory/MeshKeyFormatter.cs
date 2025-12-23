namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Utility for generating Redis-compliant mesh keys.
    /// </summary>
    public static class MeshKeyFormatter
    {
        public static string Format(string sessionId, string key)
        {
            return $"mesh:{sessionId}:{key}";
        }
    }
}
