using System;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Configuration
{
    /// <summary>
    /// Configuration options for Cosmos DB.
    /// </summary>
    public class CosmosDbOptions
    {
        /// <summary>
        /// The Cosmos DB connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The name of the Cosmos DB database.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// The name of the container for audit events.
        /// </summary>
        public string AuditContainerName { get; set; }
    }
}
