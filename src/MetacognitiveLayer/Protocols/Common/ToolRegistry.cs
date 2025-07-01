using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common
{
    /// <summary>
    /// Manages the registration and resolution of tools used by the Hybrid MCP + ACP Protocol.
    /// </summary>
    public class ToolRegistry
    {
        private readonly Dictionary<string, object> _tools;
        private readonly ILogger<ToolRegistry> _logger;

        public ToolRegistry(ILogger<ToolRegistry> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tools = new Dictionary<string, object>();
        }

        /// <summary>
        /// Registers a new tool with the protocol system.
        /// </summary>
        /// <param name="toolId">Unique identifier for the tool</param>
        /// <param name="toolDefinition">JSON definition of the tool</param>
        /// <returns>True if registration was successful</returns>
        public Task<bool> RegisterToolAsync(string toolId, string toolDefinition)
        {
            try
            {
                _logger.LogInformation("Registering tool: {ToolId}", toolId);
                
                if (string.IsNullOrEmpty(toolId))
                {
                    throw new ArgumentException("Tool ID cannot be null or empty", nameof(toolId));
                }
                
                if (string.IsNullOrEmpty(toolDefinition))
                {
                    throw new ArgumentException("Tool definition cannot be null or empty", nameof(toolDefinition));
                }
                
                // Parse and validate tool definition
                var toolObject = JsonConvert.DeserializeObject(toolDefinition);
                if (toolObject == null)
                {
                    _logger.LogError("Invalid tool definition JSON for tool {ToolId}", toolId);
                    return Task.FromResult(false);
                }
                
                // Add or update tool in registry
                _tools[toolId] = toolObject;
                
                _logger.LogInformation("Successfully registered tool: {ToolId}", toolId);
                return Task.FromResult(true);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing tool definition JSON for tool {ToolId}", toolId);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering tool: {ToolId}", toolId);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Resolves tools required for a task.
        /// </summary>
        /// <param name="toolIds">List of tool IDs to resolve</param>
        /// <returns>Dictionary of resolved tools</returns>
        public IDictionary<string, object> ResolveTools(IEnumerable<string> toolIds)
        {
            try
            {
                _logger.LogDebug("Resolving tools");
                
                if (toolIds == null)
                {
                    throw new ArgumentNullException(nameof(toolIds));
                }
                
                var resolvedTools = new Dictionary<string, object>();
                
                foreach (var toolId in toolIds)
                {
                    if (_tools.TryGetValue(toolId, out var tool))
                    {
                        resolvedTools[toolId] = tool;
                        _logger.LogDebug("Resolved tool: {ToolId}", toolId);
                    }
                    else
                    {
                        _logger.LogWarning("Tool not found: {ToolId}", toolId);
                    }
                }
                
                return resolvedTools;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tools");
                throw;
            }
        }

        /// <summary>
        /// Checks if a tool is registered.
        /// </summary>
        /// <param name="toolId">ID of the tool to check</param>
        /// <returns>True if the tool is registered</returns>
        public bool IsToolRegistered(string toolId)
        {
            if (string.IsNullOrEmpty(toolId))
            {
                return false;
            }
            
            return _tools.ContainsKey(toolId);
        }

        /// <summary>
        /// Gets all registered tools.
        /// </summary>
        /// <returns>Dictionary of all registered tools</returns>
        public IDictionary<string, object> GetAllTools()
        {
            return new Dictionary<string, object>(_tools);
        }
    }
}