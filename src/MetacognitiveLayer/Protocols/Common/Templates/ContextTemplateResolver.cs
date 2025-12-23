using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using MetacognitiveLayer.Protocols.Integration;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common.Templates
{
    /// <summary>
    /// Implementation of the context template resolver for parsing XML-based ACP request templates.
    /// </summary>
    public class ContextTemplateResolver : IContextTemplateResolver
    {
        private readonly ILogger<ContextTemplateResolver> _logger;
        private readonly string _templatesDirectory;
        private readonly Dictionary<string, ACPTemplate> _templateCache = new Dictionary<string, ACPTemplate>();

        public ContextTemplateResolver(string templatesDirectory, ILogger<ContextTemplateResolver> logger)
        {
            _templatesDirectory = templatesDirectory ?? throw new ArgumentNullException(nameof(templatesDirectory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Resolves an ACP request into a prompt by applying templates.
        /// </summary>
        public string ResolvePrompt(ACPRequest acpRequest)
        {
            try
            {
                _logger.LogInformation("Resolving prompt for agent: {AgentId}, task: {TaskName}", 
                    acpRequest.AgentId, acpRequest.TaskName);
                
                // Get template for this agent/task
                var template = GetTemplate(acpRequest.AgentId);
                if (template == null)
                {
                    _logger.LogWarning("No template found for agent: {AgentId}, using default template", acpRequest.AgentId);
                    return GenerateDefaultPrompt(acpRequest);
                }

                // Assemble variables for template resolution
                var variables = new Dictionary<string, object>(acpRequest.Parameters);
                variables["agent_id"] = acpRequest.AgentId;
                variables["task_name"] = acpRequest.TaskName;
                
                // Apply the DSL templating
                var result = ApplyTemplate(template, variables);
                
                _logger.LogDebug("Prompt resolved successfully for {AgentId}", acpRequest.AgentId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving prompt for agent: {AgentId}", acpRequest.AgentId);
                // Return a simple default prompt in case of error
                return $"Execute task: {acpRequest.TaskName}\nParameters: {string.Join(", ", acpRequest.Parameters)}";
            }
        }

        /// <summary>
        /// Applies DSL template syntax with the given variables.
        /// </summary>
        public string ApplyDSL(string acpDslTemplate, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(acpDslTemplate))
                return string.Empty;

            try
            {
                _logger.LogDebug("Applying DSL template with {VariableCount} variables", variables.Count);
                
                var result = acpDslTemplate;
                
                // Process variable substitutions
                result = ProcessVariableSubstitutions(result, variables);
                
                // Process conditionals
                result = ProcessConditionals(result, variables);
                
                // Process loops
                result = ProcessLoops(result, variables);
                
                _logger.LogDebug("DSL template applied successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying DSL template");
                return acpDslTemplate;
            }
        }

        /// <summary>
        /// Loads an ACP template from a file or cache.
        /// </summary>
        private ACPTemplate GetTemplate(string agentId)
        {
            try
            {
                // Check cache first
                if (_templateCache.TryGetValue(agentId, out var cachedTemplate))
                {
                    return cachedTemplate;
                }
                
                // Look for template file
                var templatePath = Path.Combine(_templatesDirectory, $"{agentId}.acp-request");
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("Template file not found for agent: {AgentId} at path: {TemplatePath}", 
                        agentId, templatePath);
                    return null;
                }

                // Load and parse XML template
                var serializer = new XmlSerializer(typeof(ACPTemplate));
                using (var fileStream = new FileStream(templatePath, FileMode.Open))
                {
                    var template = (ACPTemplate)serializer.Deserialize(fileStream);
                    
                    // Cache the parsed template
                    _templateCache[agentId] = template;
                    
                    return template;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading template for agent: {AgentId}", agentId);
                return null;
            }
        }

        /// <summary>
        /// Applies the loaded template with the provided variables.
        /// </summary>
        private string ApplyTemplate(ACPTemplate template, Dictionary<string, object> variables)
        {
            var promptBuilder = new StringBuilder();
            
            // Add system instructions
            if (!string.IsNullOrEmpty(template.SystemInstructions))
            {
                promptBuilder.AppendLine("# System Instructions");
                promptBuilder.AppendLine(ApplyDSL(template.SystemInstructions, variables));
                promptBuilder.AppendLine();
            }
            
            // Add role definition
            if (!string.IsNullOrEmpty(template.RoleDefinition))
            {
                promptBuilder.AppendLine("# Role");
                promptBuilder.AppendLine(ApplyDSL(template.RoleDefinition, variables));
                promptBuilder.AppendLine();
            }
            
            // Add context
            if (!string.IsNullOrEmpty(template.Context))
            {
                promptBuilder.AppendLine("# Context");
                promptBuilder.AppendLine(ApplyDSL(template.Context, variables));
                promptBuilder.AppendLine();
            }
            
            // Add examples
            if (template.Examples?.Count > 0)
            {
                promptBuilder.AppendLine("# Examples");
                foreach (var example in template.Examples)
                {
                    promptBuilder.AppendLine("## Example");
                    promptBuilder.AppendLine(ApplyDSL(example, variables));
                    promptBuilder.AppendLine();
                }
            }
            
            // Add tasks
            promptBuilder.AppendLine("# Task");
            promptBuilder.AppendLine(ApplyDSL(template.Task, variables));
            
            // Add constraints
            if (template.Constraints?.Count > 0)
            {
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("# Constraints");
                foreach (var constraint in template.Constraints)
                {
                    promptBuilder.AppendLine($"- {ApplyDSL(constraint, variables)}");
                }
            }
            
            return promptBuilder.ToString();
        }

        /// <summary>
        /// Generates a default prompt when no template is available.
        /// </summary>
        private string GenerateDefaultPrompt(ACPRequest acpRequest)
        {
            var promptBuilder = new StringBuilder();
            
            promptBuilder.AppendLine($"# Task: {acpRequest.TaskName}");
            promptBuilder.AppendLine();
            
            promptBuilder.AppendLine("## Parameters:");
            foreach (var param in acpRequest.Parameters)
            {
                promptBuilder.AppendLine($"- {param.Key}: {param.Value}");
            }
            
            return promptBuilder.ToString();
        }

        /// <summary>
        /// Processes variable substitution in the template text.
        /// </summary>
        private string ProcessVariableSubstitutions(string template, Dictionary<string, object> variables)
        {
            // Match {{variable_name}} patterns
            var result = Regex.Replace(template, @"\{\{([^}]+)\}\}", match =>
            {
                var variableName = match.Groups[1].Value.Trim();
                if (variables.TryGetValue(variableName, out object value))
                {
                    return value?.ToString() ?? string.Empty;
                }
                return match.Value; // Keep original if variable not found
            });
            
            return result;
        }

        /// <summary>
        /// Processes conditional expressions in the template text.
        /// </summary>
        private string ProcessConditionals(string template, Dictionary<string, object> variables)
        {
            // Match {%if variable%} content {%else%} alternative {%endif%} patterns
            var ifPattern = @"\{%\s*if\s+([^%]+)\s*%\}(.*?)(?:\{%\s*else\s*%\}(.*?))?\{%\s*endif\s*%\}";
            return Regex.Replace(template, ifPattern, match =>
            {
                var condition = match.Groups[1].Value.Trim();
                var trueBlock = match.Groups[2].Value;
                var falseBlock = match.Groups.Count > 3 ? match.Groups[3].Value : string.Empty;
                
                bool conditionMet = EvaluateCondition(condition, variables);
                return conditionMet ? trueBlock : falseBlock;
            }, RegexOptions.Singleline);
        }

        /// <summary>
        /// Processes loop expressions in the template text.
        /// </summary>
        private string ProcessLoops(string template, Dictionary<string, object> variables)
        {
            // Match {%for item in items%} content {%endfor%} patterns
            var forPattern = @"\{%\s*for\s+(\w+)\s+in\s+(\w+)\s*%\}(.*?)\{%\s*endfor\s*%\}";
            return Regex.Replace(template, forPattern, match =>
            {
                var itemName = match.Groups[1].Value.Trim();
                var collectionName = match.Groups[2].Value.Trim();
                var loopContent = match.Groups[3].Value;
                
                if (!variables.TryGetValue(collectionName, out object collectionObj) || collectionObj == null)
                {
                    return string.Empty;
                }
                
                // Handle different collection types
                if (collectionObj is IEnumerable<object> collection)
                {
                    var result = new StringBuilder();
                    foreach (var item in collection)
                    {
                        var loopVariables = new Dictionary<string, object>(variables) {[itemName] = item};
                        result.Append(ProcessVariableSubstitutions(loopContent, loopVariables));
                    }
                    return result.ToString();
                }
                
                return string.Empty;
            }, RegexOptions.Singleline);
        }

        /// <summary>
        /// Evaluates a simple condition expression.
        /// </summary>
        private bool EvaluateCondition(string condition, Dictionary<string, object> variables)
        {
            // Handle equality checks
            var equalityMatch = Regex.Match(condition, @"(\w+)\s*==\s*(.+)");
            if (equalityMatch.Success)
            {
                var leftVar = equalityMatch.Groups[1].Value.Trim();
                var rightVal = equalityMatch.Groups[2].Value.Trim().Trim('"', '\'');
                
                if (variables.TryGetValue(leftVar, out object varValue))
                {
                    return varValue?.ToString() == rightVal;
                }
                return false;
            }
            
            // Handle existence checks
            if (variables.TryGetValue(condition, out object value))
            {
                if (value is bool boolValue)
                    return boolValue;
                    
                return value != null;
            }
            
            return false;
        }
    }

    /// <summary>
    /// Represents an ACP template structure from XML.
    /// </summary>
    [XmlRoot("ACPTemplate")]
    public class ACPTemplate
    {
        [XmlElement("SystemInstructions")]
        public string SystemInstructions { get; set; }
        
        [XmlElement("RoleDefinition")]
        public string RoleDefinition { get; set; }
        
        [XmlElement("Context")]
        public string Context { get; set; }
        
        [XmlArray("Examples")]
        [XmlArrayItem("Example")]
        public List<string> Examples { get; set; }
        
        [XmlElement("Task")]
        public string Task { get; set; }
        
        [XmlArray("Constraints")]
        [XmlArrayItem("Constraint")]
        public List<string> Constraints { get; set; }
    }
}