using System;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Interfaces
{
    /// <summary>
    /// Defines the contract for retry policies.
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// Executes the specified asynchronous operation with retry logic.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
        Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified asynchronous operation with retry logic and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the cancellation token.</exception>
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default);
    }
}
