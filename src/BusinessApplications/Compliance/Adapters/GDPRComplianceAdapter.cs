using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;
using CognitiveMesh.BusinessApplications.Compliance.Ports;
using CognitiveMesh.BusinessApplications.Compliance.Ports.Models;
using FoundationLayer.AuditLogging;

namespace CognitiveMesh.BusinessApplications.Compliance.Adapters
{
    /// <summary>
    /// Adapter that implements the GDPR Compliance Port in the BusinessApplications Layer.
    /// This adapter is responsible for enforcing GDPR principles, managing data subject rights,
    /// and assessing data processing activities for compliance.
    /// 
    /// It integrates with the Consent Port for verifying and recording GDPR-specific consent,
    /// and with the Audit Logging Adapter for comprehensive audit trails of all GDPR-related events.
    /// </summary>
    public class GDPRComplianceAdapter : IGDPRCompliancePort
    {
        private readonly ILogger<GDPRComplianceAdapter> _logger;
        private readonly IConsentPort _consentPort;
        private readonly IAuditLoggingAdapter _auditLoggingAdapter;
        private readonly IEnumerable<IDataProcessingRegistry> _dataProcessingRegistries;

        /// <summary>
        /// Initializes a new instance of the GDPRComplianceAdapter class.
        /// </summary>
        /// <param name="logger">The logger for this adapter.</param>
        /// <param name="consentPort">The consent port for verifying and recording GDPR-specific consent.</param>
        /// <param name="auditLoggingAdapter">The audit logging adapter for recording GDPR-related events.</param>
        /// <param name="dataProcessingRegistries">The registries of data processing activities.</param>
        public GDPRComplianceAdapter(
            ILogger<GDPRComplianceAdapter> logger,
            IConsentPort consentPort,
            IAuditLoggingAdapter auditLoggingAdapter,
            IEnumerable<IDataProcessingRegistry> dataProcessingRegistries)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consentPort = consentPort ?? throw new ArgumentNullException(nameof(consentPort));
            _auditLoggingAdapter = auditLoggingAdapter ?? throw new ArgumentNullException(nameof(auditLoggingAdapter));
            _dataProcessingRegistries = dataProcessingRegistries ?? throw new ArgumentNullException(nameof(dataProcessingRegistries));
        }

        #region Data Subject Rights

        /// <inheritdoc />
        public async Task<DataSubjectRightResponse> HandleAccessRequestAsync(DataSubjectAccessRequest request)
        {
            try
            {
                _logger.LogInformation("Processing GDPR access request for subject {SubjectId} in tenant {TenantId}", 
                    request.SubjectId, request.TenantId);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.SubjectId) || string.IsNullOrWhiteSpace(request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Access",
                            request.SubjectId,
                            "Invalid request: Subject ID and Tenant ID are required.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Verify identity (in a real implementation, this would be more robust)
                if (!await VerifySubjectIdentityAsync(request.SubjectId, request.IdentityVerificationToken, request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Access",
                            request.SubjectId,
                            "Identity verification failed.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Collect personal data from all registered data processing activities
                var personalData = new Dictionary<string, object>();
                foreach (var registry in _dataProcessingRegistries)
                {
                    var data = await registry.GetPersonalDataAsync(request.SubjectId, request.TenantId);
                    foreach (var item in data)
                    {
                        personalData[item.Key] = item.Value;
                    }
                }

                // Get consent records
                var consentRecords = await _consentPort.GetUserConsentsAsync(request.SubjectId, request.TenantId);

                // Create response
                var response = new DataSubjectRightResponse
                {
                    IsSuccess = true,
                    RequestType = "Access",
                    SubjectId = request.SubjectId,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "PersonalData", personalData },
                        { "ConsentRecords", consentRecords }
                    }
                };

                // Audit the access request
                await _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                    Guid.NewGuid().ToString(),
                    "GDPR",
                    request.SubjectId,
                    true,
                    new List<string>(),
                    new List<string> { "Article 15 (Right of Access)" },
                    request.RequestedBy,
                    request.TenantId,
                    request.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GDPR access request for subject {SubjectId}", request.SubjectId);
                return new DataSubjectRightResponse
                {
                    IsSuccess = false,
                    Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        "Article 15",
                        $"Failed to process access request: {ex.Message}",
                        "Critical",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId)
                };
            }
        }

        /// <inheritdoc />
        public async Task<DataSubjectRightResponse> HandleRectificationRequestAsync(DataSubjectRectificationRequest request)
        {
            try
            {
                _logger.LogInformation("Processing GDPR rectification request for subject {SubjectId} in tenant {TenantId}", 
                    request.SubjectId, request.TenantId);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.SubjectId) || string.IsNullOrWhiteSpace(request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Rectification",
                            request.SubjectId,
                            "Invalid request: Subject ID and Tenant ID are required.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Verify identity
                if (!await VerifySubjectIdentityAsync(request.SubjectId, request.IdentityVerificationToken, request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Rectification",
                            request.SubjectId,
                            "Identity verification failed.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Apply rectifications to all registered data processing activities
                var successfulRectifications = new List<string>();
                var failedRectifications = new List<string>();

                foreach (var registry in _dataProcessingRegistries)
                {
                    try
                    {
                        var result = await registry.UpdatePersonalDataAsync(
                            request.SubjectId, 
                            request.TenantId, 
                            request.Rectifications);
                        
                        if (result)
                        {
                            successfulRectifications.Add(registry.GetType().Name);
                        }
                        else
                        {
                            failedRectifications.Add(registry.GetType().Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error applying rectification in registry {Registry} for subject {SubjectId}", 
                            registry.GetType().Name, request.SubjectId);
                        failedRectifications.Add(registry.GetType().Name);
                    }
                }

                // Create response
                var isSuccess = failedRectifications.Count == 0;
                var response = new DataSubjectRightResponse
                {
                    IsSuccess = isSuccess,
                    RequestType = "Rectification",
                    SubjectId = request.SubjectId,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "SuccessfulRectifications", successfulRectifications },
                        { "FailedRectifications", failedRectifications }
                    }
                };

                if (!isSuccess)
                {
                    response.Error = ErrorEnvelope.DataSubjectRightsViolation(
                        "Rectification",
                        request.SubjectId,
                        $"Failed to apply rectifications in some registries: {string.Join(", ", failedRectifications)}",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId);
                }

                // Audit the rectification request
                await _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                    Guid.NewGuid().ToString(),
                    "GDPR",
                    request.SubjectId,
                    isSuccess,
                    isSuccess ? new List<string>() : new List<string> { "Failed to apply all rectifications" },
                    new List<string> { "Article 16 (Right to Rectification)" },
                    request.RequestedBy,
                    request.TenantId,
                    request.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GDPR rectification request for subject {SubjectId}", request.SubjectId);
                return new DataSubjectRightResponse
                {
                    IsSuccess = false,
                    Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        "Article 16",
                        $"Failed to process rectification request: {ex.Message}",
                        "Critical",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId)
                };
            }
        }

        /// <inheritdoc />
        public async Task<DataSubjectRightResponse> HandleErasureRequestAsync(DataSubjectErasureRequest request)
        {
            try
            {
                _logger.LogInformation("Processing GDPR erasure request for subject {SubjectId} in tenant {TenantId}", 
                    request.SubjectId, request.TenantId);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.SubjectId) || string.IsNullOrWhiteSpace(request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Erasure",
                            request.SubjectId,
                            "Invalid request: Subject ID and Tenant ID are required.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Verify identity
                if (!await VerifySubjectIdentityAsync(request.SubjectId, request.IdentityVerificationToken, request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Erasure",
                            request.SubjectId,
                            "Identity verification failed.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Check for legal basis to retain data
                var legalBasisToRetain = await CheckLegalBasisToRetainDataAsync(request.SubjectId, request.TenantId);
                if (legalBasisToRetain.Any())
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        RequestType = "Erasure",
                        SubjectId = request.SubjectId,
                        ProcessedAt = DateTimeOffset.UtcNow,
                        Data = new Dictionary<string, object>
                        {
                            { "LegalBasisToRetain", legalBasisToRetain }
                        },
                        Error = ErrorEnvelope.InsufficientLegalBasis(
                            "Erasure",
                            "Right to be Forgotten",
                            "Legal basis exists to retain some data: " + string.Join(", ", legalBasisToRetain),
                            nameof(GDPRComplianceAdapter),
                            request.CorrelationId)
                    };
                }

                // Apply erasure to all registered data processing activities
                var successfulErasures = new List<string>();
                var failedErasures = new List<string>();

                foreach (var registry in _dataProcessingRegistries)
                {
                    try
                    {
                        var result = await registry.DeletePersonalDataAsync(request.SubjectId, request.TenantId);
                        if (result)
                        {
                            successfulErasures.Add(registry.GetType().Name);
                        }
                        else
                        {
                            failedErasures.Add(registry.GetType().Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error applying erasure in registry {Registry} for subject {SubjectId}", 
                            registry.GetType().Name, request.SubjectId);
                        failedErasures.Add(registry.GetType().Name);
                    }
                }

                // Revoke all consents
                await _consentPort.RevokeConsentAsync(request.SubjectId, request.TenantId, ConsentTypes.GDPRDataProcessing);
                await _consentPort.RevokeConsentAsync(request.SubjectId, request.TenantId, ConsentTypes.GDPRDataTransferOutsideEU);
                await _consentPort.RevokeConsentAsync(request.SubjectId, request.TenantId, ConsentTypes.GDPRAutomatedDecisionMaking);

                // Create response
                var isSuccess = failedErasures.Count == 0;
                var response = new DataSubjectRightResponse
                {
                    IsSuccess = isSuccess,
                    RequestType = "Erasure",
                    SubjectId = request.SubjectId,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "SuccessfulErasures", successfulErasures },
                        { "FailedErasures", failedErasures }
                    }
                };

                if (!isSuccess)
                {
                    response.Error = ErrorEnvelope.DataSubjectRightsViolation(
                        "Erasure",
                        request.SubjectId,
                        $"Failed to apply erasure in some registries: {string.Join(", ", failedErasures)}",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId);
                }

                // Audit the erasure request
                await _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                    Guid.NewGuid().ToString(),
                    "GDPR",
                    request.SubjectId,
                    isSuccess,
                    isSuccess ? new List<string>() : new List<string> { "Failed to apply all erasures" },
                    new List<string> { "Article 17 (Right to Erasure)" },
                    request.RequestedBy,
                    request.TenantId,
                    request.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GDPR erasure request for subject {SubjectId}", request.SubjectId);
                return new DataSubjectRightResponse
                {
                    IsSuccess = false,
                    Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        "Article 17",
                        $"Failed to process erasure request: {ex.Message}",
                        "Critical",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId)
                };
            }
        }

        /// <inheritdoc />
        public async Task<DataSubjectRightResponse> HandlePortabilityRequestAsync(DataSubjectPortabilityRequest request)
        {
            try
            {
                _logger.LogInformation("Processing GDPR data portability request for subject {SubjectId} in tenant {TenantId}", 
                    request.SubjectId, request.TenantId);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.SubjectId) || string.IsNullOrWhiteSpace(request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Portability",
                            request.SubjectId,
                            "Invalid request: Subject ID and Tenant ID are required.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Verify identity
                if (!await VerifySubjectIdentityAsync(request.SubjectId, request.IdentityVerificationToken, request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Portability",
                            request.SubjectId,
                            "Identity verification failed.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Collect portable data from all registered data processing activities
                var portableData = new Dictionary<string, object>();
                foreach (var registry in _dataProcessingRegistries)
                {
                    var data = await registry.GetPortableDataAsync(request.SubjectId, request.TenantId, request.Format);
                    foreach (var item in data)
                    {
                        portableData[item.Key] = item.Value;
                    }
                }

                // Create response
                var response = new DataSubjectRightResponse
                {
                    IsSuccess = true,
                    RequestType = "Portability",
                    SubjectId = request.SubjectId,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "PortableData", portableData },
                        { "Format", request.Format }
                    }
                };

                // Audit the portability request
                await _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                    Guid.NewGuid().ToString(),
                    "GDPR",
                    request.SubjectId,
                    true,
                    new List<string>(),
                    new List<string> { "Article 20 (Right to Data Portability)" },
                    request.RequestedBy,
                    request.TenantId,
                    request.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GDPR portability request for subject {SubjectId}", request.SubjectId);
                return new DataSubjectRightResponse
                {
                    IsSuccess = false,
                    Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        "Article 20",
                        $"Failed to process portability request: {ex.Message}",
                        "Critical",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId)
                };
            }
        }

        /// <inheritdoc />
        public async Task<DataSubjectRightResponse> HandleObjectionRequestAsync(DataSubjectObjectionRequest request)
        {
            try
            {
                _logger.LogInformation("Processing GDPR objection request for subject {SubjectId} in tenant {TenantId}", 
                    request.SubjectId, request.TenantId);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.SubjectId) || string.IsNullOrWhiteSpace(request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Objection",
                            request.SubjectId,
                            "Invalid request: Subject ID and Tenant ID are required.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Verify identity
                if (!await VerifySubjectIdentityAsync(request.SubjectId, request.IdentityVerificationToken, request.TenantId))
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.DataSubjectRightsViolation(
                            "Objection",
                            request.SubjectId,
                            "Identity verification failed.",
                            nameof(GDPRComplianceAdapter))
                    };
                }

                // Check for compelling legitimate grounds to continue processing
                var legitimateGrounds = await CheckLegitimateGroundsForProcessingAsync(
                    request.SubjectId, 
                    request.TenantId, 
                    request.ProcessingActivity);
                
                if (legitimateGrounds.Any())
                {
                    return new DataSubjectRightResponse
                    {
                        IsSuccess = false,
                        RequestType = "Objection",
                        SubjectId = request.SubjectId,
                        ProcessedAt = DateTimeOffset.UtcNow,
                        Data = new Dictionary<string, object>
                        {
                            { "LegitimateGrounds", legitimateGrounds }
                        },
                        Error = ErrorEnvelope.InsufficientLegalBasis(
                            request.ProcessingActivity,
                            "Legitimate Interest",
                            "Compelling legitimate grounds exist for the processing: " + string.Join(", ", legitimateGrounds),
                            nameof(GDPRComplianceAdapter),
                            request.CorrelationId)
                    };
                }

                // Apply objection to all registered data processing activities
                var successfulObjections = new List<string>();
                var failedObjections = new List<string>();

                foreach (var registry in _dataProcessingRegistries)
                {
                    try
                    {
                        var result = await registry.RegisterObjectionAsync(
                            request.SubjectId, 
                            request.TenantId, 
                            request.ProcessingActivity);
                        
                        if (result)
                        {
                            successfulObjections.Add(registry.GetType().Name);
                        }
                        else
                        {
                            failedObjections.Add(registry.GetType().Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error registering objection in registry {Registry} for subject {SubjectId}", 
                            registry.GetType().Name, request.SubjectId);
                        failedObjections.Add(registry.GetType().Name);
                    }
                }

                // Create response
                var isSuccess = failedObjections.Count == 0;
                var response = new DataSubjectRightResponse
                {
                    IsSuccess = isSuccess,
                    RequestType = "Objection",
                    SubjectId = request.SubjectId,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "SuccessfulObjections", successfulObjections },
                        { "FailedObjections", failedObjections },
                        { "ProcessingActivity", request.ProcessingActivity }
                    }
                };

                if (!isSuccess)
                {
                    response.Error = ErrorEnvelope.DataSubjectRightsViolation(
                        "Objection",
                        request.SubjectId,
                        $"Failed to register objection in some registries: {string.Join(", ", failedObjections)}",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId);
                }

                // Audit the objection request
                await _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                    Guid.NewGuid().ToString(),
                    "GDPR",
                    request.SubjectId,
                    isSuccess,
                    isSuccess ? new List<string>() : new List<string> { "Failed to register all objections" },
                    new List<string> { "Article 21 (Right to Object)" },
                    request.RequestedBy,
                    request.TenantId,
                    request.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GDPR objection request for subject {SubjectId}", request.SubjectId);
                return new DataSubjectRightResponse
                {
                    IsSuccess = false,
                    Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        "Article 21",
                        $"Failed to process objection request: {ex.Message}",
                        "Critical",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId)
                };
            }
        }

        #endregion

        #region Data Processing Assessment

        /// <inheritdoc />
        public async Task<DataProcessingAssessmentResponse> AssessDataProcessingActivityAsync(DataProcessingAssessmentRequest request)
        {
            try
            {
                _logger.LogInformation("Assessing GDPR compliance for data processing activity '{ActivityName}' in tenant {TenantId}", 
                    request.ActivityName, request.TenantId);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.ActivityName) || string.IsNullOrWhiteSpace(request.TenantId))
                {
                    return new DataProcessingAssessmentResponse
                    {
                        IsCompliant = false,
                        Error = ErrorEnvelope.RegulatoryNonCompliance(
                            "GDPR",
                            "Article 5-6",
                            "Invalid request: Activity name and Tenant ID are required.",
                            "Medium",
                            nameof(GDPRComplianceAdapter),
                            request.CorrelationId)
                    };
                }

                // Assess legal basis
                var legalBasisAssessment = AssessLegalBasis(request.LegalBasis, request.ProcessingDetails);
                
                // Assess data minimization
                var dataMinimizationAssessment = AssessDataMinimization(request.DataCategories, request.ProcessingPurpose);
                
                // Assess retention period
                var retentionAssessment = AssessRetentionPeriod(request.RetentionPeriod, request.DataCategories);
                
                // Assess cross-border transfers
                var crossBorderAssessment = AssessCrossBorderTransfers(request.DataTransfers);

                // Compile all issues
                var allIssues = new List<ComplianceIssue>();
                allIssues.AddRange(legalBasisAssessment.Issues);
                allIssues.AddRange(dataMinimizationAssessment.Issues);
                allIssues.AddRange(retentionAssessment.Issues);
                allIssues.AddRange(crossBorderAssessment.Issues);

                // Determine overall compliance
                var isCompliant = legalBasisAssessment.IsCompliant && 
                                 dataMinimizationAssessment.IsCompliant && 
                                 retentionAssessment.IsCompliant &&
                                 crossBorderAssessment.IsCompliant;

                // Create response
                var response = new DataProcessingAssessmentResponse
                {
                    IsCompliant = isCompliant,
                    ActivityName = request.ActivityName,
                    AssessedAt = DateTimeOffset.UtcNow,
                    Issues = allIssues,
                    AssessmentDetails = new Dictionary<string, object>
                    {
                        { "LegalBasisAssessment", legalBasisAssessment },
                        { "DataMinimizationAssessment", dataMinimizationAssessment },
                        { "RetentionAssessment", retentionAssessment },
                        { "CrossBorderAssessment", crossBorderAssessment }
                    }
                };

                if (!isCompliant)
                {
                    var highestSeverityIssue = allIssues.OrderByDescending(i => GetSeverityRank(i.Severity)).FirstOrDefault();
                    response.Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        highestSeverityIssue?.RegulatoryReference ?? "Multiple Articles",
                        $"Data processing activity '{request.ActivityName}' does not comply with GDPR requirements. {allIssues.Count} issues found.",
                        highestSeverityIssue?.Severity ?? "Medium",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId);
                }

                // Audit the assessment
                await _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                    Guid.NewGuid().ToString(),
                    "GDPR",
                    request.ActivityName,
                    isCompliant,
                    isCompliant ? new List<string>() : allIssues.Select(i => i.Description).ToList(),
                    new List<string> { "Article 5 (Principles)", "Article 6 (Lawfulness)" },
                    request.AssessedBy,
                    request.TenantId,
                    request.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing GDPR compliance for data processing activity '{ActivityName}'", request.ActivityName);
                return new DataProcessingAssessmentResponse
                {
                    IsCompliant = false,
                    Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        "Multiple Articles",
                        $"Failed to assess data processing activity: {ex.Message}",
                        "Critical",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId)
                };
            }
        }

        /// <inheritdoc />
        public async Task<ConsentVerificationResponse> VerifyGdprConsentAsync(ConsentVerificationRequest request)
        {
            try
            {
                _logger.LogInformation("Verifying GDPR consent for subject {SubjectId} in tenant {TenantId}", 
                    request.SubjectId, request.TenantId);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.SubjectId) || string.IsNullOrWhiteSpace(request.TenantId))
                {
                    return new ConsentVerificationResponse
                    {
                        HasConsent = false,
                        Error = ErrorEnvelope.MissingRequiredConsent(
                            request.ConsentType,
                            request.SubjectId,
                            request.ProcessingOperation,
                            nameof(GDPRComplianceAdapter),
                            request.CorrelationId)
                    };
                }

                // Map to appropriate GDPR consent type
                string gdprConsentType = MapToGdprConsentType(request.ConsentType);
                if (string.IsNullOrWhiteSpace(gdprConsentType))
                {
                    return new ConsentVerificationResponse
                    {
                        HasConsent = false,
                        Error = ErrorEnvelope.MissingRequiredConsent(
                            request.ConsentType,
                            request.SubjectId,
                            request.ProcessingOperation,
                            nameof(GDPRComplianceAdapter),
                            request.CorrelationId)
                    };
                }

                // Verify consent using the ConsentPort
                var validateRequest = new ValidateConsentRequest
                {
                    UserId = request.SubjectId,
                    TenantId = request.TenantId,
                    RequiredConsentType = gdprConsentType,
                    Scope = request.ProcessingOperation,
                    RequiredConsentLevel = "LegallyBinding",
                    RequiredLegalFramework = "GDPR"
                };

                var validationResponse = await _consentPort.ValidateConsentAsync(validateRequest);
                
                // Create response
                var response = new ConsentVerificationResponse
                {
                    HasConsent = validationResponse.HasConsent,
                    SubjectId = request.SubjectId,
                    ConsentType = gdprConsentType,
                    VerifiedAt = DateTimeOffset.UtcNow,
                    ConsentRecordId = validationResponse.ConsentRecordId
                };

                if (!validationResponse.HasConsent)
                {
                    response.Error = ErrorEnvelope.MissingRequiredConsent(
                        gdprConsentType,
                        request.SubjectId,
                        request.ProcessingOperation,
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId);
                }

                // Audit the consent verification
                await _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                    Guid.NewGuid().ToString(),
                    "GDPR",
                    request.SubjectId,
                    validationResponse.HasConsent,
                    validationResponse.HasConsent ? new List<string>() : new List<string> { "Missing required consent" },
                    new List<string> { "Article 6(1)(a) (Consent)" },
                    request.VerifiedBy,
                    request.TenantId,
                    request.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying GDPR consent for subject {SubjectId}", request.SubjectId);
                return new ConsentVerificationResponse
                {
                    HasConsent = false,
                    Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        "Article 6(1)(a)",
                        $"Failed to verify consent: {ex.Message}",
                        "Critical",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId)
                };
            }
        }

        /// <inheritdoc />
        public async Task<ConsentRecordResponse> RecordGdprConsentAsync(ConsentRecordRequest request)
        {
            try
            {
                _logger.LogInformation("Recording GDPR consent for subject {SubjectId} in tenant {TenantId}", 
                    request.SubjectId, request.TenantId);

                // Validate request
                if (string.IsNullOrWhiteSpace(request.SubjectId) || string.IsNullOrWhiteSpace(request.TenantId))
                {
                    return new ConsentRecordResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.RegulatoryNonCompliance(
                            "GDPR",
                            "Article 7",
                            "Invalid request: Subject ID and Tenant ID are required.",
                            "Medium",
                            nameof(GDPRComplianceAdapter),
                            request.CorrelationId)
                    };
                }

                // Map to appropriate GDPR consent type
                string gdprConsentType = MapToGdprConsentType(request.ConsentType);
                if (string.IsNullOrWhiteSpace(gdprConsentType))
                {
                    return new ConsentRecordResponse
                    {
                        IsSuccess = false,
                        Error = ErrorEnvelope.RegulatoryNonCompliance(
                            "GDPR",
                            "Article 7",
                            $"Invalid consent type: {request.ConsentType}",
                            "Medium",
                            nameof(GDPRComplianceAdapter),
                            request.CorrelationId)
                    };
                }

                // Record consent using the ConsentPort
                var consentRequest = new ConsentRequest
                {
                    UserId = request.SubjectId,
                    TenantId = request.TenantId,
                    ConsentType = gdprConsentType,
                    Scope = request.ProcessingOperation,
                    IsGranted = request.IsGranted,
                    Source = request.Source,
                    Evidence = request.Evidence,
                    ConsentLevel = "LegallyBinding",
                    LegalFramework = "GDPR",
                    ExpirationTime = request.ExpirationTime
                };

                var consentRecord = await _consentPort.RecordConsentAsync(consentRequest);
                
                // Create response
                var response = new ConsentRecordResponse
                {
                    IsSuccess = true,
                    SubjectId = request.SubjectId,
                    ConsentType = gdprConsentType,
                    RecordedAt = DateTimeOffset.UtcNow,
                    ConsentRecordId = consentRecord.ConsentId,
                    IsGranted = request.IsGranted
                };

                // Audit the consent recording
                await _auditLoggingAdapter.LogLegalComplianceCheckedAsync(
                    Guid.NewGuid().ToString(),
                    "GDPR",
                    request.SubjectId,
                    true,
                    new List<string>(),
                    new List<string> { "Article 7 (Conditions for Consent)" },
                    request.RecordedBy,
                    request.TenantId,
                    request.CorrelationId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording GDPR consent for subject {SubjectId}", request.SubjectId);
                return new ConsentRecordResponse
                {
                    IsSuccess = false,
                    Error = ErrorEnvelope.RegulatoryNonCompliance(
                        "GDPR",
                        "Article 7",
                        $"Failed to record consent: {ex.Message}",
                        "Critical",
                        nameof(GDPRComplianceAdapter),
                        request.CorrelationId)
                };
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Verifies the identity of a data subject.
        /// </summary>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="identityVerificationToken">The token used to verify the subject's identity.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>True if the identity is verified; otherwise, false.</returns>
        private Task<bool> VerifySubjectIdentityAsync(string subjectId, string identityVerificationToken, string tenantId)
        {
            // In a real implementation, this would verify the identity token against an identity provider
            // For this implementation, we'll assume the token is valid if it's not empty
            return Task.FromResult(!string.IsNullOrWhiteSpace(identityVerificationToken));
        }

        /// <summary>
        /// Checks if there is a legal basis to retain data despite an erasure request.
        /// </summary>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A list of legal bases to retain data, if any.</returns>
        private Task<List<string>> CheckLegalBasisToRetainDataAsync(string subjectId, string tenantId)
        {
            // In a real implementation, this would check for legal obligations, public interest, etc.
            // For this implementation, we'll assume there is no legal basis to retain data
            return Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Checks if there are compelling legitimate grounds to continue processing despite an objection.
        /// </summary>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="processingActivity">The processing activity being objected to.</param>
        /// <returns>A list of legitimate grounds to continue processing, if any.</returns>
        private Task<List<string>> CheckLegitimateGroundsForProcessingAsync(string subjectId, string tenantId, string processingActivity)
        {
            // In a real implementation, this would check for legitimate interests that override the objection
            // For this implementation, we'll assume there are no compelling legitimate grounds
            return Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Maps a general consent type to a specific GDPR consent type.
        /// </summary>
        /// <param name="consentType">The general consent type.</param>
        /// <returns>The corresponding GDPR consent type.</returns>
        private string MapToGdprConsentType(string consentType)
        {
            return consentType switch
            {
                "DataProcessing" => ConsentTypes.GDPRDataProcessing,
                "DataTransfer" => ConsentTypes.GDPRDataTransferOutsideEU,
                "AutomatedDecision" => ConsentTypes.GDPRAutomatedDecisionMaking,
                _ => consentType // If it's already a GDPR consent type, return it as is
            };
        }

        /// <summary>
        /// Assesses the legal basis for a data processing activity.
        /// </summary>
        /// <param name="legalBasis">The legal basis claimed for the processing.</param>
        /// <param name="processingDetails">Details about the processing activity.</param>
        /// <returns>An assessment of the legal basis.</returns>
        private ComplianceAssessment AssessLegalBasis(string legalBasis, Dictionary<string, object> processingDetails)
        {
            var assessment = new ComplianceAssessment
            {
                Area = "Legal Basis",
                IsCompliant = true
            };

            if (string.IsNullOrWhiteSpace(legalBasis))
            {
                assessment.IsCompliant = false;
                assessment.Issues.Add(new ComplianceIssue
                {
                    IssueId = Guid.NewGuid().ToString(),
                    Framework = "GDPR",
                    RegulatoryReference = "Article 6",
                    Description = "No legal basis specified for data processing.",
                    Severity = "Critical",
                    IdentifiedAt = DateTimeOffset.UtcNow,
                    RemediationSteps = "Specify a valid legal basis for processing (e.g., consent, contract, legitimate interest)."
                });
                return assessment;
            }

            switch (legalBasis.ToLower())
            {
                case "consent":
                    // Check if consent is properly documented
                    if (!processingDetails.ContainsKey("ConsentVerification") || 
                        processingDetails["ConsentVerification"] == null)
                    {
                        assessment.IsCompliant = false;
                        assessment.Issues.Add(new ComplianceIssue
                        {
                            IssueId = Guid.NewGuid().ToString(),
                            Framework = "GDPR",
                            RegulatoryReference = "Article 7",
                            Description = "Consent is claimed as the legal basis, but no consent verification process is documented.",
                            Severity = "High",
                            IdentifiedAt = DateTimeOffset.UtcNow,
                            RemediationSteps = "Implement and document a consent verification process."
                        });
                    }
                    break;

                case "contract":
                    // Check if contract details are provided
                    if (!processingDetails.ContainsKey("ContractDetails") || 
                        processingDetails["ContractDetails"] == null)
                    {
                        assessment.IsCompliant = false;
                        assessment.Issues.Add(new ComplianceIssue
                        {
                            IssueId = Guid.NewGuid().ToString(),
                            Framework = "GDPR",
                            RegulatoryReference = "Article 6(1)(b)",
                            Description = "Contract is claimed as the legal basis, but no contract details are provided.",
                            Severity = "Medium",
                            IdentifiedAt = DateTimeOffset.UtcNow,
                            RemediationSteps = "Provide details of the contract that necessitates this processing."
                        });
                    }
                    break;

                case "legitimate interest":
                    // Check if legitimate interest assessment is provided
                    if (!processingDetails.ContainsKey("LegitimateInterestAssessment") || 
                        processingDetails["LegitimateInterestAssessment"] == null)
                    {
                        assessment.IsCompliant = false;
                        assessment.Issues.Add(new ComplianceIssue
                        {
                            IssueId = Guid.NewGuid().ToString(),
                            Framework = "GDPR",
                            RegulatoryReference = "Article 6(1)(f)",
                            Description = "Legitimate interest is claimed as the legal basis, but no legitimate interest assessment is provided.",
                            Severity = "High",
                            IdentifiedAt = DateTimeOffset.UtcNow,
                            RemediationSteps = "Conduct and document a legitimate interest assessment."
                        });
                    }
                    break;

                default:
                    // Check if the legal basis is valid
                    if (!IsValidLegalBasis(legalBasis))
                    {
                        assessment.IsCompliant = false;
                        assessment.Issues.Add(new ComplianceIssue
                        {
                            IssueId = Guid.NewGuid().ToString(),
                            Framework = "GDPR",
                            RegulatoryReference = "Article 6",
                            Description = $"Invalid legal basis: {legalBasis}",
                            Severity = "Critical",
                            IdentifiedAt = DateTimeOffset.UtcNow,
                            RemediationSteps = "Specify a valid legal basis for processing (e.g., consent, contract, legitimate interest)."
                        });
                    }
                    break;
            }

            return assessment;
        }

        /// <summary>
        /// Assesses data minimization for a data processing activity.
        /// </summary>
        /// <param name="dataCategories">The categories of data being processed.</param>
        /// <param name="processingPurpose">The purpose of the processing.</param>
        /// <returns>An assessment of data minimization.</returns>
        private ComplianceAssessment AssessDataMinimization(List<string> dataCategories, string processingPurpose)
        {
            var assessment = new ComplianceAssessment
            {
                Area = "Data Minimization",
                IsCompliant = true
            };

            if (dataCategories == null || dataCategories.Count == 0)
            {
                assessment.IsCompliant = false;
                assessment.Issues.Add(new ComplianceIssue
                {
                    IssueId = Guid.NewGuid().ToString(),
                    Framework = "GDPR",
                    RegulatoryReference = "Article 5(1)(c)",
                    Description = "No data categories specified for processing.",
                    Severity = "Medium",
                    IdentifiedAt = DateTimeOffset.UtcNow,
                    RemediationSteps = "Specify the categories of data being processed."
                });
                return assessment;
            }

            if (string.IsNullOrWhiteSpace(processingPurpose))
            {
                assessment.IsCompliant = false;
                assessment.Issues.Add(new ComplianceIssue
                {
                    IssueId = Guid.NewGuid().ToString(),
                    Framework = "GDPR",
                    RegulatoryReference = "Article 5(1)(b)",
                    Description = "No processing purpose specified.",
                    Severity = "High",
                    IdentifiedAt = DateTimeOffset.UtcNow,
                    RemediationSteps = "Specify the purpose of the data processing."
                });
                return assessment;
            }

            // Check if any data categories are not necessary for the stated purpose
            var unnecessaryCategories = GetUnnecessaryDataCategories(dataCategories, processingPurpose);
            if (unnecessaryCategories.Any())
            {
                assessment.IsCompliant = false;
                assessment.Issues.Add(new ComplianceIssue
                {
                    IssueId = Guid.NewGuid().ToString(),
                    Framework = "GDPR",
                    RegulatoryReference = "Article 5(1)(c)",
                    Description = $"Some data categories appear unnecessary for the stated purpose: {string.Join(", ", unnecessaryCategories)}",
                    Severity = "Medium",
                    IdentifiedAt = DateTimeOffset.UtcNow,
                    RemediationSteps = "Remove unnecessary data categories or justify their necessity."
                });
            }

            return assessment;
        }

        /// <summary>
        /// Assesses the retention period for a data processing activity.
        /// </summary>
        /// <param name="retentionPeriod">The retention period for the data.</param>
        /// <param name="dataCategories">The categories of data being processed.</param>
        /// <returns>An assessment of the retention period.</returns>
        private ComplianceAssessment AssessRetentionPeriod(string retentionPeriod, List<string> dataCategories)
        {
            var assessment = new ComplianceAssessment
            {
                Area = "Retention Period",
                IsCompliant = true
            };

            if (string.IsNullOrWhiteSpace(retentionPeriod))
            {
                assessment.IsCompliant = false;
                assessment.Issues.Add(new ComplianceIssue
                {
                    IssueId = Guid.NewGuid().ToString(),
                    Framework = "GDPR",
                    RegulatoryReference = "Article 5(1)(e)",
                    Description = "No retention period specified.",
                    Severity = "High",
                    IdentifiedAt = DateTimeOffset.UtcNow,
                    RemediationSteps = "Specify a retention period for the data."
                });
                return assessment;
            }

            // Check if retention period is "indefinite" or "permanent"
            if (retentionPeriod.ToLower().Contains("indefinite") || retentionPeriod.ToLower().Contains("permanent"))
            {
                assessment.IsCompliant = false;
                assessment.Issues.Add(new ComplianceIssue
                {
                    IssueId = Guid.NewGuid().ToString(),
                    Framework = "GDPR",
                    RegulatoryReference = "Article 5(1)(e)",
                    Description = "Indefinite retention period specified, which violates storage limitation principle.",
                    Severity = "High",
                    IdentifiedAt = DateTimeOffset.UtcNow,
                    RemediationSteps = "Define a specific, justified retention period for the data."
                });
            }

            // Check if retention period is appropriate for the data categories
            if (dataCategories != null && dataCategories.Any(c => c.ToLower().Contains("sensitive") || c.ToLower().Contains("special")))
            {
                // For sensitive data, check if retention period is not too long
                if (IsRetentionPeriodTooLong(retentionPeriod))
                {
                    assessment.IsCompliant = false;
                    assessment.Issues.Add(new ComplianceIssue
                    {
                        IssueId = Guid.NewGuid().ToString(),
                        Framework = "GDPR",
                        RegulatoryReference = "Article 5(1)(e)",
                        Description = $"Retention period ({retentionPeriod}) may be too long for sensitive data categories.",
                        Severity = "Medium",
                        IdentifiedAt = DateTimeOffset.UtcNow,
                        RemediationSteps = "Review and justify the retention period for sensitive data, or reduce it."
                    });
                }
            }

            return assessment;
        }

        /// <summary>
        /// Assesses cross-border transfers for a data processing activity.
        /// </summary>
        /// <param name="dataTransfers">Information about data transfers.</param>
        /// <returns>An assessment of cross-border transfers.</returns>
        private ComplianceAssessment AssessCrossBorderTransfers(List<DataTransfer> dataTransfers)
        {
            var assessment = new ComplianceAssessment
            {
                Area = "Cross-Border Transfers",
                IsCompliant = true
            };

            if (dataTransfers == null || dataTransfers.Count == 0)
            {
                // No transfers, so no issues
                return assessment;
            }

            foreach (var transfer in dataTransfers)
            {
                // Check if destination is outside the EU/EEA
                if (IsOutsideEU(transfer.Destination))
                {
                    // Check if there's a transfer mechanism
                    if (string.IsNullOrWhiteSpace(transfer.TransferMechanism))
                    {
                        assessment.IsCompliant = false;
                        assessment.Issues.Add(new ComplianceIssue
                        {
                            IssueId = Guid.NewGuid().ToString(),
                            Framework = "GDPR",
                            RegulatoryReference = "Chapter V (Articles 44-50)",
                            Description = $"Cross-border transfer to {transfer.Destination} without a specified transfer mechanism.",
                            Severity = "Critical",
                            IdentifiedAt = DateTimeOffset.UtcNow,
                            RemediationSteps = "Specify a valid transfer mechanism (e.g., adequacy decision, SCCs, BCRs)."
                        });
                    }
                    else if (!IsValidTransferMechanism(transfer.TransferMechanism))
                    {
                        assessment.IsCompliant = false;
                        assessment.Issues.Add(new ComplianceIssue
                        {
                            IssueId = Guid.NewGuid().ToString(),
                            Framework = "GDPR",
                            RegulatoryReference = "Chapter V (Articles 44-50)",
                            Description = $"Invalid transfer mechanism ({transfer.TransferMechanism}) for transfer to {transfer.Destination}.",
                            Severity = "Critical",
                            IdentifiedAt = DateTimeOffset.UtcNow,
                            RemediationSteps = "Specify a valid transfer mechanism (e.g., adequacy decision, SCCs, BCRs)."
                        });
                    }
                    else if (transfer.TransferMechanism.ToLower() == "sccs" && !transfer.AdditionalSafeguards)
                    {
                        // After Schrems II, SCCs alone may not be sufficient
                        assessment.IsCompliant = false;
                        assessment.Issues.Add(new ComplianceIssue
                        {
                            IssueId = Guid.NewGuid().ToString(),
                            Framework = "GDPR",
                            RegulatoryReference = "Chapter V (Articles 44-50)",
                            Description = $"Standard Contractual Clauses (SCCs) used for transfer to {transfer.Destination} without additional safeguards.",
                            Severity = "High",
                            IdentifiedAt = DateTimeOffset.UtcNow,
                            RemediationSteps = "Implement and document additional safeguards to supplement SCCs."
                        });
                    }
                }
            }

            return assessment;
        }

        /// <summary>
        /// Checks if a legal basis is valid under GDPR.
        /// </summary>
        /// <param name="legalBasis">The legal basis to check.</param>
        /// <returns>True if the legal basis is valid; otherwise, false.</returns>
        private bool IsValidLegalBasis(string legalBasis)
        {
            var validBases = new[]
            {
                "consent",
                "contract",
                "legal obligation",
                "vital interests",
                "public interest",
                "official authority",
                "legitimate interest"
            };

            return validBases.Any(b => legalBasis.ToLower().Contains(b));
        }

        /// <summary>
        /// Gets data categories that appear unnecessary for a given processing purpose.
        /// </summary>
        /// <param name="dataCategories">The categories of data being processed.</param>
        /// <param name="processingPurpose">The purpose of the processing.</param>
        /// <returns>A list of unnecessary data categories.</returns>
        private List<string> GetUnnecessaryDataCategories(List<string> dataCategories, string processingPurpose)
        {
            // In a real implementation, this would use a more sophisticated analysis
            // For this implementation, we'll just flag some common unnecessary categories
            var unnecessaryCategories = new List<string>();
            
            if (processingPurpose.ToLower().Contains("authentication") || 
                processingPurpose.ToLower().Contains("login"))
            {
                // For authentication, certain data categories are likely unnecessary
                var potentiallyUnnecessary = new[] { "political opinions", "religious beliefs", "sexual orientation", "racial origin" };
                unnecessaryCategories.AddRange(dataCategories.Where(c => potentiallyUnnecessary.Any(u => c.ToLower().Contains(u))));
            }
            
            return unnecessaryCategories;
        }

        /// <summary>
        /// Checks if a retention period is too long.
        /// </summary>
        /// <param name="retentionPeriod">The retention period to check.</param>
        /// <returns>True if the retention period is too long; otherwise, false.</returns>
        private bool IsRetentionPeriodTooLong(string retentionPeriod)
        {
            // In a real implementation, this would parse the retention period and compare it to benchmarks
            // For this implementation, we'll just check for some keywords
            return retentionPeriod.ToLower().Contains("year") && 
                   (retentionPeriod.Contains("10") || 
                    retentionPeriod.Contains("20") || 
                    retentionPeriod.Contains("30") ||
                    retentionPeriod.ToLower().Contains("decade"));
        }

        /// <summary>
        /// Checks if a country or region is outside the EU/EEA.
        /// </summary>
        /// <param name="country">The country or region to check.</param>
        /// <returns>True if the country is outside the EU/EEA; otherwise, false.</returns>
        private bool IsOutsideEU(string country)
        {
            var euCountries = new[]
            {
                "austria", "belgium", "bulgaria", "croatia", "cyprus", "czech", "denmark", "estonia",
                "finland", "france", "germany", "greece", "hungary", "ireland", "italy", "latvia",
                "lithuania", "luxembourg", "malta", "netherlands", "poland", "portugal", "romania",
                "slovakia", "slovenia", "spain", "sweden", "iceland", "liechtenstein", "norway"
            };

            return !euCountries.Any(c => country.ToLower().Contains(c));
        }

        /// <summary>
        /// Checks if a transfer mechanism is valid under GDPR.
        /// </summary>
        /// <param name="mechanism">The transfer mechanism to check.</param>
        /// <returns>True if the transfer mechanism is valid; otherwise, false.</returns>
        private bool IsValidTransferMechanism(string mechanism)
        {
            var validMechanisms = new[]
            {
                "adequacy decision",
                "sccs",
                "standard contractual clauses",
                "bcrs",
                "binding corporate rules",
                "explicit consent",
                "contract performance",
                "public interest",
                "legal claims"
            };

            return validMechanisms.Any(m => mechanism.ToLower().Contains(m));
        }

        /// <summary>
        /// Gets the numeric rank of a severity level for sorting.
        /// </summary>
        /// <param name="severity">The severity level.</param>
        /// <returns>The numeric rank of the severity level.</returns>
        private int GetSeverityRank(string severity)
        {
            return severity?.ToLower() switch
            {
                "critical" => 4,
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            };
        }

        #endregion
    }

    /// <summary>
    /// Represents an assessment of a specific area of GDPR compliance.
    /// </summary>
    public class ComplianceAssessment
    {
        /// <summary>
        /// The area being assessed (e.g., "Legal Basis", "Data Minimization").
        /// </summary>
        public string Area { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the area is compliant with GDPR requirements.
        /// </summary>
        public bool IsCompliant { get; set; }

        /// <summary>
        /// A list of compliance issues identified during the assessment.
        /// </summary>
        public List<ComplianceIssue> Issues { get; set; } = new List<ComplianceIssue>();
    }

    /// <summary>
    /// Interface for a registry of data processing activities.
    /// </summary>
    public interface IDataProcessingRegistry
    {
        /// <summary>
        /// Gets personal data for a data subject.
        /// </summary>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A dictionary of personal data.</returns>
        Task<Dictionary<string, object>> GetPersonalDataAsync(string subjectId, string tenantId);

        /// <summary>
        /// Updates personal data for a data subject.
        /// </summary>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="updates">The updates to apply.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdatePersonalDataAsync(string subjectId, string tenantId, Dictionary<string, object> updates);

        /// <summary>
        /// Deletes personal data for a data subject.
        /// </summary>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeletePersonalDataAsync(string subjectId, string tenantId);

        /// <summary>
        /// Gets portable data for a data subject.
        /// </summary>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="format">The format for the portable data.</param>
        /// <returns>A dictionary of portable data.</returns>
        Task<Dictionary<string, object>> GetPortableDataAsync(string subjectId, string tenantId, string format);

        /// <summary>
        /// Registers an objection to a processing activity.
        /// </summary>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="processingActivity">The processing activity being objected to.</param>
        /// <returns>True if the objection was registered successfully; otherwise, false.</returns>
        Task<bool> RegisterObjectionAsync(string subjectId, string tenantId, string processingActivity);
    }
}
