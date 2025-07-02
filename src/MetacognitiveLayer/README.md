# Metacognitive Layer

## 🎯 Overview

Welcome to the **Metacognitive Layer**—the self-aware "mind" of the Cognitive Mesh. This layer is responsible for monitoring, managing, and optimizing the platform's own cognitive processes. It provides the critical capabilities of self-evaluation, continuous learning, and incident response, ensuring that the mesh operates efficiently, securely, and intelligently over time.

The primary mission of the Metacognitive Layer is to orchestrate the underlying cognitive functions of the `ReasoningLayer` and `FoundationLayer`, while providing a stable and observable platform for the `AgencyLayer` and `BusinessApplications` to build upon.

---

## 🏛️ Core Responsibilities

The Metacognitive Layer is responsible for the higher-order "thinking about thinking" processes:

-   **Continuous Learning & Self-Improvement:** Analyzing user feedback and operational outcomes to identify areas for improvement, generate learning insights, and adapt system behavior over time.
-   **Security Incident Monitoring & Orchestration:** Aggregating security events, correlating them into actionable incidents, and orchestrating a response by notifying stakeholders or dispatching automated agents.
-   **Performance Monitoring & Optimization:** Tracking the performance of cognitive tasks, identifying bottlenecks, and providing data for resource optimization and SLA management.
-   **Protocol Management:** Handling structured communication protocols like the **AI Communication Protocol (ACP)** and **Metacognitive Protocol (MCP)** to ensure reliable and standardized interactions between components.
-   **Reasoning Transparency & Self-Evaluation:** Providing mechanisms to explain the "why" behind AI decisions and continuously evaluating the quality and accuracy of the `ReasoningLayer`'s outputs.
-   **Cultural Adaptation:** Applying cultural context (based on frameworks like Hofstede's dimensions) to adapt UI/UX and agent interactions for global deployments.

---

## 🏗️ Architectural Principles

The Metacognitive Layer serves as a central orchestrator within the Cognitive Mesh's **Hexagonal (Ports and Adapters)** architecture.

-   **Orchestration Hub:** It sits between the lower-level reasoning/foundation layers and the higher-level agency/business layers. It consumes services from the `ReasoningLayer` (e.g., `IThreatIntelligencePort`) and `FoundationLayer` (e.g., `IAuditLoggingPort`) to perform its monitoring functions.
-   **Service Provision:** It exposes its own capabilities, such as incident management and performance data, to the `AgencyLayer` and `BusinessApplications` layer through its own set of well-defined ports (e.g., `ISecurityIncidentPort`, `IComplianceDashboardPort`).

This strategic position allows the Metacognitive Layer to have a holistic view of the system's state, enabling it to make informed decisions about optimization, security, and learning without creating tight coupling between layers.

---

## 🧩 Major Components

The Metacognitive Layer is composed of several key components that work together to provide its self-awareness capabilities.

| Component | Description | Key Files |
| :--- | :--- | :--- |
| **SecurityMonitoring** | The heart of our real-time security operations. It consumes security events, uses the `ReasoningLayer` to detect threats, and orchestrates incident response. | `ISecurityIncidentPort.cs`, `SecurityIncidentMonitor.cs` |
| **ContinuousLearning** | Implements the feedback loop for system improvement. It collects user feedback and operational data, analyzes it for insights, and drives adaptation. | `ContinuousLearningComponent.cs`, `LearningManager.cs` |
| **Protocols** | Manages the structured communication protocols (ACP, MCP) that ensure reliable interaction between agents and services across the mesh. | `ACPHandler.cs`, `MCPHandler.cs`, `ProtocolOrchestrator.cs` |
| **PerformanceMonitoring** | Tracks key performance indicators (KPIs) such as latency, throughput, and resource utilization for all cognitive tasks. | `PerformanceMonitoring.csproj` (and associated services) |
| **SelfEvaluation** | Contains the `MetacognitiveOversightComponent`, which is responsible for evaluating the quality, accuracy, and ethical alignment of the `ReasoningLayer`'s outputs. | `MetacognitiveOversightComponent.cs` |
| **ReasoningTransparency** | Provides services for generating explanations and justifications for AI-driven decisions, making the system more transparent and trustworthy. | `ReasoningTransparency.csproj` (and associated services) |
| **CulturalAdaptation** | Implements the `CrossCulturalFrameworkEngine`, which applies cultural models to tailor user interactions for a global audience. | `CrossCulturalFrameworkEngine.cs` (conceptual) |

---

## 🚀 Usage Examples

Services in the `AgencyLayer` or `BusinessApplications` layer can interact with the Metacognitive Layer's services through its defined ports.

### Example: An Agent Reporting a Security Event

Here is how an agent in the `AgencyLayer` might report a suspicious event it has observed to the `SecurityIncidentMonitor`.

```csharp
// In a custom agent class within the AgencyLayer

public class MyCustomAgent : IAgent
{
    private readonly ISecurityIncidentPort _securityIncidentPort;
    private readonly ILogger<MyCustomAgent> _logger;

    public MyCustomAgent(ISecurityIncidentPort securityIncidentPort, ILogger<MyCustomAgent> logger)
    {
        _securityIncidentPort = securityIncidentPort;
        _logger = logger;
    }

    public async Task<AgentTaskResponse> ExecuteTaskAsync(AgentTaskRequest request)
    {
        // ... agent performs its task ...

        // During its task, the agent observes a suspicious login attempt
        bool suspiciousActivityDetected = true; // based on some logic

        if (suspiciousActivityDetected)
        {
            _logger.LogWarning("Agent detected suspicious activity. Reporting to SecurityIncidentMonitor.");

            var securityEvent = new SecurityEvent
            {
                Timestamp = DateTimeOffset.UtcNow,
                Source = AgentId,
                EventType = "SuspiciousLoginAttempt",
                Data = new Dictionary<string, object>
                {
                    { "subjectId", "user-abc" },
                    { "ipAddress", "203.0.113.55" },
                    { "isSuccess", false }
                }
            };

            // Report the event to the Metacognitive Layer for analysis and correlation
            await _securityIncidentPort.HandleSecurityEventAsync(securityEvent);
        }

        return new AgentTaskResponse { IsSuccess = true, Message = "Task completed." };
    }
}
```

---

## 🤝 How to Contribute

The Metacognitive Layer is a key area for innovation. To add a new metacognitive capability (e.g., a new type of monitor or self-evaluation metric):

1.  **Define the Port:** Create a new interface in a `Ports/` directory within the appropriate component folder (e.g., `src/MetacognitiveLayer/MyNewMonitor/Ports/`).
2.  **Implement the Service/Monitor:** Create the implementation class that contains the core logic for the new capability.
3.  **Integrate with Orchestrators:** Ensure that your new service is properly integrated with the relevant orchestrators or event streams (e.g., subscribing to the audit log).
4.  **Add Unit and Integration Tests:** Provide thorough tests to validate the functionality of your new component.

Please refer to the main [CONTRIBUTING.md](../../CONTRIBUTING.md) file for general contribution guidelines.
