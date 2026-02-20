using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;

// --- Placeholder Interfaces for Outbound Adapters ---
// Data required for the Value Diagnostic ($200 Test)
public class ValueDiagnosticData
{
    public double AverageImpactScore { get; set; }
    public int HighValueContributions { get; set; }
    public int CreativityEvents { get; set; }
}

public interface IValueDiagnosticDataRepository
{
    /// <summary>Retrieves diagnostic data for the specified target.</summary>
    Task<ValueDiagnosticData> GetValueDiagnosticDataAsync(string targetId, string tenantId);
    /// <summary>Retrieves an organizational data snapshot for blindness detection.</summary>
    Task<OrgDataSnapshot> GetOrgDataSnapshotAsync(string organizationId, string[] departmentFilters, string tenantId);
}

// --- Domain Engine Implementation ---
/// <summary>
/// A pure domain engine that implements the core business logic for the Value Diagnostic Port.
/// This engine is responsible for value diagnostics, assessing the value generation profile
/// of users or teams, adhering to the Hexagonal Architecture pattern.
/// </summary>
public class ValueGenerationDiagnosticEngine : IValueDiagnosticPort
{
    private readonly ILogger<ValueGenerationDiagnosticEngine> _logger;
    private readonly IValueDiagnosticDataRepository _valueDataRepository;

    // Model version for provenance tracking, as required by the PRD.
    private const string ValueDiagnosticModelVersion = "ValueDiagnostic-v1.0";

    public ValueGenerationDiagnosticEngine(
        ILogger<ValueGenerationDiagnosticEngine> logger,
        IValueDiagnosticDataRepository valueDataRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _valueDataRepository = valueDataRepository ?? throw new ArgumentNullException(nameof(valueDataRepository));
    }

    /// <inheritdoc />
    public async Task<ValueDiagnosticResponse> RunValueDiagnosticAsync(ValueDiagnosticRequest request)
    {
        _logger.LogInformation("Running value diagnostic for TargetId '{TargetId}' with CorrelationId '{CorrelationId}'.", 
            request.TargetId, request.Provenance.CorrelationId);

        var data = await _valueDataRepository.GetValueDiagnosticDataAsync(request.TargetId, request.Provenance.TenantId);
        if (data == null)
        {
            throw new InvalidOperationException($"No diagnostic data found for target '{request.TargetId}'.");
        }

        // The "$200 Test" logic: a weighted score of different value indicators.
        double valueScore = (data.AverageImpactScore * 50) + (data.HighValueContributions * 10) + (data.CreativityEvents * 5);
            
        string profile = "Contributor";
        if (valueScore > 150) profile = "Innovator";
        else if (valueScore > 75) profile = "Connector";

        // Generate strengths based on the data
        var strengths = new List<string>();
        if (data.AverageImpactScore > 0.7) strengths.Add("High Impact Delivery");
        if (data.HighValueContributions > 5) strengths.Add("Consistent Value Creation");
        if (data.CreativityEvents > 3) strengths.Add("Creative Problem Solving");
            
        // Generate development opportunities
        var developmentOpportunities = new List<string>();
        if (data.AverageImpactScore < 0.5) developmentOpportunities.Add("Focus on high-impact work");
        if (data.HighValueContributions < 3) developmentOpportunities.Add("Increase value-generating activities");
        if (data.CreativityEvents < 2) developmentOpportunities.Add("Engage in more creative thinking sessions");
            
        // Ensure we always provide at least one strength and one opportunity
        if (strengths.Count == 0) strengths.Add("Balanced Contribution");
        if (developmentOpportunities.Count == 0) developmentOpportunities.Add("Increase cross-team collaboration");

        return new ValueDiagnosticResponse
        {
            TargetId = request.TargetId,
            ValueScore = Math.Round(valueScore, 2),
            ValueProfile = profile,
            Strengths = strengths,
            DevelopmentOpportunities = developmentOpportunities,
            ModelVersion = ValueDiagnosticModelVersion,
            CorrelationId = request.Provenance.CorrelationId
        };
    }
}