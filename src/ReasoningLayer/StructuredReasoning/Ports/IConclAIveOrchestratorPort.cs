using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;

namespace CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports
{
    /// <summary>
    /// Defines the contract for the ConclAIve Orchestrator.
    /// This is the main entry point for structured reasoning, coordinating
    /// between different reasoning recipes based on the complexity and nature
    /// of the query.
    /// </summary>
    public interface IConclAIveOrchestratorPort
    {
        /// <summary>
        /// Orchestrates structured reasoning by selecting and executing
        /// the most appropriate reasoning recipe for the given query.
        /// </summary>
        /// <param name="query">The query or question to reason about</param>
        /// <param name="recipeType">The specific reasoning recipe to use, or null for auto-selection</param>
        /// <param name="context">Additional context for the reasoning</param>
        /// <returns>A structured, auditable reasoning output</returns>
        Task<ReasoningOutput> ReasonAsync(string query, ReasoningRecipeType? recipeType = null, System.Collections.Generic.Dictionary<string, string>? context = null);
    }
}
