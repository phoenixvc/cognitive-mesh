namespace FoundationLayer.ConvenerData.ValueObjects
{
    /// <summary>
    /// Defines the scope of authority for an agent, including what domains it can act in,
    /// what resources it can access, and what operations it can perform.
    /// </summary>
    public class AuthorityScope
    {
        /// <summary>
        /// The domains in which the agent has authority to operate.
        /// </summary>
        public List<string> AllowedDomains { get; set; } = new();

        /// <summary>
        /// The specific resources the agent can access.
        /// </summary>
        public List<string> AllowedResources { get; set; } = new();

        /// <summary>
        /// The operations the agent is permitted to perform.
        /// </summary>
        public List<string> AllowedOperations { get; set; } = new();

        /// <summary>
        /// The maximum scope of data the agent can access.
        /// </summary>
        public DataAccessScope DataAccessScope { get; set; } = DataAccessScope.None;

        /// <summary>
        /// The expiration time of this authority scope, if applicable.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Defines the scope of data access for an agent.
    /// </summary>
    public enum DataAccessScope
    {
        /// <summary>
        /// No data access is permitted.
        /// </summary>
        None,

        /// <summary>
        /// Access to public data only.
        /// </summary>
        Public,

        /// <summary>
        /// Access to organization-specific data.
        /// </summary>
        Organization,

        /// <summary>
        /// Access to team-specific data.
        /// </summary>
        Team,

        /// <summary>
        /// Access to user-specific data only.
        /// </summary>
        User,

        /// <summary>
        /// Full access to all data.
        /// </summary>
        Full
    }
}
