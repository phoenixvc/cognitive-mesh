using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Ports.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// --- Placeholder Interfaces for Outbound Adapters ---
namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Adapters
{
    // Data required for Employability check
    public class EmployabilityData
    {
        public List<string> UserSkills { get; set; }
        public List<string> MarketTrendingSkills { get; set; }
        public double UserCreativeOutputScore { get; set; }
        public int ProjectsCompleted { get; set; }
        public double CollaborationScore { get; set; }
        public Dictionary<string, double> SkillRelevanceScores { get; set; }
    }

    public interface IEmployabilityDataRepository
    {
        Task<EmployabilityData> GetEmployabilityDataAsync(string userId, string tenantId);
    }

    public interface IConsentVerifier
    {
        Task<bool> VerifyConsentExistsAsync(string userId, string tenantId, string consentType);
    }

    public interface IManualReviewRequester
    {
        Task RequestManualReviewAsync(string userId, string tenantId, string reviewType, Dictionary<string, object> context);
    }
}

// --- Domain Engine Implementation ---
namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Engines
{
    using CognitiveMesh.ReasoningLayer.ValueGeneration.Adapters;

    /// <summary>
    /// A pure domain engine that implements the core business logic for the Employability Port.
    /// This engine is responsible for assessing an individual's employability risk based on their skills,
    /// creativity, and market trends, adhering to the Hexagonal Architecture pattern.
    /// As a sensitive HR operation, it assumes consent has been obtained by the calling layer.
    /// </summary>
    public class EmployabilityPredictorEngine : IEmployabilityPort
    {
        private readonly ILogger<EmployabilityPredictorEngine> _logger;
        private readonly IEmployabilityDataRepository _employabilityDataRepository;
        private readonly IConsentVerifier _consentVerifier;
        private readonly IManualReviewRequester _manualReviewRequester;

        // Model version for provenance tracking, as required by the PRD.
        private const string EmployabilityModelVersion = "EmployabilityPredictor-v1.0";
        
        // Consent type required for this operation
        private const string RequiredConsentType = "EmployabilityAnalysis";
        
        // Threshold for high-risk assessments that require manual review
        private const double HighRiskThreshold = 0.6;

        public EmployabilityPredictorEngine(
            ILogger<EmployabilityPredictorEngine> logger,
            IEmployabilityDataRepository employabilityDataRepository,
            IConsentVerifier consentVerifier,
            IManualReviewRequester manualReviewRequester)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _employabilityDataRepository = employabilityDataRepository ?? throw new ArgumentNullException(nameof(employabilityDataRepository));
            _consentVerifier = consentVerifier ?? throw new ArgumentNullException(nameof(consentVerifier));
            _manualReviewRequester = manualReviewRequester ?? throw new ArgumentNullException(nameof(manualReviewRequester));
        }

        /// <inheritdoc />
        public async Task<EmployabilityCheckResponse> CheckEmployabilityAsync(EmployabilityCheckRequest request)
        {
            _logger.LogInformation("Checking employability for UserId '{UserId}' with CorrelationId '{CorrelationId}'.", 
                request.UserId, request.Provenance.CorrelationId);

            // Verify consent exists - this is a critical step for this sensitive HR operation
            bool hasConsent = await _consentVerifier.VerifyConsentExistsAsync(
                request.UserId, 
                request.Provenance.TenantId, 
                RequiredConsentType);
            
            if (!hasConsent)
            {
                _logger.LogWarning("Consent '{ConsentType}' not found for user '{UserId}'. Operation rejected.", 
                    RequiredConsentType, request.UserId);
                throw new ConsentRequiredException($"Consent '{RequiredConsentType}' is required for employability analysis.");
            }

            // Retrieve employability data
            var data = await _employabilityDataRepository.GetEmployabilityDataAsync(request.UserId, request.Provenance.TenantId);
            if (data == null)
            {
                throw new InvalidOperationException($"No employability data found for user '{request.UserId}'.");
            }

            // Calculate risk factors and score
            var riskFactors = new List<string>();
            var recommendedActions = new List<string>();
            double riskScore = 0.0;

            // Factor 1: Skill gap analysis
            var skillGap = data.MarketTrendingSkills.Except(data.UserSkills, StringComparer.OrdinalIgnoreCase).ToList();
            if (skillGap.Any())
            {
                double gapWeight = 0.4 * (skillGap.Count / (double)Math.Max(1, data.MarketTrendingSkills.Count));
                riskScore += gapWeight;
                var missingSkills = string.Join(", ", skillGap.Take(3));
                riskFactors.Add($"Skill gap identified in trending areas: {missingSkills}.");
                recommendedActions.Add($"Explore training for '{skillGap.First()}'.");
            }

            // Factor 2: Creative output assessment
            if (data.UserCreativeOutputScore < 0.3)
            {
                riskScore += 0.3;
                riskFactors.Add("Low recent creative or innovative output.");
                recommendedActions.Add("Engage in more cross-functional brainstorming projects.");
            }
            
            // Factor 3: Collaboration assessment
            if (data.CollaborationScore < 0.4)
            {
                riskScore += 0.2;
                riskFactors.Add("Limited cross-team collaboration.");
                recommendedActions.Add("Participate in more collaborative projects across different teams.");
            }
            
            // Factor 4: Project completion rate
            if (data.ProjectsCompleted < 2)
            {
                riskScore += 0.1;
                riskFactors.Add("Low project completion rate.");
                recommendedActions.Add("Focus on completing current projects before taking on new ones.");
            }
            
            // Factor 5: Skill relevance
            if (data.SkillRelevanceScores != null && data.SkillRelevanceScores.Any())
            {
                double avgRelevance = data.SkillRelevanceScores.Values.Average();
                if (avgRelevance < 0.5)
                {
                    riskScore += 0.2;
                    riskFactors.Add("Skills have declining market relevance.");
                    
                    // Find the most relevant skill to recommend
                    var mostRelevantSkill = data.SkillRelevanceScores.OrderByDescending(s => s.Value).FirstOrDefault();
                    if (!string.IsNullOrEmpty(mostRelevantSkill.Key))
                    {
                        recommendedActions.Add($"Focus on developing '{mostRelevantSkill.Key}' which has high market relevance.");
                    }
                }
            }
            
            // Cap the risk score at 1.0
            riskScore = Math.Min(1.0, riskScore);
            
            // Determine risk level
            string riskLevel = "Low";
            if (riskScore > HighRiskThreshold) riskLevel = "High";
            else if (riskScore > 0.3) riskLevel = "Medium";
            
            // For high-risk assessments, request manual review
            if (riskScore > HighRiskThreshold)
            {
                _logger.LogInformation("High risk score ({RiskScore}) detected. Requesting manual review for user '{UserId}'.", 
                    riskScore, request.UserId);
                
                await _manualReviewRequester.RequestManualReviewAsync(
                    request.UserId,
                    request.Provenance.TenantId,
                    "EmployabilityHighRisk",
                    new Dictionary<string, object>
                    {
                        { "riskScore", riskScore },
                        { "riskFactors", riskFactors },
                        { "correlationId", request.Provenance.CorrelationId }
                    });
            }

            // Ensure we always provide at least one recommendation
            if (recommendedActions.Count == 0)
            {
                recommendedActions.Add("Continue developing your existing skills to maintain employability.");
            }

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

    /// <summary>
    /// Exception thrown when a required consent is missing.
    /// </summary>
    public class ConsentRequiredException : Exception
    {
        public ConsentRequiredException(string message) : base(message)
        {
        }

        public ConsentRequiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
