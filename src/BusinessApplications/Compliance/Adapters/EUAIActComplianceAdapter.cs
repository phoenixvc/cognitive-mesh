using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.BusinessApplications.Compliance.Ports;
using CognitiveMesh.BusinessApplications.Compliance.Ports.Models;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using FoundationLayer.AuditLogging;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.Compliance.Adapters
{
    /// <summary>
    /// Adapter that implements the EU AI Act Compliance Port.
    /// This adapter is responsible for classifying AI systems by risk, managing conformity assessments for high-risk systems,
    /// ensuring transparency obligations are met, and handling the registration of systems in the EU database.
    /// It serves as a core component of the Ethical &amp; Legal Compliance Framework.
    /// </summary>
    public class EUAIActComplianceAdapter : IEUAIActCompliancePort
    {
        private readonly ILogger<EUAIActComplianceAdapter> _logger;
#pragma warning disable CS0414 // Field is assigned but its value is never used (reserved for future consent verification)
        private readonly IConsentPort _consentPort;
#pragma warning restore CS0414
        private readonly IAuditLoggingAdapter _auditLoggingAdapter;

        // In-memory store for conformity assessments. In a real system, this would be a persistent database.
        private static readonly ConcurrentDictionary<string, ConformityAssessment> _conformityAssessments = new();

        public EUAIActComplianceAdapter(
            ILogger<EUAIActComplianceAdapter> logger,
            IConsentPort consentPort,
            IAuditLoggingAdapter auditLoggingAdapter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consentPort = consentPort ?? throw new ArgumentNullException(nameof(consentPort));
            _auditLoggingAdapter = auditLoggingAdapter ?? throw new ArgumentNullException(nameof(auditLoggingAdapter));
        }

        /// <inheritdoc />
        public Task<RiskClassificationResponse> ClassifySystemRiskAsync(RiskClassificationRequest request)
        {
            _logger.LogInformation("Classifying AI system risk for '{SystemName}' (Version: {SystemVersion}) in tenant {TenantId}.",
                request.SystemName, request.SystemVersion, request.TenantId);

            var response = new RiskClassificationResponse
            {
                SystemName = request.SystemName,
                SystemVersion = request.SystemVersion,
                AssessedAt = DateTimeOffset.UtcNow,
                CorrelationId = request.CorrelationId
            };

            // Rule-based risk classification based on EU AI Act categories.
            // This is a simplified implementation; a real system would use a more sophisticated rules engine.

            // 1. Check for Unacceptable Risk (Title II, Article 5)
            if (IsUnacceptableRisk(request.IntendedPurpose))
            {
                response.RiskLevel = AIRiskLevel.Unacceptable;
                response.Justification = "The system's intended purpose falls under the category of prohibited AI practices as per EU AI Act, Article 5 (e.g., social scoring, real-time remote biometric identification in public spaces).";
                response.ApplicableArticles.Add("Title II, Article 5");
            }
            // 2. Check for High Risk (Title III, Annex III)
            else if (IsHighRisk(request.IntendedPurpose, request.DataSources))
            {
                response.RiskLevel = AIRiskLevel.High;
                response.Justification = "The system is classified as high-risk because its intended purpose falls under one of the critical areas listed in Annex III of the EU AI Act (e.g., recruitment, credit scoring, critical infrastructure management).";
                response.ApplicableArticles.Add("Title III, Chapter 2");
                response.ApplicableArticles.Add("Annex III");
            }
            // 3. Check for Limited Risk (Title IV, Article 52)
            else if (IsLimitedRisk(request.IntendedPurpose))
            {
                response.RiskLevel = AIRiskLevel.Limited;
                response.Justification = "The system is subject to limited risk and specific transparency obligations as it involves direct interaction with natural persons (e.g., chatbot) or generates synthetic content (e.g., deepfake).";
                response.ApplicableArticles.Add("Title IV, Article 52");
            }
            // 4. Default to Minimal Risk
            else
            {
                response.RiskLevel = AIRiskLevel.Minimal;
                response.Justification = "The system does not fall into the unacceptable, high, or limited risk categories. It is encouraged to adhere to voluntary codes of conduct.";
            }

            _logger.LogInformation("System '{SystemName}' classified as {RiskLevel}. Justification: {Justification}",
                request.SystemName, response.RiskLevel, response.Justification);

            // Audit the classification event (fire-and-forget; discard suppresses CS4014)
            _ = _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                Guid.NewGuid().ToString(),
                "EUAIAct",
                request.SystemName,
                true, // The check itself was successful
                new List<string> { $"Classified as {response.RiskLevel}" },
                response.ApplicableArticles,
                "System",
                request.TenantId,
                request.CorrelationId);

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public async Task<ConformityAssessment> RequestConformityAssessmentAsync(ConformityAssessmentRequest request)
        {
            _logger.LogInformation("Conformity assessment requested for agent {AgentId} in tenant {TenantId}.", request.AgentId, request.TenantId);

            // First, verify the system is indeed high-risk.
            // In a real system, we would fetch the agent's definition and intended purpose.
            var classificationRequest = new RiskClassificationRequest
            {
                SystemName = $"Agent-{request.AgentId}",
                SystemVersion = request.SystemVersion,
                IntendedPurpose = "Simulated high-risk purpose: credit scoring and financial analysis.", // Mock purpose
                TenantId = request.TenantId,
                CorrelationId = request.CorrelationId
            };
            var classification = await ClassifySystemRiskAsync(classificationRequest);

            if (classification.RiskLevel != AIRiskLevel.High)
            {
                throw new InvalidOperationException($"Conformity assessment can only be requested for high-risk AI systems. This system was classified as {classification.RiskLevel}.");
            }

            var assessment = new ConformityAssessment
            {
                AssessmentId = Guid.NewGuid().ToString(),
                AgentId = request.AgentId,
                Status = "InProgress",
                Outcome = "Pending",
                AssessedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(1) // Assessments need periodic review
            };

            // Simulate the assessment process
            assessment.Findings.Add("Risk management system documentation verified.");
            assessment.Findings.Add("Data governance and quality checks passed.");
            assessment.Findings.Add("Technical documentation review is pending.");
            assessment.Findings.Add("Human oversight procedures are in place.");
            assessment.EvidenceLocation = $"/compliance/evidence/eu-ai-act/{assessment.AssessmentId}";

            _conformityAssessments[assessment.AssessmentId] = assessment;

            _logger.LogInformation("Conformity assessment {AssessmentId} initiated for agent {AgentId}.", assessment.AssessmentId, request.AgentId);

            return assessment;
        }

        /// <inheritdoc />
        public Task<ConformityAssessment> GetConformityAssessmentStatusAsync(string assessmentId, string tenantId)
        {
            if (_conformityAssessments.TryGetValue(assessmentId, out var assessment))
            {
                return Task.FromResult(assessment);
            }
            _logger.LogWarning("Conformity assessment with ID {AssessmentId} not found for tenant {TenantId}.", assessmentId, tenantId);
            return Task.FromResult<ConformityAssessment>(null!);
        }

        /// <inheritdoc />
        public Task<TransparencyCheckResponse> EnsureTransparencyObligationsAsync(TransparencyCheckRequest request)
        {
            _logger.LogInformation("Ensuring transparency obligations for agent {AgentId} of type {SystemType}.", request.AgentId, request.SystemType);

            var response = new TransparencyCheckResponse
            {
                IsCompliant = true,
                CheckedAt = DateTimeOffset.UtcNow,
                ApplicableArticle = "Title IV, Article 52"
            };

            bool requiresDisclosure = Regex.IsMatch(request.SystemType, @"(Chatbot|Deepfake|BiometricCategorization|EmotionRecognition)", RegexOptions.IgnoreCase);

            if (requiresDisclosure)
            {
                bool isDisclosed = Regex.IsMatch(request.DisclosureContent, @"(AI system|AI-generated|simulated|synthetic content)", RegexOptions.IgnoreCase);
                if (!isDisclosed)
                {
                    response.IsCompliant = false;
                    response.Violations.Add($"System type '{request.SystemType}' requires explicit disclosure that the user is interacting with an AI or viewing AI-generated content. The provided content did not contain this disclosure.");
                }
            }

            _logger.LogInformation("Transparency check for agent {AgentId} completed. Compliant: {IsCompliant}", request.AgentId, response.IsCompliant);
            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<EUDatabaseRegistrationResponse> RegisterHighRiskSystemAsync(EUDatabaseRegistrationRequest request)
        {
            _logger.LogInformation("Registering high-risk AI system {SystemName} (Agent ID: {AgentId}) in EU database.", request.SystemName, request.AgentId);

            // Verify that a valid conformity assessment has been completed.
            if (!_conformityAssessments.TryGetValue(request.ConformityAssessmentId, out var assessment) || assessment.Outcome != "Compliant")
            {
                var error = ErrorEnvelope.RegulatoryNonCompliance(
                    "EU AI Act",
                    "Article 60",
                    "Registration requires a valid and completed conformity assessment with a 'Compliant' outcome.",
                    "Critical",
                    nameof(EUAIActComplianceAdapter),
                    request.CorrelationId);

                return Task.FromResult(new EUDatabaseRegistrationResponse { IsSuccess = false, Status = "Failed", Error = error });
            }

            // Simulate registration
            var registrationId = $"EU-HR-AI-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            _logger.LogInformation("Successfully registered system {SystemName} with registration ID {RegistrationId}.", request.SystemName, registrationId);

            return Task.FromResult(new EUDatabaseRegistrationResponse
            {
                IsSuccess = true,
                RegistrationId = registrationId,
                Status = "Registered",
                RegisteredAt = DateTimeOffset.UtcNow
            });
        }

        #region Private Helper Methods

        private bool IsUnacceptableRisk(string intendedPurpose)
        {
            // Simplified keyword-based check. A real system would use a more robust classification engine.
            var unacceptablePatterns = new[] { "social scoring", "manipulative techniques", "exploit vulnerabilities", "real-time remote biometric" };
            return unacceptablePatterns.Any(p => intendedPurpose.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsHighRisk(string intendedPurpose, List<string> dataSources)
        {
            var highRiskKeywords = new[]
            {
                "recruitment", "promotion", "termination", // Employment
                "credit scoring", "loan application", "creditworthiness", // Credit
                "critical infrastructure", "energy supply", "water management", // Infrastructure
                "biometric identification", "polygraph", // Law enforcement
                "asylum", "visa", "border control", // Migration
                "judicial authority", "democratic process" // Administration of justice
            };
            return highRiskKeywords.Any(k => intendedPurpose.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsLimitedRisk(string intendedPurpose)
        {
            var limitedRiskKeywords = new[] { "chatbot", "customer service bot", "deepfake", "emotion recognition", "biometric categorization" };
            return limitedRiskKeywords.Any(k => intendedPurpose.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
