using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CognitiveMesh.BusinessApplications.Common.Models
{
    /// <summary>
    /// Standard error envelope for all APIs, providing consistent error reporting
    /// and correlation capabilities across the Cognitive Mesh platform.
    /// </summary>
    public class ErrorEnvelope
    {
        /// <summary>
        /// A machine-readable error code that uniquely identifies the error type.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// A human-readable error message suitable for end users.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// A detailed description of the error, typically for developers or logging.
        /// </summary>
        public string? DetailedMessage { get; set; }

        /// <summary>
        /// The component or service that generated the error.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// The timestamp when the error occurred.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// An optional correlation ID for tracing related events across the system.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Optional additional details about the error.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? AdditionalDetails { get; set; }

        /// <summary>
        /// Creates a new instance of the ErrorEnvelope class.
        /// </summary>
        public ErrorEnvelope()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Creates a new instance of the ErrorEnvelope class with the specified parameters.
        /// </summary>
        /// <param name="errorCode">A machine-readable error code.</param>
        /// <param name="errorMessage">A human-readable error message.</param>
        /// <param name="detailedMessage">A detailed description of the error.</param>
        /// <param name="source">The component or service that generated the error.</param>
        /// <param name="correlationId">An optional correlation ID for tracing.</param>
        /// <param name="additionalDetails">Optional additional details about the error.</param>
        public ErrorEnvelope(
            string? errorCode,
            string? errorMessage,
            string? detailedMessage,
            string? source,
            string? correlationId = null,
            Dictionary<string, object>? additionalDetails = null)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            DetailedMessage = detailedMessage;
            Source = source;
            Timestamp = DateTimeOffset.UtcNow;
            CorrelationId = correlationId;
            AdditionalDetails = additionalDetails;
        }

        #region Factory Methods for Compliance Errors

        /// <summary>
        /// Creates an error envelope for a data subject rights violation (GDPR Article 12-22).
        /// </summary>
        /// <param name="rightType">The type of data subject right that was violated (e.g., "Access", "Erasure", "Portability").</param>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="reason">The reason for the violation.</param>
        /// <param name="source">The component or service that generated the error.</param>
        /// <param name="correlationId">An optional correlation ID for tracing.</param>
        /// <returns>A new ErrorEnvelope instance.</returns>
        public static ErrorEnvelope DataSubjectRightsViolation(
            string rightType,
            string subjectId,
            string reason,
            string source,
            string? correlationId = null)
        {
            var details = new Dictionary<string, object>
            {
                { "RightType", rightType },
                { "SubjectId", subjectId },
                { "RegulatoryReference", "GDPR Articles 12-22" }
            };

            return new ErrorEnvelope(
                errorCode: "GDPR.DSR.Violation",
                errorMessage: $"Data subject right violation: {rightType}",
                detailedMessage: reason,
                source: source,
                correlationId: correlationId,
                additionalDetails: details);
        }

        /// <summary>
        /// Creates an error envelope for a missing required consent scenario.
        /// </summary>
        /// <param name="consentType">The type of consent that was missing.</param>
        /// <param name="subjectId">The ID of the data subject.</param>
        /// <param name="processingOperation">The operation that required consent.</param>
        /// <param name="source">The component or service that generated the error.</param>
        /// <param name="correlationId">An optional correlation ID for tracing.</param>
        /// <returns>A new ErrorEnvelope instance.</returns>
        public static ErrorEnvelope MissingRequiredConsent(
            string consentType,
            string subjectId,
            string processingOperation,
            string source,
            string? correlationId = null)
        {
            var details = new Dictionary<string, object>
            {
                { "ConsentType", consentType },
                { "SubjectId", subjectId },
                { "ProcessingOperation", processingOperation },
                { "RegulatoryReference", "GDPR Article 6(1)(a)" }
            };

            return new ErrorEnvelope(
                errorCode: "GDPR.Consent.Missing",
                errorMessage: $"Missing required consent: {consentType}",
                detailedMessage: $"The operation '{processingOperation}' requires explicit consent of type '{consentType}' which was not provided by subject '{subjectId}'.",
                source: source,
                correlationId: correlationId,
                additionalDetails: details);
        }

        /// <summary>
        /// Creates an error envelope for an insufficient legal basis scenario.
        /// </summary>
        /// <param name="processingActivity">The processing activity that lacks a legal basis.</param>
        /// <param name="attemptedLegalBasis">The legal basis that was attempted but found insufficient.</param>
        /// <param name="reason">The reason why the legal basis was insufficient.</param>
        /// <param name="source">The component or service that generated the error.</param>
        /// <param name="correlationId">An optional correlation ID for tracing.</param>
        /// <returns>A new ErrorEnvelope instance.</returns>
        public static ErrorEnvelope InsufficientLegalBasis(
            string processingActivity,
            string attemptedLegalBasis,
            string reason,
            string source,
            string? correlationId = null)
        {
            var details = new Dictionary<string, object>
            {
                { "ProcessingActivity", processingActivity },
                { "AttemptedLegalBasis", attemptedLegalBasis },
                { "RegulatoryReference", "GDPR Article 6" }
            };

            return new ErrorEnvelope(
                errorCode: "GDPR.LegalBasis.Insufficient",
                errorMessage: $"Insufficient legal basis for processing: {attemptedLegalBasis}",
                detailedMessage: reason,
                source: source,
                correlationId: correlationId,
                additionalDetails: details);
        }

        /// <summary>
        /// Creates an error envelope for a cross-border data transfer violation.
        /// </summary>
        /// <param name="dataType">The type of data being transferred.</param>
        /// <param name="sourceCountry">The source country/region.</param>
        /// <param name="destinationCountry">The destination country/region.</param>
        /// <param name="reason">The reason why the transfer is not permitted.</param>
        /// <param name="source">The component or service that generated the error.</param>
        /// <param name="correlationId">An optional correlation ID for tracing.</param>
        /// <returns>A new ErrorEnvelope instance.</returns>
        public static ErrorEnvelope CrossBorderTransferViolation(
            string dataType,
            string sourceCountry,
            string destinationCountry,
            string reason,
            string source,
            string? correlationId = null)
        {
            var details = new Dictionary<string, object>
            {
                { "DataType", dataType },
                { "SourceCountry", sourceCountry },
                { "DestinationCountry", destinationCountry },
                { "RegulatoryReference", "GDPR Chapter V (Articles 44-50)" }
            };

            return new ErrorEnvelope(
                errorCode: "GDPR.Transfer.Violation",
                errorMessage: $"Cross-border data transfer violation: {sourceCountry} to {destinationCountry}",
                detailedMessage: reason,
                source: source,
                correlationId: correlationId,
                additionalDetails: details);
        }

        /// <summary>
        /// Creates an error envelope for a general regulatory non-compliance scenario.
        /// </summary>
        /// <param name="regulation">The regulation that was violated (e.g., "GDPR", "EU AI Act").</param>
        /// <param name="article">The specific article or section of the regulation.</param>
        /// <param name="description">A description of the non-compliance.</param>
        /// <param name="severity">The severity of the non-compliance (e.g., "Low", "Medium", "High", "Critical").</param>
        /// <param name="source">The component or service that generated the error.</param>
        /// <param name="correlationId">An optional correlation ID for tracing.</param>
        /// <returns>A new ErrorEnvelope instance.</returns>
        public static ErrorEnvelope RegulatoryNonCompliance(
            string regulation,
            string article,
            string description,
            string severity,
            string source,
            string? correlationId = null)
        {
            var details = new Dictionary<string, object>
            {
                { "Regulation", regulation },
                { "Article", article },
                { "Severity", severity },
                { "RegulatoryReference", $"{regulation} {article}" }
            };

            return new ErrorEnvelope(
                errorCode: $"{regulation.Replace(" ", "")}.NonCompliance",
                errorMessage: $"{regulation} non-compliance: {article}",
                detailedMessage: description,
                source: source,
                correlationId: correlationId,
                additionalDetails: details);
        }

        #endregion
    }
}
