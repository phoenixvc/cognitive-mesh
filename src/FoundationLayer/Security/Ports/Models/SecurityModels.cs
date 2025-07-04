using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CognitiveMesh.FoundationLayer.Security.Ports.Models
{
    /// <summary>
    /// Represents the result of an authentication operation
    /// </summary>
    public class AuthenticationResult
    {
        public bool IsAuthenticated { get; set; }
        public string UserId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string Error { get; set; }
    }

    /// <summary>
    /// Represents the result of an authorization operation
    /// </summary>
    public class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Represents a security policy
    /// </summary>
    public class SecurityPolicy
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PolicyType { get; set; }
        public JsonDocument PolicyDocument { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents a compliance report
    /// </summary>
    public class ComplianceReport
    {
        public string ReportId { get; set; }
        public string ReportType { get; set; }
        public DateTimeOffset GeneratedAt { get; set; }
        public JsonDocument ReportData { get; set; }
    }
}
