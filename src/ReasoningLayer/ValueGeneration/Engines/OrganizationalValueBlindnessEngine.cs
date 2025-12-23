using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;

// --- Placeholder Interfaces for Outbound Adapters ---
// Data required for Org Blindness detection
public class OrgDataSnapshot
{
    public Dictionary<string, double> PerceivedValueScores { get; set; } // e.g., from surveys
    public Dictionary<string, double> ActualImpactScores { get; set; } // e.g., from project outcomes
    public Dictionary<string, double> ResourceAllocation { get; set; } // e.g., budget, headcount
    public Dictionary<string, double> RecognitionMetrics { get; set; } // e.g., awards, promotions
}

public interface IOrganizationalDataRepository
{
    Task<OrgDataSnapshot> GetOrgDataSnapshotAsync(string organizationId, string[] departmentFilters, string tenantId);
}

// --- Domain Engine Implementation ---
/// <summary>
/// A pure domain engine that implements the core business logic for the Organizational Blindness Detection Port.
/// This engine is responsible for analyzing organizational data to detect areas where value creation
/// is overlooked or undervalued, adhering to the Hexagonal Architecture pattern.
/// </summary>
public class OrganizationalValueBlindnessEngine : IOrgBlindnessDetectionPort
{
    private readonly ILogger<OrganizationalValueBlindnessEngine> _logger;
    private readonly IOrganizationalDataRepository _orgDataRepository;

    // Model version for provenance tracking, as required by the PRD.
    private const string OrgBlindnessModelVersion = "OrgBlindness-v1.1";

    public OrganizationalValueBlindnessEngine(
        ILogger<OrganizationalValueBlindnessEngine> logger,
        IOrganizationalDataRepository orgDataRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orgDataRepository = orgDataRepository ?? throw new ArgumentNullException(nameof(orgDataRepository));
    }

    /// <inheritdoc />
    public async Task<OrgBlindnessDetectionResponse> DetectOrganizationalBlindnessAsync(OrgBlindnessDetectionRequest request)
    {
        _logger.LogInformation("Detecting organizational blindness for OrgId '{OrgId}' with CorrelationId '{CorrelationId}'.", 
            request.OrganizationId, request.Provenance.CorrelationId);
            
        var data = await _orgDataRepository.GetOrgDataSnapshotAsync(
            request.OrganizationId, 
            request.DepartmentFilters, 
            request.Provenance.TenantId);
            
        if (data == null)
        {
            throw new InvalidOperationException($"No organizational data found for organization '{request.OrganizationId}'.");
        }

        var blindSpots = new List<string>();
        double totalDiscrepancy = 0;
        int comparisonCount = 0;

        // Analyze perceived value vs. actual impact
        foreach (var perceived in data.PerceivedValueScores)
        {
            if (data.ActualImpactScores.TryGetValue(perceived.Key, out var actual))
            {
                comparisonCount++;
                // A large positive discrepancy means perceived value is much higher than actual impact
                var discrepancy = perceived.Value - actual;
                if (discrepancy > 0.3) // 30% gap
                {
                    blindSpots.Add($"Overvaluing '{perceived.Key}' compared to its impact.");
                    totalDiscrepancy += discrepancy;
                }
                else if (discrepancy < -0.3) // 30% gap
                {
                    blindSpots.Add($"Undervaluing '{perceived.Key}' despite its high impact.");
                    totalDiscrepancy += Math.Abs(discrepancy);
                }
            }
        }

        // Analyze resource allocation vs. actual impact
        foreach (var resource in data.ResourceAllocation)
        {
            if (data.ActualImpactScores.TryGetValue(resource.Key, out var actual))
            {
                comparisonCount++;
                // A large positive discrepancy means resources are allocated to low-impact areas
                var discrepancy = resource.Value - actual;
                if (discrepancy > 0.3)
                {
                    blindSpots.Add($"Overallocating resources to '{resource.Key}' relative to its impact.");
                    totalDiscrepancy += discrepancy;
                }
                else if (discrepancy < -0.3)
                {
                    blindSpots.Add($"Underresourcing '{resource.Key}' despite its high impact.");
                    totalDiscrepancy += Math.Abs(discrepancy);
                }
            }
        }

        // Analyze recognition vs. actual impact
        foreach (var recognition in data.RecognitionMetrics)
        {
            if (data.ActualImpactScores.TryGetValue(recognition.Key, out var actual))
            {
                comparisonCount++;
                // A large positive discrepancy means recognition is given to low-impact areas
                var discrepancy = recognition.Value - actual;
                if (discrepancy > 0.3)
                {
                    blindSpots.Add($"Overrecognizing '{recognition.Key}' compared to its impact.");
                    totalDiscrepancy += discrepancy;
                }
                else if (discrepancy < -0.3)
                {
                    blindSpots.Add($"Underrecognizing '{recognition.Key}' despite its high impact.");
                    totalDiscrepancy += Math.Abs(discrepancy);
                }
            }
        }

        // Calculate the overall blindness risk score
        double riskScore = comparisonCount > 0 
            ? Math.Min(1.0, totalDiscrepancy / comparisonCount) 
            : 0.0;
            
        // Deduplicate and limit the number of blind spots reported
        blindSpots = blindSpots.Distinct().Take(10).ToList();
            
        // Ensure we always provide at least one insight if data is available
        if (blindSpots.Count == 0 && comparisonCount > 0)
        {
            blindSpots.Add("No significant value blindness detected. Continue monitoring alignment between perceived and actual value.");
        }

        return new OrgBlindnessDetectionResponse
        {
            OrganizationId = request.OrganizationId,
            BlindnessRiskScore = Math.Round(riskScore, 2),
            IdentifiedBlindSpots = blindSpots,
            ModelVersion = OrgBlindnessModelVersion,
            CorrelationId = request.Provenance.CorrelationId
        };
    }
}