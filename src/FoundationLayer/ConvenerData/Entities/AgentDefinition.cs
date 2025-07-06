using System;
using System.Collections.Generic;
using CognitiveMesh.FoundationLayer.ConvenerData.ValueObjects;

namespace CognitiveMesh.FoundationLayer.ConvenerData.Entities
{
    /// <summary>
    /// Represents the definition of an agent in the system.
    /// </summary>
    public class AgentDefinition
    {
        public string AgentId { get; set; } = Guid.NewGuid().ToString();
        public string AgentType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public Dictionary<string, string> Capabilities { get; set; } = new();
        public AuthorityScope DefaultAuthorityScope { get; set; } = new();
        public AutonomyLevel DefaultAutonomyLevel { get; set; } = AutonomyLevel.RecommendOnly;
        public AgentStatus Status { get; set; } = AgentStatus.Active;
        public Dictionary<string, string> Configuration { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents the status of an agent.
    /// </summary>
    public enum AgentStatus
    {
        Active,
        Inactive,
        PendingApproval,
        Rejected,
        Retired
    }
}
