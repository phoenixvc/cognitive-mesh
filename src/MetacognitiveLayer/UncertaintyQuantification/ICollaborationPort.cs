namespace MetacognitiveLayer.UncertaintyQuantification
{
    /// <summary>
    /// Port interface for human collaboration capabilities needed by the MetacognitiveLayer.
    /// This abstraction allows the MetacognitiveLayer to request human intervention
    /// without depending on the AgencyLayer's concrete collaboration implementation.
    /// </summary>
    /// <remarks>
    /// Implementations of this port reside in the AgencyLayer (or higher),
    /// preserving the dependency direction: Foundation - Reasoning - Metacognitive - Agency - Business.
    /// </remarks>
    public interface ICollaborationPort
    {
        /// <summary>
        /// Creates a new collaboration session for human intervention.
        /// </summary>
        /// <param name="sessionName">The name of the session to create.</param>
        /// <param name="description">An optional description of the session's purpose.</param>
        /// <param name="participantIds">The identifiers of participants to include in the session.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous session creation operation.</returns>
        Task CreateCollaborationSessionAsync(
            string sessionName,
            string? description,
            IEnumerable<string> participantIds,
            CancellationToken cancellationToken = default);
    }
}
