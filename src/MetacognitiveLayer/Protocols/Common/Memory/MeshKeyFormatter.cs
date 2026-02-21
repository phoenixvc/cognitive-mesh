namespace MetacognitiveLayer.Protocols.Common.Memory
{
    /// <summary>
    /// Utility for generating Redis-compliant mesh keys.
    /// </summary>
    public static class MeshKeyFormatter
    {
        /// <summary>
        /// Formats a session identifier and key into a Redis-compliant mesh key.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="key">The context key.</param>
        /// <returns>A formatted string in the form <c>mesh:{sessionId}:{key}</c>.</returns>
        public static string Format(string sessionId, string key)
        {
            return $"mesh:{sessionId}:{key}";
        }
    }
}
