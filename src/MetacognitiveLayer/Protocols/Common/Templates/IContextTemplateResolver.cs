using System.Collections.Generic;
using CognitiveMesh.MetacognitiveLayer.Protocols.Integration;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common.Templates
{
    /// <summary>
    /// Interface for resolving context templates into prompts.
    /// </summary>
    public interface IContextTemplateResolver
    {
        /// <summary>
        /// Resolves an ACP request into a prompt by applying templates.
        /// </summary>
        /// <param name="acpRequest">The ACP request to resolve</param>
        /// <returns>The resolved prompt</returns>
        string ResolvePrompt(ACPRequest acpRequest);
        
        /// <summary>
        /// Applies DSL template syntax with the given variables.
        /// </summary>
        /// <param name="acpDslTemplate">The template string in DSL format</param>
        /// <param name="variables">Variables to apply to the template</param>
        /// <returns>The processed template string</returns>
        string ApplyDSL(string acpDslTemplate, Dictionary<string, object> variables);
    }
}