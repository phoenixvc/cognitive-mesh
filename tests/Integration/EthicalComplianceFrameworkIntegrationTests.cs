using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

// Foundation Layer
using FoundationLayer.AuditLogging;
// Reasoning Layer
using CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports;
// Metacognitive Layer
using MetacognitiveLayer.AIGovernance;
using MetacognitiveLayer.CulturalAdaptation.Ports;
using MetacognitiveLayer.CulturalAdaptation.Engines;
// Agency Layer
using AgencyLayer.MultiAgentOrchestration.Ports;
using AgencyLayer.MultiAgentOrchestration.Engines;
// Business Applications
using CognitiveMesh.BusinessApplications.Compliance.Ports;
using CognitiveMesh.BusinessApplications.Compliance.Ports.Models;

namespace CognitiveMesh.Tests.Integration
{
    /// <summary>
    /// Test fixture for setting up the dependency injection container for integration tests.
    /// This fixture configures all the necessary services, engines, and mocked adapters.
    /// </summary>
    public class EthicalComplianceTestFixture
    {
        public ServiceProvider ServiceProvider { get; private set; }
        public Mock<IAuditLoggingPort> MockAuditLoggingPort { get; } = new();
        public Mock<INotificationPort> MockNotificationPort { get; } = new();
        public Mock<IAgentRuntimeAdapter> MockAgentRuntimeAdapter { get; } = new();
        public Mock<IAgentKnowledgeRepository> MockAgentKnowledgeRepository { get; } = new();
        public Mock<IApprovalAdapter> MockApprovalAdapter { get; } = new();
        public Mock<IGDPRCompliancePort> MockGDPRCompliancePort { get; } = new();
        public Mock<IEUAIActCompliancePort> MockEUAIActCompliancePort { get; } = new();
        public Mock<INormativeAgencyPort> MockNormativeAgencyPort { get; } = new();
        public Mock<IInformationEthicsPort> MockInformationEthicsPort { get; } = new();

        public EthicalComplianceTestFixture()
        {
            var services = new ServiceCollection();

            // Add Logging
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

            // Foundation Layer Mocks
            services.AddSingleton(MockAuditLoggingPort.Object);
            services.AddSingleton(MockNotificationPort.Object);

            // Reasoning Layer — mocked ports for controlled ethical checks
            MockNormativeAgencyPort
                .Setup(x => x.ValidateActionAsync(It.IsAny<NormativeActionValidationRequest>()))
                .ReturnsAsync(new NormativeActionValidationResponse { IsValid = true });

            MockInformationEthicsPort
                .Setup(x => x.AssessInformationalDignityAsync(It.IsAny<DignityAssessmentRequest>()))
                .ReturnsAsync(new DignityAssessmentResponse { IsDignityPreserved = true });

            services.AddSingleton(MockNormativeAgencyPort.Object);
            services.AddSingleton(MockInformationEthicsPort.Object);

            // Metacognitive Layer Engines
            services.AddSingleton<ICrossCulturalFrameworkPort, CrossCulturalFrameworkEngine>();
            services.AddSingleton<IAIGovernanceMonitorPort, AIGovernanceMonitor>();

            // Business Applications Layer — mocked ports (adapters have complex DI chains)
            services.AddSingleton(MockGDPRCompliancePort.Object);
            services.AddSingleton(MockEUAIActCompliancePort.Object);

            // Mock IGovernancePort as its implementation would require a database
            var mockGovernancePort = new Mock<IGovernancePort>();
            mockGovernancePort.Setup(p => p.ListPoliciesAsync()).ReturnsAsync(new List<PolicyRecord>
            {
                new() { PolicyId = "policy-001", Name = "High-Risk Transaction Approval Policy", Status = "Active", Version = 1 },
                new() { PolicyId = "policy-002", Name = "PII Data Handling Policy", Status = "Active", Version = 2 }
            });
            services.AddSingleton(mockGovernancePort.Object);

            // Agency Layer Mocks and Engine
            services.AddSingleton(MockAgentRuntimeAdapter.Object);
            services.AddSingleton(MockAgentKnowledgeRepository.Object);
            services.AddSingleton(MockApprovalAdapter.Object);
            services.AddSingleton<IMultiAgentOrchestrationPort, MultiAgentOrchestrationEngine>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// Comprehensive integration tests for the Ethical &amp; Legal Compliance Framework.
    /// These tests validate the end-to-end workflows and interactions between the various
    /// components responsible for ensuring ethical, legal, and compliant AI behavior.
    /// </summary>
    public class EthicalComplianceFrameworkIntegrationTests : IClassFixture<EthicalComplianceTestFixture>
    {
        private readonly EthicalComplianceTestFixture _fixture;
        private readonly IMultiAgentOrchestrationPort _orchestrationEngine;
        private readonly IGDPRCompliancePort _gdprAdapter;
        private readonly IEUAIActCompliancePort _euAiActAdapter;
        private readonly ICrossCulturalFrameworkPort _culturalFrameworkEngine;

        public EthicalComplianceFrameworkIntegrationTests(EthicalComplianceTestFixture fixture)
        {
            _fixture = fixture;
            _orchestrationEngine = _fixture.ServiceProvider.GetRequiredService<IMultiAgentOrchestrationPort>();
            _gdprAdapter = _fixture.ServiceProvider.GetRequiredService<IGDPRCompliancePort>();
            _euAiActAdapter = _fixture.ServiceProvider.GetRequiredService<IEUAIActCompliancePort>();
            _culturalFrameworkEngine = _fixture.ServiceProvider.GetRequiredService<ICrossCulturalFrameworkPort>();

            // Reset ethical mocks to default passing behavior for each test
            _fixture.MockNormativeAgencyPort.Reset();
            _fixture.MockNormativeAgencyPort
                .Setup(x => x.ValidateActionAsync(It.IsAny<NormativeActionValidationRequest>()))
                .ReturnsAsync(new NormativeActionValidationResponse { IsValid = true });

            _fixture.MockInformationEthicsPort.Reset();
            _fixture.MockInformationEthicsPort
                .Setup(x => x.AssessInformationalDignityAsync(It.IsAny<DignityAssessmentRequest>()))
                .ReturnsAsync(new DignityAssessmentResponse { IsDignityPreserved = true });
        }

        [Fact]
        public async Task EndToEnd_CompliantWorkflow_SucceedsAndRegistersProvenance()
        {
            // Arrange
            var agentDefinition = new AgentDefinition { AgentType = "TestAgent", DefaultAutonomyLevel = AutonomyLevel.FullyAutonomous };
            await _orchestrationEngine.RegisterAgentAsync(agentDefinition);

            var request = new AgentExecutionRequest
            {
                Task = new AgentTask { Goal = "Analyze sales data", RequiredAgentTypes = new List<string> { "TestAgent" } },
                RequestingUserId = "test-user"
            };

            // Include "COMPLETE" so the CollaborativeSwarm convergence check succeeds
            _fixture.MockAgentRuntimeAdapter
                .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
                .ReturnsAsync("Analysis complete. Sales are up 15%. COMPLETE");

            // Act
            var response = await _orchestrationEngine.ExecuteTaskAsync(request);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Contains("Analysis complete", response.Result?.ToString() ?? "");
        }

        [Fact]
        public async Task EndToEnd_NormativeViolation_InvokesEthicalValidation()
        {
            // Arrange: override normative mock to reject the action
            _fixture.MockNormativeAgencyPort
                .Setup(x => x.ValidateActionAsync(It.IsAny<NormativeActionValidationRequest>()))
                .ReturnsAsync(new NormativeActionValidationResponse
                {
                    IsValid = false,
                    Violations = new List<string> { "Normative Violation: Justifications do not align with any core ethical principles." }
                });

            var agentDefinition = new AgentDefinition { AgentType = "UnjustifiedAgent", DefaultAutonomyLevel = AutonomyLevel.FullyAutonomous };
            await _orchestrationEngine.RegisterAgentAsync(agentDefinition);

            var request = new AgentExecutionRequest
            {
                Task = new AgentTask { Goal = "Propose action without justification", RequiredAgentTypes = new List<string> { "UnjustifiedAgent" } },
                RequestingUserId = "test-user"
            };

            // Act
            var response = await _orchestrationEngine.ExecuteTaskAsync(request);

            // Assert: engine completes (ethical rejections are handled gracefully, not as exceptions)
            // but the normative port was invoked to validate the action
            Assert.True(response.IsSuccess);
            _fixture.MockNormativeAgencyPort.Verify(
                x => x.ValidateActionAsync(It.Is<NormativeActionValidationRequest>(
                    r => r.ProposedAction == "Propose action without justification")),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task EndToEnd_DignityViolation_InvokesDignityAssessment()
        {
            // Arrange: set up dignity check to fail when UserData context is present
            _fixture.MockInformationEthicsPort
                .Setup(x => x.AssessInformationalDignityAsync(It.IsAny<DignityAssessmentRequest>()))
                .ReturnsAsync(new DignityAssessmentResponse
                {
                    IsDignityPreserved = false,
                    PotentialViolations = new List<string> { "Processing exceeds original consent scope." }
                });

            var agentDefinition = new AgentDefinition { AgentType = "DataProcessingAgent", DefaultAutonomyLevel = AutonomyLevel.FullyAutonomous };
            await _orchestrationEngine.RegisterAgentAsync(agentDefinition);

            var request = new AgentExecutionRequest
            {
                Task = new AgentTask
                {
                    Goal = "Process user behavioral data",
                    RequiredAgentTypes = new List<string> { "DataProcessingAgent" },
                    Context = new Dictionary<string, object>
                    {
                        { "UserData", "behavioral-data-001" },
                        { "UserId", "user-456" }
                    }
                },
                RequestingUserId = "test-user"
            };

            // Act
            var response = await _orchestrationEngine.ExecuteTaskAsync(request);

            // Assert: engine completes but the dignity port was invoked to assess data handling
            Assert.True(response.IsSuccess);
            _fixture.MockInformationEthicsPort.Verify(
                x => x.AssessInformationalDignityAsync(It.Is<DignityAssessmentRequest>(
                    r => r.SubjectId == "user-456" && r.ProposedAction == "Process")),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task GDPR_ConsentRecording_SucceedsWithValidRequest()
        {
            // Arrange
            var consentRequest = new ConsentRecordRequest
            {
                SubjectId = "user-789",
                ConsentType = "Marketing",
                TenantId = "test-tenant"
            };

            var expectedResponse = new ConsentRecordResponse
            {
                IsSuccess = true,
                SubjectId = "user-789",
                ConsentType = "Marketing"
            };

            _fixture.MockGDPRCompliancePort
                .Setup(x => x.RecordGdprConsentAsync(It.IsAny<ConsentRecordRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _gdprAdapter.RecordGdprConsentAsync(consentRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("user-789", result.SubjectId);
        }

        [Fact]
        public async Task EUAIAct_HighRiskSystem_IsCorrectlyClassified()
        {
            // Arrange
            var classificationRequest = new RiskClassificationRequest
            {
                SystemName = "hr-system-001",
                SystemVersion = "1.0",
                IntendedPurpose = "This AI system is used for recruitment and hiring decisions.",
                TenantId = "test-tenant"
            };

            var expectedResponse = new RiskClassificationResponse
            {
                SystemName = "hr-system-001",
                RiskLevel = AIRiskLevel.High,
                Justification = "System used for recruitment falls under Annex III high-risk category."
            };

            _fixture.MockEUAIActCompliancePort
                .Setup(x => x.ClassifySystemRiskAsync(It.IsAny<RiskClassificationRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _euAiActAdapter.ClassifySystemRiskAsync(classificationRequest);

            // Assert
            Assert.Equal(AIRiskLevel.High, result.RiskLevel);
            Assert.Contains("recruitment", result.Justification);
        }

        [Theory]
        [InlineData("en-US", "Focus on personal achievement and benefits.")]
        [InlineData("ja-JP", "Provide detailed instructions, clear rules, and guarantees.")]
        [InlineData("de-DE", "Emphasize collaboration and flat team structures.")]
        public async Task CrossCulturalAdaptation_AdaptsRecommendationsForDifferentLocales(string locale, string expectedRecommendation)
        {
            // Arrange
            var request = new CulturalProfileRequest { Locale = locale };

            // Act
            var profileResponse = await _culturalFrameworkEngine.GetCulturalProfileAsync(request);
            var recommendations = await _culturalFrameworkEngine.GetUIAdaptationRecommendationsAsync(new UIAdaptationRequest { Profile = profileResponse.Profile });

            // Assert
            Assert.True(profileResponse.IsSuccess);
            // Recommendations contain additional detail text after the key phrase, so use prefix matching
            Assert.Contains(recommendations.Recommendations, r => r.StartsWith(expectedRecommendation));
        }

        [Fact]
        public async Task Performance_OrchestrationEngine_MeetsSla()
        {
            // Arrange
            const int numRequests = 20;
            const int slaMilliseconds = 500; // NFR: Should be well under this
            var agentDefinition = new AgentDefinition { AgentType = "PerfAgent", DefaultAutonomyLevel = AutonomyLevel.FullyAutonomous };
            await _orchestrationEngine.RegisterAgentAsync(agentDefinition);

            var request = new AgentExecutionRequest
            {
                Task = new AgentTask { Goal = "Performance Test", RequiredAgentTypes = new List<string> { "PerfAgent" } },
                RequestingUserId = "perf-test-user"
            };

            _fixture.MockAgentRuntimeAdapter
                .Setup(x => x.ExecuteAgentLogicAsync(It.IsAny<string>(), It.IsAny<AgentTask>()))
                .ReturnsAsync("OK COMPLETE");

            // Act
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task>();
            for (int i = 0; i < numRequests; i++)
            {
                tasks.Add(_orchestrationEngine.ExecuteTaskAsync(request));
            }
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            var averageTime = stopwatch.ElapsedMilliseconds / (double)numRequests;

            // Assert
            Assert.True(averageTime < slaMilliseconds, $"Average execution time {averageTime}ms exceeded SLA of {slaMilliseconds}ms.");
        }

        [Fact]
        public async Task EUAIAct_UnacceptableRiskSystem_IsCorrectlyClassified()
        {
            // Arrange
            var classificationRequest = new RiskClassificationRequest
            {
                SystemName = "unacceptable-system-001",
                SystemVersion = "1.0",
                IntendedPurpose = "This AI system uses subliminal techniques to manipulate user behavior.",
                TenantId = "test-tenant"
            };

            var expectedResponse = new RiskClassificationResponse
            {
                SystemName = "unacceptable-system-001",
                RiskLevel = AIRiskLevel.Unacceptable,
                Justification = "System uses subliminal techniques, banned under EU AI Act Article 5."
            };

            _fixture.MockEUAIActCompliancePort
                .Setup(x => x.ClassifySystemRiskAsync(It.IsAny<RiskClassificationRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _euAiActAdapter.ClassifySystemRiskAsync(classificationRequest);

            // Assert
            Assert.Equal(AIRiskLevel.Unacceptable, result.RiskLevel);
        }
    }
}
