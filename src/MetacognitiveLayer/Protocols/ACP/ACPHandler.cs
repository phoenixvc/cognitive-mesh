using System.Xml;
using System.Xml.Serialization;
using MetacognitiveLayer.Protocols.ACP.Models;
using MetacognitiveLayer.Protocols.Common;
using MetacognitiveLayer.Protocols.Common.Tools;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.ACP
{
    /// <summary>
    /// Handles the AI Communication Protocol (ACP) operations, including parsing,
    /// validation, and execution of XML-based templates.
    /// </summary>
    public class ACPHandler
    {
        private readonly ACPValidator _validator;
        private readonly Dictionary<string, string> _templateLibrary;
        private readonly ILogger<ACPHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPHandler"/> class.
        /// </summary>
        /// <param name="validator">The ACP validator used to validate XML templates.</param>
        /// <param name="logger">The logger instance for diagnostic output.</param>
        public ACPHandler(ACPValidator validator, ILogger<ACPHandler> logger)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _templateLibrary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Parses and validates an ACP XML template.
        /// </summary>
        public async Task<ACPTask> ParseTemplateAsync(string templateXml)
        {
            try
            {
                _logger.LogDebug("Parsing ACP template");
                
                // Validate XML against ACP schema
                var isValid = await _validator.ValidateAsync(templateXml);
                if (!isValid)
                {
                    _logger.LogError("Invalid ACP template format");
                    throw new InvalidOperationException("The ACP template does not conform to the required schema");
                }
                
                // Deserialize XML to ACPTask
                ACPTask task;
                using (var reader = new StringReader(templateXml))
                {
                    var serializer = new XmlSerializer(typeof(ACPTask));
                    task = (ACPTask)serializer.Deserialize(reader)!;
                }

                _logger.LogDebug("Successfully parsed ACP template for task: {TaskName}", task?.Name);
                return task!;
            }
            catch (XmlException ex)
            {
                _logger.LogError(ex, "Error parsing ACP XML template");
                throw new InvalidOperationException("Invalid ACP XML format", ex);
            }
        }

        /// <summary>
        /// Executes a task defined by an ACP template.
        /// </summary>
        public async Task<object> ExecuteTemplateAsync(ACPTask task, IDictionary<string, object> tools, SessionContext session)
        {
            try
            {
                _logger.LogInformation("Executing ACP template task: {TaskName}", task.Name);
                
                // Process task parameters
                var processedParameters = ProcessParameters(task.Parameters, session);
                
                // Validate constraints
                ValidateConstraints(task.Constraints, processedParameters);
                
                // Execute tools required by the task
                var result = await ExecuteToolsAsync(task, tools, processedParameters, session);
                
                _logger.LogInformation("Successfully executed ACP template task: {TaskName}", task.Name);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ACP template task: {TaskName}", task.Name);
                throw;
            }
        }

        /// <summary>
        /// Registers a template in the template library.
        /// </summary>
        public Task<bool> RegisterTemplateAsync(string templateId, string templateXml)
        {
            try
            {
                _logger.LogInformation("Registering ACP template: {TemplateId}", templateId);
                
                if (string.IsNullOrEmpty(templateId))
                {
                    throw new ArgumentException("Template ID cannot be null or empty", nameof(templateId));
                }
                
                if (string.IsNullOrEmpty(templateXml))
                {
                    throw new ArgumentException("Template XML cannot be null or empty", nameof(templateXml));
                }
                
                // Validate template before registration
                var isValid = _validator.ValidateAsync(templateXml).Result;
                if (!isValid)
                {
                    _logger.LogError("Cannot register invalid ACP template: {TemplateId}", templateId);
                    return Task.FromResult(false);
                }
                
                // Add or update template in library
                _templateLibrary[templateId] = templateXml;
                
                _logger.LogInformation("Successfully registered ACP template: {TemplateId}", templateId);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering ACP template: {TemplateId}", templateId);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Gets a template from the template library.
        /// </summary>
        public Task<string> GetTemplateAsync(string templateId)
        {
            try
            {
                _logger.LogDebug("Getting ACP template: {TemplateId}", templateId);
                
                if (string.IsNullOrEmpty(templateId))
                {
                    throw new ArgumentException("Template ID cannot be null or empty", nameof(templateId));
                }
                
                if (_templateLibrary.TryGetValue(templateId, out var templateXml))
                {
                    return Task.FromResult(templateXml);
                }
                
                _logger.LogWarning("ACP template not found: {TemplateId}", templateId);
                return Task.FromResult<string>(null!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ACP template: {TemplateId}", templateId);
                throw;
            }
        }

        #region Private Helper Methods

        private IDictionary<string, object> ProcessParameters(IEnumerable<ACPParameter> parameters, SessionContext session)
        {
            var processedParams = new Dictionary<string, object>();
            
            foreach (var param in parameters)
            {
                // Resolve parameter value from context if needed
                object value = param.Value;
                if (param.IsContextual && param.Value is string contextKey)
                {
                    value = session.GetContextValue(contextKey);
                }
                
                processedParams[param.Name] = value;
            }
            
            return processedParams;
        }

        private void ValidateConstraints(IEnumerable<ACPConstraint> constraints, IDictionary<string, object> parameters)
        {
            foreach (var constraint in constraints)
            {
                if (!parameters.TryGetValue(constraint.ParameterName, out var paramValue))
                {
                    throw new InvalidOperationException($"Parameter '{constraint.ParameterName}' not found for constraint validation");
                }
                
                switch (constraint.Type)
                {
                    case "range":
                        ValidateRangeConstraint(constraint, paramValue);
                        break;
                    case "regex":
                        ValidateRegexConstraint(constraint, paramValue);
                        break;
                    case "enum":
                        ValidateEnumConstraint(constraint, paramValue);
                        break;
                    // Add other constraint types as needed
                    default:
                        _logger.LogWarning("Unknown constraint type: {ConstraintType}", constraint.Type);
                        break;
                }
            }
        }

        private void ValidateRangeConstraint(ACPConstraint constraint, object value)
        {
            // Implementation for range validation
            _logger.LogDebug("Validating range constraint for parameter: {ParameterName}", constraint.ParameterName);
        }

        private void ValidateRegexConstraint(ACPConstraint constraint, object value)
        {
            // Implementation for regex validation
            _logger.LogDebug("Validating regex constraint for parameter: {ParameterName}", constraint.ParameterName);
        }

        private void ValidateEnumConstraint(ACPConstraint constraint, object value)
        {
            // Implementation for enum validation
            _logger.LogDebug("Validating enum constraint for parameter: {ParameterName}", constraint.ParameterName);
        }

        private async Task<object> ExecuteToolsAsync(ACPTask task, IDictionary<string, object> tools, IDictionary<string, object> parameters, SessionContext session)
        {
            _logger.LogDebug("Executing tools for task: {TaskName}", task.Name);

            // Execute primary tool
            if (string.IsNullOrEmpty(task.PrimaryTool))
            {
                throw new InvalidOperationException("No primary tool specified in the ACP task");
            }

            if (!tools.TryGetValue(task.PrimaryTool, out var tool))
            {
                throw new InvalidOperationException($"Primary tool '{task.PrimaryTool}' not found in tool registry");
            }

            var toolContext = new ToolContext
            {
                SessionId = session.SessionId,
                UserId = session.UserId,
                AdditionalContext = parameters.ToDictionary(kv => kv.Key, kv => kv.Value)
            };

            object primaryResult;

            // Dispatch based on tool type
            if (tool is IToolRunner toolRunner)
            {
                // Direct IToolRunner implementation
                _logger.LogDebug("Executing primary tool '{ToolName}' via IToolRunner", task.PrimaryTool);
                var inputDict = new Dictionary<string, object>(parameters);
                primaryResult = await toolRunner.Execute(task.PrimaryTool, inputDict, toolContext);
            }
            else if (tool is Func<IDictionary<string, object>, Task<object>> asyncFunc)
            {
                // Async function delegate
                _logger.LogDebug("Executing primary tool '{ToolName}' via async delegate", task.PrimaryTool);
                primaryResult = await asyncFunc(parameters);
            }
            else if (tool is Func<IDictionary<string, object>, object> syncFunc)
            {
                // Synchronous function delegate
                _logger.LogDebug("Executing primary tool '{ToolName}' via sync delegate", task.PrimaryTool);
                primaryResult = syncFunc(parameters);
            }
            else
            {
                _logger.LogWarning(
                    "Primary tool '{ToolName}' has unsupported type {ToolType}. Returning tool object as result.",
                    task.PrimaryTool, tool.GetType().Name);
                primaryResult = tool;
            }

            // Execute additional required tools if specified
            var allResults = new Dictionary<string, object>
            {
                [task.PrimaryTool] = primaryResult
            };

            if (task.RequiredTools != null && task.RequiredTools.Count > 0)
            {
                foreach (var requiredToolName in task.RequiredTools)
                {
                    if (string.Equals(requiredToolName, task.PrimaryTool, StringComparison.OrdinalIgnoreCase))
                    {
                        continue; // Already executed as primary
                    }

                    if (!tools.TryGetValue(requiredToolName, out var requiredTool))
                    {
                        _logger.LogWarning("Required tool '{ToolName}' not found in tool registry; skipping", requiredToolName);
                        continue;
                    }

                    try
                    {
                        object requiredResult;
                        if (requiredTool is IToolRunner requiredRunner)
                        {
                            requiredResult = await requiredRunner.Execute(requiredToolName, new Dictionary<string, object>(parameters), toolContext);
                        }
                        else if (requiredTool is Func<IDictionary<string, object>, Task<object>> reqAsyncFunc)
                        {
                            requiredResult = await reqAsyncFunc(parameters);
                        }
                        else if (requiredTool is Func<IDictionary<string, object>, object> reqSyncFunc)
                        {
                            requiredResult = reqSyncFunc(parameters);
                        }
                        else
                        {
                            _logger.LogWarning("Required tool '{ToolName}' has unsupported type; skipping", requiredToolName);
                            continue;
                        }

                        allResults[requiredToolName] = requiredResult;
                        _logger.LogDebug("Successfully executed required tool: {ToolName}", requiredToolName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing required tool '{ToolName}'; continuing with remaining tools", requiredToolName);
                    }
                }
            }

            // If only the primary tool was executed, return its result directly
            // Otherwise return the combined results dictionary
            return allResults.Count == 1 ? primaryResult : allResults;
        }

        #endregion
    }
}