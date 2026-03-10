using System.Text.Json;

namespace AgencyLayer.AgentTeamFramework.Serialization;

/// <summary>
/// Provides shared JSON serialization options for agent team communication.
/// All agent teams should use these defaults for consistent serialization.
/// </summary>
public static class AgentJsonDefaults
{
    /// <summary>
    /// Standard options for agent inter-communication: camelCase property names and indented output.
    /// </summary>
    public static JsonSerializerOptions CamelCaseIndented { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
