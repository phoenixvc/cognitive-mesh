using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;

namespace CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports
{
    /// <summary>
    /// Defines the contract for Sequential Reasoning.
    /// This decomposes complex questions into specialized steps (human-like phases)
    /// and then integrates the results into a global conclusion.
    /// </summary>
    public interface ISequentialReasoningPort
    {
        /// <summary>
        /// Executes sequential reasoning by breaking down a complex question
        /// into manageable steps and synthesizing the final answer.
        /// </summary>
        /// <param name="request">The sequential reasoning request</param>
        /// <returns>A structured reasoning output with step-by-step trace</returns>
        Task<ReasoningOutput> ExecuteSequentialReasoningAsync(SequentialReasoningRequest request);
    }
}
