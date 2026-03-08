namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Type of state.
/// </summary>
public enum StateType
{
    /// <summary>Agent's internal state.</summary>
    Internal,
    /// <summary>Shared state between agents.</summary>
    Shared,
    /// <summary>Session-scoped state.</summary>
    Session,
    /// <summary>Persistent state.</summary>
    Persistent
}

/// <summary>
/// A state entry.
/// </summary>
public class StateEntry
{
    /// <summary>State key.</summary>
    public required string Key { get; init; }

    /// <summary>State value (JSON serialized).</summary>
    public required string Value { get; init; }

    /// <summary>Value type.</summary>
    public required string ValueType { get; init; }

    /// <summary>State type.</summary>
    public StateType Type { get; init; }

    /// <summary>Owner agent ID.</summary>
    public string? OwnerId { get; init; }

    /// <summary>Version for optimistic concurrency.</summary>
    public long Version { get; init; }

    /// <summary>When created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When last modified.</summary>
    public DateTimeOffset ModifiedAt { get; init; }

    /// <summary>Time-to-live.</summary>
    public TimeSpan? TTL { get; init; }

    /// <summary>Tags.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result of a state operation.
/// </summary>
public class StateOperationResult
{
    /// <summary>Whether operation succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>New version if updated.</summary>
    public long? NewVersion { get; init; }

    /// <summary>Error if failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// A state change event.
/// </summary>
public class StateChangeEvent
{
    /// <summary>State key.</summary>
    public required string Key { get; init; }

    /// <summary>Change type (Set, Delete).</summary>
    public required string ChangeType { get; init; }

    /// <summary>Previous value.</summary>
    public string? PreviousValue { get; init; }

    /// <summary>New value.</summary>
    public string? NewValue { get; init; }

    /// <summary>Changed by.</summary>
    public required string ChangedBy { get; init; }

    /// <summary>When changed.</summary>
    public DateTimeOffset ChangedAt { get; init; }
}

/// <summary>
/// Port for agent state management.
/// Implements the "Agent State Management" / "Shared Memory" patterns.
/// </summary>
public interface IAgentStatePort
{
    /// <summary>
    /// Gets a state value.
    /// </summary>
    Task<StateEntry?> GetAsync(
        string key,
        string? ownerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a typed state value.
    /// </summary>
    Task<T?> GetAsync<T>(
        string key,
        string? ownerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a state value.
    /// </summary>
    Task<StateOperationResult> SetAsync(
        string key,
        string value,
        string valueType,
        string ownerId,
        StateType type = StateType.Internal,
        TimeSpan? ttl = null,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a typed state value.
    /// </summary>
    Task<StateOperationResult> SetAsync<T>(
        string key,
        T value,
        string ownerId,
        StateType type = StateType.Internal,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a state value.
    /// </summary>
    Task<StateOperationResult> DeleteAsync(
        string key,
        string ownerId,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists state entries.
    /// </summary>
    Task<IReadOnlyList<StateEntry>> ListAsync(
        string? ownerId = null,
        StateType? type = null,
        string? keyPrefix = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Watches for state changes.
    /// </summary>
    IAsyncEnumerable<StateChangeEvent> WatchAsync(
        string keyPattern,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a transaction on multiple keys.
    /// </summary>
    Task<StateOperationResult> TransactionAsync(
        IEnumerable<(string Key, string Value, string ValueType)> operations,
        string ownerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all state for an owner.
    /// </summary>
    Task<int> ClearAsync(
        string ownerId,
        StateType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets state history.
    /// </summary>
    Task<IReadOnlyList<StateChangeEvent>> GetHistoryAsync(
        string key,
        int limit = 50,
        CancellationToken cancellationToken = default);
}
