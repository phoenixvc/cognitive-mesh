using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.CustomerIntelligence
{
    /// <summary>
    /// Port interface for customer data retrieval and persistence operations.
    /// Adapters implement this to integrate with specific data stores (CosmosDB, SQL, etc.).
    /// </summary>
    public interface ICustomerDataPort
    {
        /// <summary>
        /// Retrieves a customer profile by its unique identifier.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The customer profile, or null if not found.</returns>
        Task<CustomerProfile?> GetProfileAsync(string customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries customer segments based on filtering criteria.
        /// </summary>
        /// <param name="query">The query parameters for filtering segments.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A collection of customer segments matching the query.</returns>
        Task<IEnumerable<CustomerSegment>> QuerySegmentsAsync(CustomerSegmentQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves interaction history for a customer used to generate insights.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A collection of interaction records as key-value dictionaries.</returns>
        Task<IEnumerable<Dictionary<string, object>>> GetInteractionHistoryAsync(string customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves behavioral feature vectors for a customer used in prediction models.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A dictionary of feature names to their numeric values.</returns>
        Task<Dictionary<string, double>> GetBehavioralFeaturesAsync(string customerId, CancellationToken cancellationToken = default);
    }
}
