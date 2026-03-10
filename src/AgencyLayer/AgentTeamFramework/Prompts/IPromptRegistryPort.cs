namespace AgencyLayer.AgentTeamFramework.Prompts;

/// <summary>
/// Port for loading and resolving prompt templates used by agent teams.
/// Implementations may load from YAML files, databases, or other sources.
/// </summary>
public interface IPromptRegistryPort
{
    /// <summary>
    /// Loads a single prompt template by its unique identifier.
    /// </summary>
    /// <param name="promptId">The unique identifier of the prompt (e.g., "roadmapcrew-vision-keeper").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The prompt template, or <c>null</c> if not found.</returns>
    Task<PromptTemplate?> GetPromptAsync(string promptId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads all prompt templates for a given team, identified by a path prefix convention.
    /// </summary>
    /// <param name="teamId">The team identifier used to locate prompts (e.g., "roadmapcrew").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All prompt templates belonging to the specified team.</returns>
    Task<IReadOnlyList<PromptTemplate>> GetTeamPromptsAsync(string teamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a prompt with the given identifier exists in the registry.
    /// </summary>
    /// <param name="promptId">The unique identifier to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the prompt exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(string promptId, CancellationToken cancellationToken = default);
}
