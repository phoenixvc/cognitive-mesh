using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports.Models;
using CognitiveMesh.BusinessApplications.AgentRegistry.Services;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports;
using CognitiveMesh.BusinessApplications.Common.Models;

namespace CognitiveMesh.Tests.BusinessApplications.UnitTests.AgentRegistry.Services
{
    public class AgentRegistryServiceTests : IDisposable
    {
        private readonly AgentDbContext _dbContext;
        private readonly Mock<ILogger<AgentRegistryService>> _mockLogger;
        private readonly AgentRegistryService _agentRegistryService;

        public AgentRegistryServiceTests()
        {
            var options = new DbContextOptionsBuilder<AgentDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test run
                .Options;
            _dbContext = new AgentDbContext(options);
            _dbContext.Database.EnsureCreated(); // Ensure the database is created

            _mockLogger = new Mock<ILogger<AgentRegistryService>>();
            _agentRegistryService = new AgentRegistryService(_dbContext, _mockLogger.Object);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted(); // Clean up the database after each test
            _dbContext.Dispose();
        }

        private AgentDefinition CreateTestAgentDefinition(string agentType, string description = "Test Description", AutonomyLevel autonomy = AutonomyLevel.FullyAutonomous)
        {
            return new AgentDefinition
            {
                AgentId = Guid.NewGuid(),
                AgentType = agentType,
                Description = description,
                Capabilities = new List<string> { "Capability1", "Capability2" },
                DefaultAutonomyLevel = autonomy,
                DefaultAuthorityScope = new AuthorityScope
                {
                    AllowedApiEndpoints = new List<string> { "/api/v1/test" },
                    MaxResourceConsumption = 100,
                    MaxBudget = 500,
                    DataAccessPolicies = new List<string> { "read:public" }
                },
                Status = AgentStatus.Active
            };
        }

        [Fact]
        public async Task RegisterAgentAsync_ValidDefinition_RegistersSuccessfully()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("TestAgent1");

            // Act
            var registeredAgent = await _agentRegistryService.RegisterAgentAsync(agent);

            // Assert
            Assert.NotNull(registeredAgent);
            Assert.NotEqual(Guid.Empty, registeredAgent.AgentId);
            Assert.Equal(agent.AgentType, registeredAgent.AgentType);
            Assert.Equal(AgentStatus.Active, registeredAgent.Status);

            var retrievedAgent = await _dbContext.AgentDefinitions.FindAsync(registeredAgent.AgentId);
            Assert.NotNull(retrievedAgent);
            Assert.Equal(registeredAgent.AgentType, retrievedAgent.AgentType);

            var versionRecord = await _dbContext.AgentVersionRecords.FirstOrDefaultAsync(v => v.AgentId == registeredAgent.AgentId);
            Assert.NotNull(versionRecord);
            Assert.Equal("1.0.0", versionRecord.Version);
        }

        [Fact]
        public async Task RegisterAgentAsync_InvalidDefinition_ThrowsAgentValidationException()
        {
            // Arrange
            var agent = CreateTestAgentDefinition(""); // Invalid: empty AgentType

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AgentValidationException>(() => _agentRegistryService.RegisterAgentAsync(agent));
            Assert.Contains("AgentType is required", exception.Message);
        }

        [Fact]
        public async Task RegisterAgentAsync_DuplicateAgentType_ThrowsAgentValidationException()
        {
            // Arrange
            var agent1 = CreateTestAgentDefinition("DuplicateAgent");
            await _agentRegistryService.RegisterAgentAsync(agent1);

            var agent2 = CreateTestAgentDefinition("DuplicateAgent"); // Duplicate type

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AgentValidationException>(() => _agentRegistryService.RegisterAgentAsync(agent2));
            Assert.Contains("An agent with type 'DuplicateAgent' already exists", exception.Message);
        }

        [Fact]
        public async Task GetAgentByIdAsync_AgentExists_ReturnsAgent()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("ExistingAgent");
            await _agentRegistryService.RegisterAgentAsync(agent);

            // Act
            var retrievedAgent = await _agentRegistryService.GetAgentByIdAsync(agent.AgentId);

            // Assert
            Assert.NotNull(retrievedAgent);
            Assert.Equal(agent.AgentId, retrievedAgent.AgentId);
            Assert.Equal(agent.AgentType, retrievedAgent.AgentType);
        }

        [Fact]
        public async Task GetAgentByIdAsync_AgentNotFound_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var retrievedAgent = await _agentRegistryService.GetAgentByIdAsync(nonExistentId);

            // Assert
            Assert.Null(retrievedAgent);
        }

        [Fact]
        public async Task ListAgentsAsync_ReturnsAllAgents()
        {
            // Arrange
            await _agentRegistryService.RegisterAgentAsync(CreateTestAgentDefinition("AgentA"));
            await _agentRegistryService.RegisterAgentAsync(CreateTestAgentDefinition("AgentB"));
            await _agentRegistryService.RegisterAgentAsync(CreateTestAgentDefinition("AgentC"));

            // Act
            var agents = (await _agentRegistryService.ListAgentsAsync()).ToList();

            // Assert
            Assert.Equal(3, agents.Count);
            Assert.Contains(agents, a => a.AgentType == "AgentA");
            Assert.Contains(agents, a => a.AgentType == "AgentB");
            Assert.Contains(agents, a => a.AgentType == "AgentC");
        }

        [Fact]
        public async Task ListAgentsAsync_IncludeRetiredFalse_ExcludesRetiredAgents()
        {
            // Arrange
            var activeAgent = CreateTestAgentDefinition("ActiveAgent");
            var retiredAgent = CreateTestAgentDefinition("RetiredAgent");
            retiredAgent.Status = AgentStatus.Retired;

            await _agentRegistryService.RegisterAgentAsync(activeAgent);
            await _agentRegistryService.RegisterAgentAsync(retiredAgent);
            await _dbContext.SaveChangesAsync(); // Ensure status is saved

            // Act
            var agents = (await _agentRegistryService.ListAgentsAsync(includeRetired: false)).ToList();

            // Assert
            Assert.Single(agents);
            Assert.Contains(agents, a => a.AgentType == "ActiveAgent");
            Assert.DoesNotContain(agents, a => a.AgentType == "RetiredAgent");
        }

        [Fact]
        public async Task ListAgentsAsync_IncludeDeprecatedFalse_ExcludesDeprecatedAgents()
        {
            // Arrange
            var activeAgent = CreateTestAgentDefinition("ActiveAgent");
            var deprecatedAgent = CreateTestAgentDefinition("DeprecatedAgent");
            deprecatedAgent.Status = AgentStatus.Deprecated;

            await _agentRegistryService.RegisterAgentAsync(activeAgent);
            await _agentRegistryService.RegisterAgentAsync(deprecatedAgent);
            await _dbContext.SaveChangesAsync(); // Ensure status is saved

            // Act
            var agents = (await _agentRegistryService.ListAgentsAsync(includeDeprecated: false)).ToList();

            // Assert
            Assert.Single(agents);
            Assert.Contains(agents, a => a.AgentType == "ActiveAgent");
            Assert.DoesNotContain(agents, a => a.AgentType == "DeprecatedAgent");
        }

        [Fact]
        public async Task UpdateAgentAsync_ValidUpdate_UpdatesSuccessfully()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("AgentToUpdate");
            await _agentRegistryService.RegisterAgentAsync(agent);

            agent.Description = "Updated Description";
            agent.Capabilities.Add("NewCapability");

            // Act
            var updatedAgent = await _agentRegistryService.UpdateAgentAsync(agent);

            // Assert
            Assert.NotNull(updatedAgent);
            Assert.Equal("Updated Description", updatedAgent.Description);
            Assert.Contains("NewCapability", updatedAgent.Capabilities);

            var retrievedAgent = await _dbContext.AgentDefinitions.FindAsync(agent.AgentId);
            Assert.NotNull(retrievedAgent);
            Assert.Equal("Updated Description", retrievedAgent.Description);
            Assert.Contains("NewCapability", retrievedAgent.Capabilities);

            var versionRecords = await _dbContext.AgentVersionRecords.Where(v => v.AgentId == agent.AgentId).ToListAsync();
            Assert.Equal(2, versionRecords.Count); // Initial + Update
            Assert.Equal("1.0.1", versionRecords.Last().Version);
        }

        [Fact]
        public async Task UpdateAgentAsync_AgentNotFound_ThrowsAgentNotFoundException()
        {
            // Arrange
            var nonExistentAgent = CreateTestAgentDefinition("NonExistent");
            nonExistentAgent.AgentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<AgentNotFoundException>(() => _agentRegistryService.UpdateAgentAsync(nonExistentAgent));
        }

        [Fact]
        public async Task UpdateAgentAsync_RetiredAgent_ThrowsAgentRetiredException()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("RetiredAgent");
            await _agentRegistryService.RegisterAgentAsync(agent);
            await _agentRegistryService.RetireAgentAsync(agent.AgentId); // Retire the agent

            agent.Description = "Attempt to update";

            // Act & Assert
            await Assert.ThrowsAsync<AgentRetiredException>(() => _agentRegistryService.UpdateAgentAsync(agent));
        }

        [Fact]
        public async Task DeprecateAgentAsync_AgentExists_DeprecatesSuccessfully()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("AgentToDeprecate");
            await _agentRegistryService.RegisterAgentAsync(agent);
            var deprecationNotice = new DeprecationNotice
            {
                Reason = "Newer version available",
                SunsetDate = DateTimeOffset.UtcNow.AddMonths(6)
            };

            // Act
            var success = await _agentRegistryService.DeprecateAgentAsync(agent.AgentId, deprecationNotice);

            // Assert
            Assert.True(success);
            var retrievedAgent = await _dbContext.AgentDefinitions.FindAsync(agent.AgentId);
            Assert.NotNull(retrievedAgent);
            Assert.Equal(AgentStatus.Deprecated, retrievedAgent.Status);

            var versionRecords = await _dbContext.AgentVersionRecords.Where(v => v.AgentId == agent.AgentId).ToListAsync();
            Assert.Equal(2, versionRecords.Count); // Initial + Deprecation
            Assert.Equal(AgentStatus.Deprecated, versionRecords.Last().Status);
        }

        [Fact]
        public async Task DeprecateAgentAsync_AgentNotFound_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var deprecationNotice = new DeprecationNotice { Reason = "Test" };

            // Act
            var success = await _agentRegistryService.DeprecateAgentAsync(nonExistentId, deprecationNotice);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task RetireAgentAsync_AgentExists_RetiresSuccessfully()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("AgentToRetire");
            await _agentRegistryService.RegisterAgentAsync(agent);

            // Act
            var success = await _agentRegistryService.RetireAgentAsync(agent.AgentId);

            // Assert
            Assert.True(success);
            var retrievedAgent = await _dbContext.AgentDefinitions.FindAsync(agent.AgentId);
            Assert.NotNull(retrievedAgent);
            Assert.Equal(AgentStatus.Retired, retrievedAgent.Status);

            var versionRecords = await _dbContext.AgentVersionRecords.Where(v => v.AgentId == agent.AgentId).ToListAsync();
            Assert.Equal(2, versionRecords.Count); // Initial + Retirement
            Assert.Equal(AgentStatus.Retired, versionRecords.Last().Status);
        }

        [Fact]
        public async Task RetireAgentAsync_AgentNotFound_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var success = await _agentRegistryService.RetireAgentAsync(nonExistentId);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task RetireAgentAsync_AlreadyRetired_ReturnsTrue()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("AlreadyRetiredAgent");
            await _agentRegistryService.RegisterAgentAsync(agent);
            await _agentRegistryService.RetireAgentAsync(agent.AgentId); // Retire once

            // Act
            var success = await _agentRegistryService.RetireAgentAsync(agent.AgentId); // Retire again

            // Assert
            Assert.True(success); // Should still return true as it's already in the desired state
            var retrievedAgent = await _dbContext.AgentDefinitions.FindAsync(agent.AgentId);
            Assert.Equal(AgentStatus.Retired, retrievedAgent.Status);
        }

        [Fact]
        public async Task FindAgentsAsync_ByCapabilities_ReturnsMatchingAgents()
        {
            // Arrange
            var agent1 = CreateTestAgentDefinition("AgentCap1");
            agent1.Capabilities = new List<string> { "Reporting", "Analytics" };
            var agent2 = CreateTestAgentDefinition("AgentCap2");
            agent2.Capabilities = new List<string> { "Analytics", "Visualization" };
            var agent3 = CreateTestAgentDefinition("AgentCap3");
            agent3.Capabilities = new List<string> { "Reporting" };

            await _agentRegistryService.RegisterAgentAsync(agent1);
            await _agentRegistryService.RegisterAgentAsync(agent2);
            await _agentRegistryService.RegisterAgentAsync(agent3);

            var criteria = new AgentSearchCriteria { RequiredCapabilities = new List<string> { "Analytics" } };

            // Act
            var result = (await _agentRegistryService.FindAgentsAsync(criteria)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.AgentType == "AgentCap1");
            Assert.Contains(result, a => a.AgentType == "AgentCap2");
        }

        [Fact]
        public async Task FindAgentsAsync_ByAgentTypes_ReturnsMatchingAgents()
        {
            // Arrange
            var agent1 = CreateTestAgentDefinition("TypeA");
            var agent2 = CreateTestAgentDefinition("TypeB");
            var agent3 = CreateTestAgentDefinition("TypeC");

            await _agentRegistryService.RegisterAgentAsync(agent1);
            await _agentRegistryService.RegisterAgentAsync(agent2);
            await _agentRegistryService.RegisterAgentAsync(agent3);

            var criteria = new AgentSearchCriteria { AgentTypes = new List<string> { "TypeA", "TypeC" } };

            // Act
            var result = (await _agentRegistryService.FindAgentsAsync(criteria)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.AgentType == "TypeA");
            Assert.Contains(result, a => a.AgentType == "TypeC");
        }

        [Fact]
        public async Task FindAgentsAsync_ByMinimumAutonomyLevel_ReturnsMatchingAgents()
        {
            // Arrange
            var agent1 = CreateTestAgentDefinition("AgentLowAutonomy", autonomy: AutonomyLevel.RecommendOnly);
            var agent2 = CreateTestAgentDefinition("AgentMediumAutonomy", autonomy: AutonomyLevel.ActWithConfirmation);
            var agent3 = CreateTestAgentDefinition("AgentHighAutonomy", autonomy: AutonomyLevel.FullyAutonomous);

            await _agentRegistryService.RegisterAgentAsync(agent1);
            await _agentRegistryService.RegisterAgentAsync(agent2);
            await _agentRegistryService.RegisterAgentAsync(agent3);

            var criteria = new AgentSearchCriteria { MinimumAutonomyLevel = AutonomyLevel.ActWithConfirmation };

            // Act
            var result = (await _agentRegistryService.FindAgentsAsync(criteria)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.AgentType == "AgentMediumAutonomy");
            Assert.Contains(result, a => a.AgentType == "AgentHighAutonomy");
            Assert.DoesNotContain(result, a => a.AgentType == "AgentLowAutonomy");
        }

        [Fact]
        public async Task FindAgentsAsync_BySearchText_ReturnsMatchingAgents()
        {
            // Arrange
            var agent1 = CreateTestAgentDefinition("SearchAgent1", "This agent handles financial reports.");
            var agent2 = CreateTestAgentDefinition("SearchAgent2", "Performs data analysis.");
            var agent3 = CreateTestAgentDefinition("FinancialBot", "Another financial agent.");

            await _agentRegistryService.RegisterAgentAsync(agent1);
            await _agentRegistryService.RegisterAgentAsync(agent2);
            await _agentRegistryService.RegisterAgentAsync(agent3);

            var criteria = new AgentSearchCriteria { SearchText = "financial" };

            // Act
            var result = (await _agentRegistryService.FindAgentsAsync(criteria)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.AgentType == "SearchAgent1");
            Assert.Contains(result, a => a.AgentType == "FinancialBot");
        }

        [Fact]
        public async Task GetAgentVersionHistoryAsync_ReturnsHistory()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("VersionedAgent");
            await _agentRegistryService.RegisterAgentAsync(agent); // Version 1.0.0

            agent.Description = "First update";
            await _agentRegistryService.UpdateAgentAsync(agent); // Version 1.0.1

            agent.Description = "Second update";
            await _agentRegistryService.UpdateAgentAsync(agent); // Version 1.0.2

            // Act
            var history = (await _agentRegistryService.GetAgentVersionHistoryAsync(agent.AgentId)).ToList();

            // Assert
            Assert.Equal(3, history.Count);
            Assert.Equal("1.0.2", history[0].Version);
            Assert.Equal("1.0.1", history[1].Version);
            Assert.Equal("1.0.0", history[2].Version);
        }

        [Fact]
        public async Task CircuitBreaker_DatabaseTransientFailure_RetriesAndSucceeds()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("TransientFailureAgent");
            var mockDbSet = new Mock<DbSet<AgentDefinition>>();
            var mockDbContext = new Mock<AgentDbContext>(new DbContextOptionsBuilder<AgentDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            mockDbContext.Setup(m => m.AgentDefinitions).Returns(mockDbSet.Object);
            mockDbContext.Setup(m => m.AgentDefinitions.Add(It.IsAny<AgentDefinition>())).Callback<AgentDefinition>(d => _dbContext.AgentDefinitions.Add(d));
            mockDbContext.Setup(m => m.AgentVersionRecords.Add(It.IsAny<AgentVersionRecord>())).Callback<AgentVersionRecord>(v => _dbContext.AgentVersionRecords.Add(v));

            // Simulate transient failures for SaveChangesAsync
            var callCount = 0;
            mockDbContext.Setup(m => m.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount <= 2) // Fail first 2 calls
                    {
                        throw new DbUpdateException("Simulated transient DB error", new Exception());
                    }
                    return 1; // Succeed on the 3rd call
                });

            var serviceWithMockedDb = new AgentRegistryService(mockDbContext.Object, _mockLogger.Object);

            // Act
            var registeredAgent = await serviceWithMockedDb.RegisterAgentAsync(agent);

            // Assert
            Assert.NotNull(registeredAgent);
            Assert.Equal(3, callCount); // Should have retried twice and succeeded on the third attempt
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to register agent")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // Logged warning for each retry
        }

        [Fact]
        public async Task CircuitBreaker_DatabasePersistentFailure_OpensCircuit()
        {
            // Arrange
            var agent = CreateTestAgentDefinition("PersistentFailureAgent");
            var mockDbSet = new Mock<DbSet<AgentDefinition>>();
            var mockDbContext = new Mock<AgentDbContext>(new DbContextOptionsBuilder<AgentDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            mockDbContext.Setup(m => m.AgentDefinitions).Returns(mockDbSet.Object);
            mockDbContext.Setup(m => m.AgentDefinitions.Add(It.IsAny<AgentDefinition>())).Callback<AgentDefinition>(d => _dbContext.AgentDefinitions.Add(d));
            mockDbContext.Setup(m => m.AgentVersionRecords.Add(It.IsAny<AgentVersionRecord>())).Callback<AgentVersionRecord>(v => _dbContext.AgentVersionRecords.Add(v));

            // Simulate persistent failures for SaveChangesAsync (always fail)
            mockDbContext.Setup(m => m.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Simulated persistent DB error", new Exception()));

            var serviceWithMockedDb = new AgentRegistryService(mockDbContext.Object, _mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AgentRegistrationException>(() => serviceWithMockedDb.RegisterAgentAsync(agent));
            Assert.Contains("Operation failed after 3 retries", exception.InnerException.Message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to register agent")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3)); // Logged warning for each failed attempt
        }
    }
}
