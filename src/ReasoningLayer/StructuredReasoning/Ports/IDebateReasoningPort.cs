using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;

namespace CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports
{
    /// <summary>
    /// Defines the contract for Debate &amp; Vote reasoning.
    /// This orchestrates multiple perspectives to surface diverse angles,
    /// compare arguments, select the best ideas, and synthesize a conclusion.
    /// </summary>
    public interface IDebateReasoningPort
    {
        /// <summary>
        /// Executes a debate and vote process to reach a conclusion.
        /// Multiple perspectives are generated, debated, and synthesized.
        /// </summary>
        /// <param name="request">The debate request with question and perspectives</param>
        /// <returns>A structured reasoning output with the debate trace and conclusion</returns>
        Task<ReasoningOutput> ExecuteDebateAsync(DebateRequest request);
    }
}
