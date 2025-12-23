using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;

namespace CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports
{
    /// <summary>
    /// Defines the contract for Strategic Simulation reasoning.
    /// This explores multiple scenarios using patterns, data, and constraints
    /// to anticipate possible outcomes and make strategic decisions.
    /// </summary>
    public interface IStrategicSimulationPort
    {
        /// <summary>
        /// Executes strategic simulation to explore possible scenarios
        /// and anticipate outcomes based on patterns and constraints.
        /// </summary>
        /// <param name="request">The strategic simulation request</param>
        /// <returns>A structured reasoning output with explored scenarios</returns>
        Task<ReasoningOutput> ExecuteStrategicSimulationAsync(StrategicSimulationRequest request);
    }
}
