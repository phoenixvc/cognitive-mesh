using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.FoundationLayer.AuditLogging.Models;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Interfaces
{
    /// <summary>
    /// Defines the contract for the audit event repository.
    /// </summary>
    public interface IAuditEventRepository : IDisposable
    {
        /// <summary>
        /// Adds an audit event to the repository.
        /// </summary>
        /// <param name="auditEvent">The audit event to add.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="auditEvent"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
        Task AddEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an audit event by its unique identifier.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation with the audit event, or null if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="eventId"/> is null or whitespace.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
        Task<AuditEvent?> GetEventByIdAsync(string eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for audit events based on the specified criteria.
        /// </summary>
        /// <param name="criteria">The search criteria.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation with the search results.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="criteria"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
        Task<IEnumerable<AuditEvent>> SearchEventsAsync(AuditSearchCriteria criteria, CancellationToken cancellationToken = default);
    }
}
