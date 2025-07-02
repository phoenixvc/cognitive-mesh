using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// --- Placeholder Interfaces for Outbound Adapters ---
// These define the contracts for how the pure domain engine communicates with the outside world.
// The concrete implementations of these adapters would reside in the Infrastructure layer.
namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Adapters
{
    // Data required for the Value Diagnostic ($200 Test)
    public class ValueDiagnosticData
    {
        public double AverageImpactScore { get; set; }
        public int HighValueContributions { get; set; }
        public int CreativityEvents { get; set; }
    }

    // Data required for Org Blindness detection
    public class OrgDataSnapshot
    {
        public Dictionary<string, double> PerceivedValueScores { get; set; } // e.g., from surveys
        public Dictionary<string, double> ActualImpactScores { get; set; } // e.g., from project outcomes
    }

    // Data required for Employability check
    public class EmployabilityData
    {
        public List<string> UserSkills { get; set; }
        public List<string> MarketTrendingSkills { get; set; }
        public double UserCreativeOutputScore { get; set; }
    }

    public interface IValueDiagnosticDataRepository
    {
        Task<ValueDiagnosticData> GetValueDiagnosticDataAsync(string targetId, string tenantId);
        Task<OrgDataSnapshot> GetOrgDataSnapshotAsync(string organizationId, string[] departmentFilters, string tenantId);
    }

    public interface IEmployabilityDataRepository
    {
        Task<EmployabilityData> GetEmployabilityDataAsync(string userId, string tenantId);
    }
}


// --- Domain Engine Implementation ---
namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Engines
{
    using CognitiveMesh.ReasoningLayer.ValueGeneration.Adapters;

    /// <summary>
    /// A pure domain engine that implements the core business logic for the Value Generation Port.
    /// This engine is responsible for value diagnostics, organizational blindness detection, and employability risk assessment.
    /// As part of a Hexagonal Architecture, this engine is completely isolated from infrastructure concerns.
    /// </summary>
    public class ValueGenerationEngine : IValueGenerationPort
    {
        private readonly ILogger<ValueGenerationEngine> _logger;
        private readonly IValueDiagnosticDataRepository _valueDataRepository;
        private readonly IEmployabilityDataRepository _employabilityDataRepository;

        // Model versions for provenance tracking, as required by the PRD.
        private const string ValueDiagnosticModelVersion = "ValueDiagnostic-v1.0";
        private const string OrgBlindnessModelVersion = "OrgBlindness-v1.1";
        private const string EmployabilityModelVersion = "EmployabilityPredictor-v1.0";

        public ValueGenerationEngine(
            ILogger<ValueGenerationEngine> logger,
            IValueDiagnosticDataRepository valueDataRepository,
            IEmployabilityDataRepository employabilityDataRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _valueDataRepository = valueDataRepository ?? throw new ArgumentNullException(nameof(valueDataRepository));
            _employabilityDataRepository = employabilityDataRepository ?? throw new ArgumentNullException(nameof(employabilityDataRepository));
        }

        /// <inheritdoc />
        public async Task<ValueDiagnosticResponse> RunValueDiagnosticAsync(ValueDiagnosticRequest request)
        {
            _logger.LogInformation("Running value diagnostic for TargetId '{TargetId}' with CorrelationId '{CorrelationId}'.", request.TargetId, request.Provenance.CorrelationId);

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

            return new ValueDiagnosticResponse
            {
                TargetId = request.TargetId,
                ValueScore = Math.Round(valueScore, 2),
                ValueProfile = profile,
                Strengths = new List<string> { "High Impact Delivery" }, // Placeholder
                DevelopmentOpportunities = new List<string> { "Increase cross-team collaboration" }, // Placeholder
                ModelVersion = ValueDiagnosticModelVersion,
                CorrelationId = request.Provenance.CorrelationId
            };
        }

        /// <inheritdoc />
        public async Task<OrgBlindnessDetectionResponse> DetectOrganizationalBlindnessAsync(OrgBlindnessDetectionRequest request)
        {
            _logger.LogInformation("Detecting organizational blindness for OrgId '{OrgId}' with CorrelationId '{CorrelationId}'.", request.OrganizationId, request.Provenance.CorrelationId);
            
            var data = await _valueDataRepository.GetOrgDataSnapshotAsync(request.OrganizationId, request.DepartmentFilters, request.Provenance.TenantId);
            var blindSpots = new List<string>();
            double totalDiscrepancy = 0;

            foreach (var perceived in data.PerceivedValueScores)
            {
                if (data.ActualImpactScores.TryGetValue(perceived.Key, out var actual))
                {
                    // A large positive discrepancy means perceived value is much higher than actual impact.
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

            double riskScore = Math.Min(1.0, totalDiscrepancy / data.PerceivedValueScores.Count);
            
            return new OrgBlindnessDetectionResponse
            {
                OrganizationId = request.OrganizationId,
                BlindnessRiskScore = Math.Round(riskScore, 2),
                IdentifiedBlindSpots = blindSpots,
                ModelVersion = OrgBlindnessModelVersion,
                CorrelationId = request.Provenance.CorrelationId
            };
        }

        /// <inheritdoc />
        public async Task<EmployabilityCheckResponse> CheckEmployabilityAsync(EmployabilityCheckRequest request)
        {
            _logger.LogInformation("Checking employability for UserId '{UserId}' with CorrelationId '{CorrelationId}'.", request.UserId, request.Provenance.CorrelationId);

            var data = await _employabilityDataRepository.GetEmployabilityDataAsync(request.UserId, request.Provenance.TenantId);
            var riskFactors = new List<string>();
            var recommendedActions = new List<string>();
            double riskScore = 0.0;

            var skillGap = data.MarketTrendingSkills.Except(data.UserSkills, StringComparer.OrdinalIgnoreCase).ToList();
            if (skillGap.Any())
            {
                riskScore += 0.4;
                var missingSkills = string.Join(", ", skillGap.Take(3));
                riskFactors.Add($"Skill gap identified in trending areas: {missingSkills}.");
                recommendedActions.Add($"Explore training for '{skillGap.First()}'.");
            }

            if (data.UserCreativeOutputScore < 0.3)
            {
                riskScore += 0.3;
                riskFactors.Add("Low recent creative or innovative output.");
                recommendedActions.Add("Engage in more cross-functional brainstorming projects.");
            }
            
            string riskLevel = "Low";
            if (riskScore > 0.6) riskLevel = "High";
            else if (riskScore > 0.3) riskLevel = "Medium";

            return new EmployabilityCheckResponse
            {
                UserId = request.UserId,
                EmployabilityRiskScore = Math.Round(riskScore, 2),
                RiskLevel = riskLevel,
                RiskFactors = riskFactors,
                RecommendedActions = recommendedActions,
                ModelVersion = EmployabilityModelVersion,
                CorrelationId = request.Provenance.CorrelationId
            };
        }
    }
}
