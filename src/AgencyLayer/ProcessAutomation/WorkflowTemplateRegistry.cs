using System.Collections.Concurrent;
using AgencyLayer.Orchestration.Execution;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.ProcessAutomation;

/// <summary>
/// Registry for pre-approved workflow templates. Templates registered here
/// bypass synchronous governance checks during execution (governance hot path),
/// while still recording an asynchronous audit trail.
/// </summary>
public class WorkflowTemplateRegistry
{
    private readonly ConcurrentDictionary<string, WorkflowTemplate> _templates = new();
    private readonly ILogger<WorkflowTemplateRegistry> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowTemplateRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public WorkflowTemplateRegistry(ILogger<WorkflowTemplateRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a pre-approved workflow template.
    /// </summary>
    public void RegisterTemplate(WorkflowTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _templates[template.TemplateId] = template;
        _logger.LogInformation("Registered workflow template: {TemplateId} ({TemplateName}), PreApproved={IsPreApproved}",
            template.TemplateId, template.Name, template.IsPreApproved);
    }

    /// <summary>
    /// Creates a WorkflowDefinition from a registered template.
    /// Thread-safe: captures the template reference atomically via ConcurrentDictionary.TryGetValue.
    /// </summary>
    public WorkflowDefinition? CreateWorkflowFromTemplate(string templateId, Dictionary<string, object>? parameters = null)
    {
        if (!_templates.TryGetValue(templateId, out var template))
        {
            _logger.LogWarning("Workflow template {TemplateId} not found", templateId);
            return null;
        }

        // template is a snapshot reference — safe to use even if registry is modified concurrently
        var buildFunc = template.BuildWorkflow
            ?? throw new InvalidOperationException(
                $"Workflow template '{templateId}' ({template.Name}) has no BuildWorkflow delegate.");
        var workflow = buildFunc(parameters ?? new Dictionary<string, object>())
            ?? throw new InvalidOperationException(
                $"BuildWorkflow delegate for template '{templateId}' ({template.Name}) returned null.");
        _logger.LogInformation("Created workflow {WorkflowId} from template {TemplateId}", workflow.WorkflowId, templateId);
        return workflow;
    }

    /// <summary>
    /// Checks if a registered template is pre-approved for governance bypass.
    /// </summary>
    public bool IsPreApproved(string templateId) =>
        _templates.TryGetValue(templateId, out var template) && template.IsPreApproved;

    /// <summary>
    /// Returns all registered workflow templates.
    /// </summary>
    public IEnumerable<WorkflowTemplate> GetAllTemplates() => _templates.Values;
}

/// <summary>
/// A reusable workflow template that can generate WorkflowDefinitions.
/// </summary>
public class WorkflowTemplate
{
    /// <summary>Gets or sets the unique identifier for this template.</summary>
    public string TemplateId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the human-readable name of the template.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a description of what the template does.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this template is pre-approved for governance bypass.</summary>
    public bool IsPreApproved { get; set; }

    /// <summary>Gets or sets the identity of the approver who pre-approved this template.</summary>
    public string ApprovedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the date and time when this template was approved.</summary>
    public DateTime ApprovedAt { get; set; }

    /// <summary>Gets or sets the delegate that builds a <see cref="WorkflowDefinition"/> from a parameter dictionary.</summary>
    public Func<Dictionary<string, object>, WorkflowDefinition> BuildWorkflow { get; set; } = _ => new WorkflowDefinition();
}
