using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CognitiveMesh.BusinessApplications.Common.Models;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.Compliance.Ports.Models
{
    #region Data Models

    /// <summary>
    /// Represents a request to check compliance against a specific sectoral regulation.
    /// </summary>
    public class SectoralComplianceCheckRequest
    {
        /// <summary>
        /// The name of the regulation to check against (e.g., "HIPAA", "MiFID II").
        /// This is used to dispatch the request to the correct compliance module.
        /// </summary>
        public string Regulation { get; set; }

        /// <summary>
        /// The type of action being assessed (e.g., "DataStorage", "DataSharing", "AutomatedDecision").
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// The data involved in the action, which will be assessed by the compliance module.
        /// </summary>
        public Dictionary<string, object> DataContext { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The tenant ID under which the compliance check is being performed.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// A unique identifier for tracing this request across systems.
        /// </summary>
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the result of a compliance assessment.
    /// </summary>
    public class ComplianceAssessmentResult
    {
        /// <summary>
        /// Indicates whether the assessed action is compliant with the specified regulation.
        /// </summary>
        public bool IsCompliant { get; set; }

        /// <summary>
        /// The regulation against which the assessment was performed.
        /// </summary>
        public string Regulation { get; set; }

        /// <summary>
        /// A list of compliance issues identified during the assessment. This list is empty if IsCompliant is true.
        /// </summary>
        public List<ComplianceIssue> Issues { get; set; } = new List<ComplianceIssue>();

        /// <summary>
        /// A list of recommended actions to achieve compliance or improve the compliance posture.
        /// </summary>
        public List<string> Recommendations { get; set; } = new List<string>();

        /// <summary>
        /// The timestamp when the assessment was completed.
        /// </summary>
        public DateTimeOffset AssessedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Represents a single compliance issue identified during an assessment.
    /// </summary>
    public class ComplianceIssue
    {
        /// <summary>
        /// A unique code for the identified issue.
        /// </summary>
        public string IssueCode { get; set; }

        /// <summary>
        /// A description of the compliance issue.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The severity of the issue (e.g., "Low", "Medium", "High", "Critical").
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// A reference to the specific article or section of the regulation that was violated.
        /// </summary>
        public string RegulatoryReference { get; set; }
    }

    #endregion
}

namespace CognitiveMesh.BusinessApplications.Compliance.Ports
{
    using CognitiveMesh.BusinessApplications.Compliance.Ports.Models;

    #region Interfaces

    /// <summary>
    /// Defines the contract for a pluggable compliance module for a specific sector or regulation.
    /// Each implementation of this interface handles the compliance logic for one regulation (e.g., HIPAA, MiFID II).
    /// </summary>
    public interface ISectoralComplianceModule
    {
        /// <summary>
        /// Gets the name of the regulation supported by this module (e.g., "HIPAA").
        /// This name is used to register and dispatch requests.
        /// </summary>
        string SupportedRegulation { get; }

        /// <summary>
        /// Assesses a given action and its context for compliance with the specific regulation.
        /// </summary>
        /// <param name="request">The compliance check request containing the action and data context.</param>
        /// <returns>A task that resolves to a <see cref="ComplianceAssessmentResult"/>.</returns>
        Task<ComplianceAssessmentResult> AssessComplianceAsync(SectoralComplianceCheckRequest request);
    }

    /// <summary>
    /// Defines the contract for the Sectoral Regulations Framework Port.
    /// This port is the main entry point for performing compliance checks against various industry-specific regulations.
    /// </summary>
    public interface ISectoralRegulationsFrameworkPort
    {
        /// <summary>
        /// Assesses a given action for compliance with a specific sectoral regulation.
        /// The framework dispatches the request to the appropriate registered compliance module.
        /// </summary>
        /// <param name="request">The compliance check request.</param>
        /// <returns>The result of the compliance assessment.</returns>
        Task<ComplianceAssessmentResult> AssessComplianceAsync(SectoralComplianceCheckRequest request);

        /// <summary>
        /// Gets a list of all sectoral regulations supported by the currently loaded compliance modules.
        /// </summary>
        /// <returns>An enumerable collection of supported regulation names.</returns>
        IEnumerable<string> GetSupportedRegulations();
    }

    #endregion
}

namespace CognitiveMesh.BusinessApplications.Compliance.Adapters
{
    using CognitiveMesh.BusinessApplications.Compliance.Ports;
    using CognitiveMesh.BusinessApplications.Compliance.Ports.Models;

    /// <summary>
    /// Provides a pluggable framework for managing and dispatching compliance checks
    /// to various industry-specific compliance modules. This class acts as a central registry
    /// and dispatcher, making the system extensible to new regulations without core code changes.
    /// </summary>
    public class SectoralRegulationsFramework : ISectoralRegulationsFrameworkPort
    {
        private readonly ILogger<SectoralRegulationsFramework> _logger;
        private readonly ConcurrentDictionary<string, ISectoralComplianceModule> _modules;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectoralRegulationsFramework"/> class.
        /// It discovers all registered <see cref="ISectoralComplianceModule"/> implementations
        /// via dependency injection and builds a registry for dispatching.
        /// </summary>
        /// <param name="complianceModules">An enumeration of all registered compliance modules, provided by DI.</param>
        /// <param name="logger">The logger for this framework.</param>
        public SectoralRegulationsFramework(
            IEnumerable<ISectoralComplianceModule> complianceModules,
            ILogger<SectoralRegulationsFramework> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modules = new ConcurrentDictionary<string, ISectoralComplianceModule>(StringComparer.OrdinalIgnoreCase);

            if (complianceModules == null)
            {
                _logger.LogWarning("No sectoral compliance modules were provided to the framework.");
                return;
            }

            foreach (var module in complianceModules)
            {
                if (string.IsNullOrWhiteSpace(module.SupportedRegulation))
                {
                    _logger.LogWarning("A compliance module of type {ModuleType} has a null or empty SupportedRegulation property and will be ignored.", module.GetType().Name);
                    continue;
                }

                if (!_modules.TryAdd(module.SupportedRegulation, module))
                {
                    _logger.LogWarning("A compliance module for regulation '{Regulation}' is already registered. The module of type {ModuleType} will be ignored.",
                        module.SupportedRegulation, module.GetType().Name);
                }
                else
                {
                    _logger.LogInformation("Successfully registered compliance module for regulation: {Regulation}", module.SupportedRegulation);
                }
            }
        }

        /// <inheritdoc />
        public async Task<ComplianceAssessmentResult> AssessComplianceAsync(SectoralComplianceCheckRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Regulation))
            {
                _logger.LogWarning("AssessComplianceAsync called with a null or invalid request.");
                return new ComplianceAssessmentResult
                {
                    IsCompliant = false,
                    Regulation = request?.Regulation ?? "Unknown",
                    Issues = { new ComplianceIssue { IssueCode = "InvalidRequest", Description = "The compliance check request was null or missing a regulation identifier." } }
                };
            }

            if (_modules.TryGetValue(request.Regulation, out var module))
            {
                _logger.LogInformation("Dispatching compliance check for regulation '{Regulation}' to module {ModuleType}.",
                    request.Regulation, module.GetType().Name);
                try
                {
                    return await module.AssessComplianceAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred in the '{Regulation}' compliance module while assessing action '{ActionType}'.",
                        request.Regulation, request.ActionType);
                    return new ComplianceAssessmentResult
                    {
                        IsCompliant = false,
                        Regulation = request.Regulation,
                        Issues = { new ComplianceIssue { IssueCode = "ModuleExecutionError", Description = $"The compliance module for '{request.Regulation}' encountered an internal error." } }
                    };
                }
            }
            else
            {
                _logger.LogWarning("No compliance module found for regulation '{Regulation}'. The action will be treated as non-compliant by default.", request.Regulation);
                return new ComplianceAssessmentResult
                {
                    IsCompliant = false,
                    Regulation = request.Regulation,
                    Issues = { new ComplianceIssue { IssueCode = "UnsupportedRegulation", Description = $"No compliance module is registered to handle the '{request.Regulation}' regulation." } }
                };
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetSupportedRegulations()
        {
            return _modules.Keys.ToList();
        }
    }
}
