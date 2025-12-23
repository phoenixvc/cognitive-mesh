namespace FoundationLayer.ConvenerData.ValueObjects
{
    /// <summary>
    /// Defines the degree of independent decision-making an agent can exercise without outside direction.
    /// This directly implements the 'Autonomy' dimension of the Agent Framework.
    /// </summary>
    public enum AutonomyLevel
    {
        /// <summary>
        /// The agent can only analyze and provide recommendations. A human or another agent must approve action.
        /// </summary>
        RecommendOnly,

        /// <summary>
        /// The agent can propose and prepare an action but requires explicit confirmation before execution.
        /// </summary>
        ActWithConfirmation,

        /// <summary>
        /// The agent can act independently within its defined authority scope without requiring confirmation for each action.
        /// </summary>
        FullyAutonomous
    }

    /// <summary>
    /// Represents the lifecycle status of an agent in the registry.
    /// </summary>
    public enum AgentStatus
    {
        /// <summary>
        /// The agent is active and available for orchestration.
        /// </summary>
        Active,

        /// <summary>
        /// The agent is supported but will be retired in a future version.
        /// </summary>
        Deprecated,

        /// <summary>
        /// The agent is no longer supported and cannot be used.
        /// </summary>
        Retired
    }
}
