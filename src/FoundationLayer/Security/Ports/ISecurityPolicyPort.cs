using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.FoundationLayer.Security.Ports.Models
{
    /// <summary>
    /// Represents a request to verify an authentication credential.
    /// </summary>
    public class AuthenticationVerificationRequest
    {
        /// <summary>
        /// The authentication token (e.g., JWT, API Key).
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The type of the token provided (e.g., "JWT", "APIKey").
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// Additional context for the authentication attempt (e.g., IP address, device ID).
        /// </summary>
        public Dictionary<string, string> Context { get; set; } = new();
    }

    /// <summary>
    /// Represents the result of an authentication verification.
    /// </summary>
    public class AuthenticationVerificationResponse
    {
        public bool IsAuthenticated { get; set; }
        public string SubjectId { get; set; }
        public List<string> Claims { get; set; } = new();
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents a request to authorize an action based on Zero-Trust principles.
    /// </summary>
    public class AuthorizationRequest
    {
        /// <summary>
        /// The unique identifier of the subject (user, service, agent) requesting the action.
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// The claims associated with the subject.
        /// </summary>
        public List<string> SubjectClaims { get; set; } = new();

        /// <summary>
        /// The action the subject is attempting to perform (e.g., "read", "write", "delete").
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The unique identifier of the resource being accessed.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Additional context for the authorization request (e.g., time of day, location).
        /// </summary>
        public Dictionary<string, string> Context { get; set; } = new();
    }

    /// <summary>
    /// Represents the result of an authorization decision.
    /// </summary>
    public class AuthorizationResponse
    {
        public bool IsAuthorized { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// Represents a request to validate a security policy document.
    /// </summary>
    public class PolicyValidationRequest
    {
        /// <summary>
        /// The content of the policy document (e.g., JSON, YAML, Rego).
        /// </summary>
        public string PolicyDocument { get; set; }

        /// <summary>
        /// The type of the policy being validated (e.g., "OPA", "CustomJson").
        /// </summary>
        public string PolicyType { get; set; }
    }

    /// <summary>
    /// Represents the result of a policy validation check.
    /// </summary>
    public class PolicyValidationResponse
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Represents a request to generate a compliance report.
    /// </summary>
    public class ComplianceReportRequest
    {
        /// <summary>
        /// The type of report to generate (e.g., "GDPR-Access-Log", "Least-Privilege-Violations").
        /// </summary>
        public string ReportType { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }

    /// <summary>
    /// Represents a generated compliance report.
    /// </summary>
    public class ComplianceReportResponse
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// The generated report data, typically in a structured format like JSON or CSV.
        /// </summary>
        public string ReportData { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
    }
}

namespace CognitiveMesh.FoundationLayer.Security.Ports
{
    using CognitiveMesh.FoundationLayer.Security.Ports.Models;

    /// <summary>
    /// Defines the contract for the Security Policy Port, which serves as the primary interface
    /// for all security policy evaluation, enforcement, and management within the Cognitive Mesh.
    /// This port is a cornerstone of the Zero-Trust Security Framework.
    /// </summary>
    public interface ISecurityPolicyPort
    {
        /// <summary>
        /// Verifies an authentication credential against the current security policies.
        /// This is a key step in the Zero-Trust "never trust, always verify" principle.
        /// </summary>
        /// <param name="request">The request containing the authentication token and context.</param>
        /// <returns>A response indicating whether the credential is valid and the identity it represents.</returns>
        Task<AuthenticationVerificationResponse> VerifyAuthenticationAsync(AuthenticationVerificationRequest request);

        /// <summary>
        /// Authorizes a request from a subject to perform an action on a resource.
        /// This method enforces the principle of least privilege by dynamically evaluating access policies.
        /// </summary>
        /// <param name="request">The request containing details about the subject, action, and resource.</param>
        /// <returns>A response indicating whether the action is permitted and the reason for the decision.</returns>
        Task<AuthorizationResponse> AuthorizeRequestAsync(AuthorizationRequest request);

        /// <summary>
        /// Validates the syntax and semantics of a security policy document before it is stored or applied.
        /// </summary>
        /// <param name="request">The request containing the policy document to validate.</param>
        /// <returns>A response indicating whether the policy is valid and a list of any errors found.</returns>
        Task<PolicyValidationResponse> ValidatePolicyAsync(PolicyValidationRequest request);

        /// <summary>
        /// Generates a compliance report based on historical security event data.
        /// This is essential for auditing and demonstrating adherence to regulatory requirements.
        /// </summary>
        /// <param name="request">The request specifying the type of report and the time frame.</param>
        /// <returns>The generated compliance report.</returns>
        Task<ComplianceReportResponse> GenerateComplianceReportAsync(ComplianceReportRequest request);
    }
}
