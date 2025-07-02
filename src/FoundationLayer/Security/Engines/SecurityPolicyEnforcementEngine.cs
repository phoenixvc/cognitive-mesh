using CognitiveMesh.FoundationLayer.Security.Ports;
using CognitiveMesh.FoundationLayer.Security.Ports.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CognitiveMesh.FoundationLayer.Security.Engines
{
    /// <summary>
    /// Implements the core Zero-Trust security policy evaluation and enforcement logic.
    /// This engine is a pure domain component that depends only on ports and abstractions.
    /// </summary>
    public class SecurityPolicyEnforcementEngine : ISecurityPolicyPort
    {
        private readonly ILogger<SecurityPolicyEnforcementEngine> _logger;
        private readonly IConfiguration _configuration;

        public SecurityPolicyEnforcementEngine(
            ILogger<SecurityPolicyEnforcementEngine> logger,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public async Task<AuthenticationVerificationResponse> VerifyAuthenticationAsync(AuthenticationVerificationRequest request)
        {
            _logger.LogInformation("Verifying authentication token of type '{TokenType}'", request.TokenType);

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return new AuthenticationVerificationResponse { IsAuthenticated = false, ErrorMessage = "Token cannot be null or empty." };
            }

            if (!request.TokenType.Equals("JWT", StringComparison.OrdinalIgnoreCase))
            {
                return new AuthenticationVerificationResponse { IsAuthenticated = false, ErrorMessage = "Unsupported token type." };
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out SecurityToken validatedToken);

                var claims = new List<string>();
                foreach (var claim in principal.Claims)
                {
                    claims.Add($"{claim.Type}: {claim.Value}");
                }

                _logger.LogInformation("Successfully authenticated subject '{SubjectId}'", principal.Identity.Name);

                return new AuthenticationVerificationResponse
                {
                    IsAuthenticated = true,
                    SubjectId = principal.Identity.Name,
                    Claims = claims
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Authentication token validation failed.");
                return new AuthenticationVerificationResponse { IsAuthenticated = false, ErrorMessage = ex.Message };
            }
        }

        /// <inheritdoc />
        public async Task<AuthorizationResponse> AuthorizeRequestAsync(AuthorizationRequest request)
        {
            _logger.LogInformation("Authorizing action '{Action}' on resource '{ResourceId}' for subject '{SubjectId}'", request.Action, request.ResourceId, request.SubjectId);

            // This is a simplified example of RBAC/ABAC logic.
            // In a real system, this would use a more sophisticated policy engine (like OPA).
            bool isAuthorized = false;
            string reason = "Access denied by default policy.";

            // Example Policy: Only users with the 'Admin' role can delete resources.
            if (request.Action.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                if (request.SubjectClaims.Contains("role: Admin"))
                {
                    isAuthorized = true;
                    reason = "Authorized: Subject has 'Admin' role required for delete actions.";
                }
                else
                {
                    reason = "Denied: Subject lacks 'Admin' role required for delete actions.";
                }
            }
            // Example Policy: Any authenticated user can read.
            else if (request.Action.Equals("read", StringComparison.OrdinalIgnoreCase))
            {
                isAuthorized = true;
                reason = "Authorized: Read access is granted to all authenticated subjects.";
            }
            else
            {
                // More complex policies would go here.
                isAuthorized = false;
                reason = $"Denied: No policy found to authorize action '{request.Action}'.";
            }

            if (isAuthorized)
            {
                _logger.LogInformation("Authorization successful for subject '{SubjectId}'. Reason: {Reason}", request.SubjectId, reason);
            }
            else
            {
                _logger.LogWarning("Authorization failed for subject '{SubjectId}'. Reason: {Reason}", request.SubjectId, reason);
            }

            return new AuthorizationResponse
            {
                IsAuthorized = isAuthorized,
                Reason = reason
            };
        }

        /// <inheritdoc />
        public async Task<PolicyValidationResponse> ValidatePolicyAsync(PolicyValidationRequest request)
        {
            _logger.LogInformation("Validating security policy of type '{PolicyType}'", request.PolicyType);
            var response = new PolicyValidationResponse { IsValid = false };

            if (string.IsNullOrWhiteSpace(request.PolicyDocument))
            {
                response.Errors.Add("Policy document cannot be null or empty.");
                return response;
            }

            // Simple validation based on type. A real implementation would use a proper parser/validator.
            if (request.PolicyType.Equals("JSON", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    JsonDocument.Parse(request.PolicyDocument);
                    response.IsValid = true;
                    _logger.LogInformation("Policy document is valid JSON.");
                }
                catch (JsonException ex)
                {
                    response.Errors.Add($"Invalid JSON: {ex.Message}");
                    _logger.LogWarning("Policy validation failed: Invalid JSON. Error: {ErrorMessage}", ex.Message);
                }
            }
            else
            {
                response.Errors.Add($"Unsupported policy type: {request.PolicyType}");
                _logger.LogWarning("Policy validation failed: Unsupported policy type '{PolicyType}'", request.PolicyType);
            }

            return response;
        }

        /// <inheritdoc />
        public async Task<ComplianceReportResponse> GenerateComplianceReportAsync(ComplianceReportRequest request)
        {
            _logger.LogInformation("Generating compliance report of type '{ReportType}' for period {StartDate} to {EndDate}", request.ReportType, request.StartDate, request.EndDate);

            // This is a mock implementation. A real system would query an audit log or data warehouse.
            string reportData;
            switch (request.ReportType)
            {
                case "GDPR-Access-Log":
                    reportData = JsonSerializer.Serialize(new
                    {
                        reportTitle = "GDPR Data Access Log",
                        period = $"{request.StartDate:o} to {request.EndDate:o}",
                        entries = new[]
                        {
                            new { timestamp = DateTime.UtcNow.AddDays(-1), subjectId = "user-123", resourceId = "pii-record-abc", action = "read", justification = "Customer service request #456" },
                            new { timestamp = DateTime.UtcNow.AddHours(-5), subjectId = "admin-456", resourceId = "pii-record-xyz", action = "delete", justification = "User account deletion request #789" }
                        }
                    });
                    break;
                case "Least-Privilege-Violations":
                    reportData = JsonSerializer.Serialize(new
                    {
                        reportTitle = "Least Privilege Violation Report",
                        period = $"{request.StartDate:o} to {request.EndDate:o}",
                        violations = new[]
                        {
                            new { timestamp = DateTime.UtcNow.AddDays(-2), subjectId = "user-789", attemptedAction = "delete", resourceId = "system-config", outcome = "Denied" }
                        }
                    });
                    break;
                default:
                    _logger.LogWarning("Unknown compliance report type requested: '{ReportType}'", request.ReportType);
                    return null; // Or throw a specific exception
            }

            _logger.LogInformation("Successfully generated compliance report '{ReportType}'", request.ReportType);

            return new ComplianceReportResponse
            {
                ReportData = reportData,
                GeneratedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
