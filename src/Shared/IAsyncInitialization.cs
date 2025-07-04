using System.Threading.Tasks;

namespace CognitiveMesh.Shared.Interfaces
{
    /// <summary>
    /// Provides a mechanism for asynchronous initialization.
    /// </summary>
    public interface IAsyncInitialization
    {
        /// <summary>
        /// The result of the asynchronous initialization of this instance.
        /// </summary>
        Task Initialization { get; }
    }
}
