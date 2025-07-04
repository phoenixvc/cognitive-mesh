using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CognitiveMesh.FoundationLayer.Security.Engines;
using CognitiveMesh.FoundationLayer.Security.Ports;
using CognitiveMesh.FoundationLayer.Security.Ports.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace CognitiveMesh.FoundationLayer.Security.Tests.Engines
{
    public class SecurityPolicyEnforcementEngineTests : IDisposable
    {
        private readonly Mock<ILogger<SecurityPolicyEnforcementEngine>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly SecurityPolicyEnforcementEngine _engine;
        private readonly string _testJwtKey;
        private readonly string _testIssuer;
        private readonly string _testAudience;

        public SecurityPolicyEnforcementEngineTests()
        {
            _mockLogger = new Mock<ILogger<SecurityPolicyEnforcementEngine>>();
            _mockConfig = new Mock<IConfiguration>();
            
            // Setup test JWT configuration
            _testJwtKey = "ThisIsASecretKeyForTestingPurposesOnly";
            _testIssuer = "test-issuer";
            _testAudience = "test-audience";
            
            // Setup configuration values directly on the mock
            _mockConfig.Setup(x => x["Jwt:Key"]).Returns(_testJwtKey);
            _mockConfig.Setup(x => x["Jwt:Issuer"]).Returns(_testIssuer);
            _mockConfig.Setup(x => x["Jwt:Audience"]).Returns(_testAudience);
            
            // Setup GetSection to return the same mock for any section
            _mockConfig.Setup(x => x.GetSection(It.IsAny<string>()))
                .Returns<string>(section => {
                    var sectionMock = new Mock<IConfigurationSection>();
                    sectionMock.Setup(x => x[It.IsAny<string>()])
                        .Returns<string>(key => _mockConfig.Object[$"{section}:{key}"]);
                    sectionMock.Setup(x => x.Value).Returns(_mockConfig.Object[section]);
                    return sectionMock.Object;
                });
            
            _engine = new SecurityPolicyEnforcementEngine(_mockLogger.Object, _mockConfig.Object);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        private string GenerateTestJwtToken(string subject, string role = null, DateTime? expires = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_testJwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                { 
                    new Claim(ClaimTypes.Name, subject),
                    new Claim(ClaimTypes.Role, role ?? "User")
                }),
                Expires = expires ?? DateTime.UtcNow.AddHours(1),
                Issuer = _testIssuer,
                Audience = _testAudience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Fact]
        public async Task VerifyAuthenticationAsync_WithValidToken_ReturnsAuthenticated()
        {
            // Diagnostic output
            Console.WriteLine("Starting VerifyAuthenticationAsync_WithValidToken_ReturnsAuthenticated test");
            
            try 
            {
                // Arrange
                Console.WriteLine("Generating test JWT token...");
                var token = GenerateTestJwtToken("test-user");
                var request = new AuthenticationVerificationRequest 
                { 
                    Token = token, 
                    TokenType = "JWT" 
                };
                Console.WriteLine("Test JWT token generated successfully");

            // Act
            var result = await _engine.VerifyAuthenticationAsync(request);

                // Assert
                Console.WriteLine("Verifying test assertions...");
                result.Should().NotBeNull();
                result.IsAuthenticated.Should().BeTrue();
                result.SubjectId.Should().Be("test-user");
                result.Claims.Should().NotBeEmpty();
                
                // Verify logging
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Verifying authentication token")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
                
                Console.WriteLine("Test VerifyAuthenticationAsync_WithValidToken_ReturnsAuthenticated completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed with exception: {ex}");
                throw;
            }
        }

        [Fact]
        public async Task VerifyAuthenticationAsync_WithExpiredToken_ReturnsNotAuthenticated()
        {
            // Arrange
            var expiredToken = GenerateTestJwtToken("test-user", expires: DateTime.UtcNow.AddHours(-1));
            var request = new AuthenticationVerificationRequest 
            { 
                Token = expiredToken, 
                TokenType = "JWT" 
            };

            // Act
            var result = await _engine.VerifyAuthenticationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsAuthenticated.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task AuthorizeRequestAsync_WithAdminRole_AllowsDeleteAction()
        {
            // Arrange
            var request = new AuthorizationRequest
            {
                SubjectId = "admin-1",
                Action = "delete",
                ResourceId = "resource-123",
                SubjectClaims = new List<string> { "role: Admin" }
            };

            // Act
            var result = await _engine.AuthorizeRequestAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsAuthorized.Should().BeTrue();
            result.Reason.Should().Contain("Authorized: Subject has 'Admin' role");
        }

        [Fact]
        public async Task AuthorizeRequestAsync_WithoutAdminRole_DeniesDeleteAction()
        {
            // Arrange
            var request = new AuthorizationRequest
            {
                SubjectId = "user-1",
                Action = "delete",
                ResourceId = "resource-123",
                SubjectClaims = new List<string> { "role: User" }
            };

            // Act
            var result = await _engine.AuthorizeRequestAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsAuthorized.Should().BeFalse();
            result.Reason.Should().Contain("lacks 'Admin' role");
        }

        [Fact]
        public async Task ValidatePolicyAsync_WithValidJson_ReturnsValid()
        {
            // Arrange
            var policy = "{ \"policy\": { \"allow\": [\"read\"] } }";
            var request = new PolicyValidationRequest
            {
                PolicyType = "JSON",
                PolicyDocument = policy
            };

            // Act
            var result = await _engine.ValidatePolicyAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidatePolicyAsync_WithInvalidJson_ReturnsError()
        {
            // Arrange
            var invalidPolicy = "{ invalid: json }";
            var request = new PolicyValidationRequest
            {
                PolicyType = "JSON",
                PolicyDocument = invalidPolicy
            };

            // Act
            var result = await _engine.ValidatePolicyAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors[0].Should().Contain("Invalid JSON");
        }

        [Fact]
        public async Task GenerateComplianceReportAsync_ForGdprReport_ReturnsValidReport()
        {
            // Arrange
            var request = new ComplianceReportRequest
            {
                ReportType = "GDPR-Access-Log",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            // Act
            var result = await _engine.GenerateComplianceReportAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.ReportData.Should().NotBeNullOrEmpty();
            result.ReportData.Should().Contain("GDPR Data Access Log");
            
            // Verify the report contains valid JSON
            var report = JsonSerializer.Deserialize<JsonElement>(result.ReportData);
            report.GetProperty("reportTitle").GetString().Should().Be("GDPR Data Access Log");
            report.GetProperty("entries").GetArrayLength().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GenerateComplianceReportAsync_ForLeastPrivilegeReport_ReturnsValidReport()
        {
            // Arrange
            var request = new ComplianceReportRequest
            {
                ReportType = "Least-Privilege-Violations",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            // Act
            var result = await _engine.GenerateComplianceReportAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.ReportData.Should().NotBeNullOrEmpty();
            result.ReportData.Should().Contain("Least Privilege Violation Report");
            
            // Verify the report contains valid JSON
            var report = JsonSerializer.Deserialize<JsonElement>(result.ReportData);
            report.GetProperty("reportTitle").GetString().Should().Be("Least Privilege Violation Report");
            report.GetProperty("violations").GetArrayLength().Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public async Task GenerateComplianceReportAsync_WithInvalidReportType_ReturnsEmptyReport()
        {
            // Arrange
            var request = new ComplianceReportRequest
            {
                ReportType = "Invalid-Report-Type",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };

            // Act
            var result = await _engine.GenerateComplianceReportAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.ReportData.Should().NotBeNull();
            result.ReportData.Should().Be("{\"error\":\"Unsupported report type: Invalid-Report-Type\"}");
        }
    }
}
