using System.Threading.Tasks;

namespace CognitiveMesh.Shared.Interfaces
{
    /// <summary>
    /// Provides a mechanism for asynchronous initialization.
    /// </summary>
    public interface IAsyncInitialization
    {
        /// <summary>
        /// Gets the task that represents the asynchronous initialization.
        /// </summary>
        Task Initialization { get; }
    }
}
