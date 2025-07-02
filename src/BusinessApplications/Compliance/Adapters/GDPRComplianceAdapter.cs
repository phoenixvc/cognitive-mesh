using CognitiveMesh.BusinessApplications.Compliance.Ports;
using CognitiveMesh.FoundationLayer.Ports;
using CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports;
using CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.Compliance.Adapters
{
    /// <summary>
    /// Implements the GDPR compliance port, providing services for consent management,
    /// data subject rights (DSR), and ensuring adherence to GDPR principles like
    /// the "right to explanation" and data portability.
    /// </summary>
    public class GDPRComplianceAdapter : IGDPRCompliancePort
    {
        private readonly ILogger<GDPRComplianceAdapter> _logger;
        private readonly IInformationEthicsPort _informationEthicsPort;
        private readonly IAuditLoggingPort _auditLoggingPort;

        // In a real system, these would be injected ports to persistent data stores in the FoundationLayer.
        // For this simulation, we use thread-safe in-memory dictionaries.
        private static readonly ConcurrentDictionary<string, GDPRConsentRecord> _consentRepository = new();
        private static readonly ConcurrentDictionary<string, Dictionary<string, object>> _userDataStore = new();
        private static readonly ConcurrentDictionary<string, object> _evidenceStore = new();

        public GDPRComplianceAdapter(
            ILogger<GDPRComplianceAdapter> logger,
            IInformationEthicsPort informationEthicsPort,
            IAuditLoggingPort auditLoggingPort)
        {
            _logger = logger;
            _informationEthicsPort = informationEthicsPort;
            _auditLoggingPort = auditLoggingPort;

            // Seed with some sample data for demonstration
            if (_userDataStore.IsEmpty)
            {
                _userDataStore.TryAdd("user-123", new Dictionary<string, object>
                {
                    { "email", "jane.doe@example.com" },
                    { "preferences", new { newsletter = "weekly" } },
                    { "lastLogin", DateTimeOffset.UtcNow.AddDays(-1) }
                });
            }
        }

        /// <inheritdoc />
        public async Task<GDPRConsentRecord> RecordConsentAsync(GDPRConsentRecord consentRecord)
        {
            _logger.LogInformation("Recording GDPR consent for subject '{SubjectId}' of type '{ConsentType}'.", consentRecord.SubjectId, consentRecord.ConsentType);

            // 1. Assess the integrity of the consent request to prevent dark patterns.
            var assessmentRequest = new ConsentIntegrityAssessmentRequest
            {
                // In a real system, the prompt and options would be passed in.
                PromptText = $"Do you agree to {consentRecord.ConsentType}?",
                PresentedOptions = new List<string> { "Yes, I agree", "No, I decline" },
                UserSelection = consentRecord.IsGiven ? "Yes, I agree" : "No, I decline"
            };
            var assessment = await _informationEthicsPort.AssessConsentIntegrityAsync(assessmentRequest);

            if (assessment.IntegrityScore < 0.75)
            {
                var errorMessage = $"Consent rejected due to low integrity score ({assessment.IntegrityScore}). Potential issues: {string.Join(", ", assessment.PotentialIssues)}";
                _logger.LogWarning(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // 2. Store evidence of the consent.
            var evidenceId = Guid.NewGuid().ToString();
            var evidence = new
            {
                ConsentRecord = consentRecord,
                IntegrityAssessment = assessment,
                IpAddress = "127.0.0.1", // This would be captured from the request context.
                UserAgent = "CognitiveMesh/1.0"
            };
            _evidenceStore.TryAdd(evidenceId, evidence);
            consentRecord.EvidenceId = evidenceId;

            // 3. Persist the consent record.
            _consentRepository[GetConsentKey(consentRecord.SubjectId, consentRecord.ConsentType)] = consentRecord;

            // 4. Log the compliance event for audit purposes.
            await _auditLoggingPort.LogEventAsync(new AuditEvent
            {
                EventType = "GDPRConsentRecorded",
                SubjectId = consentRecord.SubjectId,
                Timestamp = DateTimeOffset.UtcNow,
                Details = $"Consent of type '{consentRecord.ConsentType}' recorded with status '{consentRecord.IsGiven}'. Evidence ID: {evidenceId}."
            });

            _logger.LogInformation("Successfully recorded consent for subject '{SubjectId}'. Evidence ID: {EvidenceId}", consentRecord.SubjectId, evidenceId);
            return consentRecord;
        }

        /// <inheritdoc />
        public async Task<DataSubjectRequest> HandleDataSubjectRequestAsync(string subjectId, string requestType)
        {
            _logger.LogInformation("Handling GDPR Data Subject Request of type '{RequestType}' for subject '{SubjectId}'.", requestType, subjectId);

            var dsr = new DataSubjectRequest
            {
                SubjectId = subjectId,
                RequestType = requestType
            };

            try
            {
                switch (requestType.ToLower())
                {
                    case "access":
                        dsr.Data = await ProvideDataAccess(subjectId);
                        dsr.Status = "Completed";
                        break;
                    case "rectify":
                        // In a real flow, the new data would be part of the request.
                        dsr.Status = "Pending-Verification"; // Simulate a verification step.
                        break;
                    case "erasure":
                        await PerformErasure(subjectId);
                        dsr.Status = "Completed";
                        break;
                    default:
                        throw new ArgumentException("Unsupported DSR request type.", nameof(requestType));
                }

                await _auditLoggingPort.LogEventAsync(new AuditEvent
                {
                    EventType = "GDPR_DSR_Handled",
                    SubjectId = subjectId,
                    Timestamp = DateTimeOffset.UtcNow,
                    Details = $"DSR of type '{requestType}' processed with status '{dsr.Status}'. Request ID: {dsr.RequestId}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle DSR for subject '{SubjectId}'.", subjectId);
                dsr.Status = "Failed";
                dsr.Data = ex.Message;
            }

            return dsr;
        }

        private Task<string> ProvideDataAccess(string subjectId)
        {
            if (!_userDataStore.TryGetValue(subjectId, out var userData))
            {
                return Task.FromResult(JsonSerializer.Serialize(new { message = "No data found for this subject." }));
            }

            // Fulfills "Right to Explanation" and "Data Portability"
            var portableData = new
            {
                dataSubjectId = subjectId,
                data = userData,
                processingPurposes = new
                {
                    email = "Used for account identification and communication.",
                    preferences = "Used to personalize user experience.",
                    lastLogin = "Used for security auditing and session management."
                },
                dataPortabilityInfo = new
                {
                    format = "JSON",
                    schemaVersion = "1.0",
                    exportedAt = DateTimeOffset.UtcNow
                }
            };

            return Task.FromResult(JsonSerializer.Serialize(portableData, new JsonSerializerOptions { WriteIndented = true }));
        }

        private Task PerformErasure(string subjectId)
        {
            // In a real system, this would trigger a complex workflow to erase data across all microservices.
            _logger.LogWarning("Initiating erasure workflow for subject '{SubjectId}'.", subjectId);
            _userDataStore.TryRemove(subjectId, out _);
            _consentRepository.TryRemove(GetConsentKey(subjectId, "DataProcessing"), out _);
            _consentRepository.TryRemove(GetConsentKey(subjectId, "Marketing"), out _);
            _logger.LogInformation("Simulated erasure completed for subject '{SubjectId}'.", subjectId);
            return Task.CompletedTask;
        }

        private string GetConsentKey(string subjectId, string consentType) => $"{subjectId}-{consentType}";
    }
}
