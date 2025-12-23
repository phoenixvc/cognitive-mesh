using CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CognitiveMesh.ReasoningLayer.EthicalReasoning.Engines
{
    /// <summary>
    /// Implements the core reasoning capabilities for ensuring that all data handling and content generation
    /// within the Cognitive Mesh adheres to the principles of Luciano Floridi's Information Ethics. This engine
    /// focuses on respecting informational dignity, acting as a responsible data steward, maintaining epistemic
    /// integrity, and providing clear attribution for all generated content.
    /// </summary>
    public class InformationEthicsEngine : IInformationEthicsPort
    {
        private readonly ILogger<InformationEthicsEngine> _logger;
        // In-memory store for provenance records. In a real system, this would be a persistent, immutable ledger.
        private static readonly ConcurrentDictionary<string, ProvenanceRecord> _provenanceLedger = new();

        public InformationEthicsEngine(ILogger<InformationEthicsEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<DignityAssessmentResponse> AssessInformationalDignityAsync(DignityAssessmentRequest request)
        {
            _logger.LogInformation("Assessing informational dignity for action '{ProposedAction}' on data type '{DataType}' for subject '{SubjectId}'.",
                request.ProposedAction, request.DataType, request.SubjectId);

            var response = new DignityAssessmentResponse { IsDignityPreserved = true };

            // Rule 1: Check for risk of misrepresentation.
            if (request.ProposedAction == "Analyze" && request.DataType == "Behavioral" && request.ActionContext.Contains("MarketingProfile"))
            {
                response.IsDignityPreserved = false;
                response.PotentialViolations.Add("Risk of Misrepresentation: Using behavioral data for marketing profiles can create an inaccurate or reductive version of the informational self.");
            }

            // Rule 2: Check for denial of access or control.
            if (request.ProposedAction == "Share" && request.ActionContext.Contains("ThirdPartyAnalytics"))
            {
                response.IsDignityPreserved = false;
                response.PotentialViolations.Add("Loss of Control: Sharing data with third parties for analytics diminishes the subject's control over their informational self.");
            }

            // Rule 3: Check for potential informational harm.
            if (request.ProposedAction == "Store" && request.DataType == "Inferred" && request.ActionContext.Contains("Creditworthiness"))
            {
                response.IsDignityPreserved = false;
                response.PotentialViolations.Add("Potential for Informational Harm: Storing inferred data about creditworthiness can lead to significant negative consequences (e.g., loan denial) based on opaque logic.");
            }

            if (response.IsDignityPreserved)
            {
                _logger.LogInformation("Assessment complete: Proposed action is deemed to preserve informational dignity for subject '{SubjectId}'.", request.SubjectId);
            }
            else
            {
                _logger.LogWarning("Assessment complete: Proposed action poses a risk to informational dignity for subject '{SubjectId}'. Violations: {Violations}",
                    request.SubjectId, string.Join(", ", response.PotentialViolations));
            }

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<DataStewardshipValidationResponse> ValidateDataStewardshipAsync(DataStewardshipValidationRequest request)
        {
            _logger.LogInformation("Validating data stewardship for action '{DataAction}' on data type '{DataType}'.", request.DataAction, request.DataType);
            var response = new DataStewardshipValidationResponse { IsCompliant = true };

            // Rule 1: Principle of Necessity.
            if (request.DataAction == "Collect" && request.DataType == "PII" && !request.PolicyIds.Contains("Policy-Explicit-Consent-For-PII"))
            {
                response.IsCompliant = false;
                response.ComplianceIssues.Add("Stewardship Violation (Necessity): Collection of PII without a corresponding policy demonstrating explicit consent and necessity.");
            }

            // Rule 2: Principle of Proportionality.
            if (request.DataAction == "Process" && request.DataType == "PII" && request.PolicyIds.Contains("Policy-Anonymization-Possible"))
            {
                response.IsCompliant = false;
                response.ComplianceIssues.Add("Stewardship Violation (Proportionality): Processing PII when anonymized data would suffice for the stated purpose.");
            }

            // Rule 3: Purpose Limitation.
            if (request.DataAction == "Share" && request.PolicyIds.Contains("Policy-Internal-Use-Only"))
            {
                response.IsCompliant = false;
                response.ComplianceIssues.Add("Stewardship Violation (Purpose Limitation): Attempting to share data that is explicitly limited to internal use.");
            }

            if (response.IsCompliant)
            {
                _logger.LogInformation("Data stewardship validation passed for action '{DataAction}'.", request.DataAction);
            }
            else
            {
                _logger.LogWarning("Data stewardship validation failed for action '{DataAction}'. Issues: {Issues}",
                    request.DataAction, string.Join(", ", response.ComplianceIssues));
            }

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<EpistemicResponsibilityCheckResponse> CheckEpistemicResponsibilityAsync(EpistemicResponsibilityCheckRequest request)
        {
            _logger.LogInformation("Checking epistemic responsibility for content from source '{Source}'.", request.Source);
            var response = new EpistemicResponsibilityCheckResponse { ReliabilityScore = 0.5 }; // Start with a neutral score.

            // Adjust score based on source reputation.
            if (Regex.IsMatch(request.Source, @"(reuters\.com|apnews\.com|bbc\.co\.uk)", RegexOptions.IgnoreCase))
            {
                response.ReliabilityScore += 0.3;
                _logger.LogDebug("Source '{Source}' is a reputable news agency. Increasing reliability.", request.Source);
            }
            else if (Regex.IsMatch(request.Source, @"(wikipedia\.org)", RegexOptions.IgnoreCase))
            {
                response.ReliabilityScore += 0.1;
                _logger.LogDebug("Source '{Source}' is a user-editable encyclopedia. Slightly increasing reliability.", request.Source);
            }
            else if (Regex.IsMatch(request.Source, @"(personalblog\.com|forum\.net)", RegexOptions.IgnoreCase))
            {
                response.ReliabilityScore -= 0.2;
                response.IdentifiedRisks.Add("Source has low reputation (personal blog or forum).");
                _logger.LogDebug("Source '{Source}' is a personal blog or forum. Decreasing reliability.", request.Source);
            }

            // Adjust score based on content type and potential for misinformation.
            if (request.ContentType == "FactualClaim")
            {
                var sensationalistRegex = new Regex(@"(\b(shocking|unbelievable|secret|exposed)\b|!{2,})", RegexOptions.IgnoreCase);
                if (sensationalistRegex.IsMatch(request.Content))
                {
                    response.ReliabilityScore -= 0.25;
                    response.IdentifiedRisks.Add("Content contains sensationalist language, which is a high-risk marker for misinformation.");
                    _logger.LogDebug("Content contains sensationalist language. Decreasing reliability.");
                }
            }

            response.ReliabilityScore = Math.Clamp(response.ReliabilityScore, 0.0, 1.0);
            _logger.LogInformation("Epistemic responsibility check complete. Final reliability score: {Score}", response.ReliabilityScore);

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<RegisterAttributionResponse> RegisterAttributionAsync(RegisterAttributionRequest request)
        {
            _logger.LogInformation("Registering attribution for new content '{ContentId}' generated by agent '{AgentId}'.", request.ContentId, request.AgentId);

            var provenanceRecord = new ProvenanceRecord
            {
                ContentId = request.ContentId,
                Timestamp = DateTimeOffset.UtcNow,
                Action = "Created",
                SourceContentIds = request.SourceContentIds,
                AgentId = request.AgentId,
                Justification = request.GenerationProcessDescription
            };

            if (_provenanceLedger.TryAdd(provenanceRecord.ContentId, provenanceRecord))
            {
                _logger.LogInformation("Successfully registered provenance record '{ProvenanceRecordId}'.", provenanceRecord.ContentId);
                return Task.FromResult(new RegisterAttributionResponse { IsSuccess = true, ProvenanceRecordId = provenanceRecord.ContentId });
            }
            else
            {
                var errorMsg = $"Failed to register attribution: A record with ContentId '{request.ContentId}' already exists.";
                _logger.LogError(errorMsg);
                return Task.FromResult(new RegisterAttributionResponse { IsSuccess = false, ErrorMessage = errorMsg });
            }
        }

        /// <inheritdoc />
        public Task<GetProvenanceResponse> GetProvenanceAsync(GetProvenanceRequest request)
        {
            _logger.LogInformation("Retrieving provenance chain for content '{ContentId}'.", request.ContentId);
            var response = new GetProvenanceResponse();
            var chain = new List<ProvenanceRecord>();
            var contentIdsToTrace = new Queue<string>();
            var tracedIds = new HashSet<string>();

            contentIdsToTrace.Enqueue(request.ContentId);

            while (contentIdsToTrace.Count > 0)
            {
                var currentId = contentIdsToTrace.Dequeue();
                if (tracedIds.Contains(currentId)) continue; // Avoid cycles

                if (_provenanceLedger.TryGetValue(currentId, out var record))
                {
                    chain.Add(record);
                    tracedIds.Add(currentId);

                    foreach (var sourceId in record.SourceContentIds)
                    {
                        if (!tracedIds.Contains(sourceId))
                        {
                            contentIdsToTrace.Enqueue(sourceId);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Provenance trace stopped: Could not find record for ContentId '{ContentId}'.", currentId);
                }
            }

            if (chain.Any())
            {
                response.IsSuccess = true;
                response.ProvenanceChain = chain.OrderBy(r => r.Timestamp).ToList();
                _logger.LogInformation("Successfully retrieved provenance chain with {Count} records for content '{ContentId}'.", chain.Count, request.ContentId);
            }
            else
            {
                response.IsSuccess = false;
                response.ErrorMessage = $"No provenance records found for ContentId '{request.ContentId}'.";
                _logger.LogWarning(response.ErrorMessage);
            }

            return Task.FromResult(response);
        }
    }
}
