# Agency Layer

## 🎯 Overview

Welcome to the **Agency Layer**—the dynamic "action" arm of the Cognitive Mesh. This is where cognitive strategies and plans are transformed into tangible outcomes. The Agency Layer houses a diverse ecosystem of autonomous agents, each designed to perform specific tasks, use tools, and collaborate to solve complex problems.

The primary mission of the Agency Layer is to provide a robust and extensible framework for autonomous task execution, ensuring that all actions are efficient, auditable, and aligned with the strategic goals defined by the higher layers of the mesh.

---

## 🏛️ Core Responsibilities

The Agency Layer is responsible for all forms of autonomous and semi-autonomous execution:

-   **Task Execution:** Carrying out complex, multi-step tasks assigned by the `BusinessApplications` layer, such as conducting research, analyzing data, or automating business processes.
-   **Tool Integration & Use:** Providing agents with a rich set of tools—from web search and data analysis to code execution—and managing the secure and efficient use of these capabilities.
-   **Multi-Agent Orchestration:** Coordinating teams of specialized agents to tackle problems that are too complex for a single agent. This includes dynamic task decomposition, agent assignment, and result synthesis.
-   **Automated Security Response:** Deploying specialized security agents to perform immediate containment and remediation actions in response to threats detected by the `MetacognitiveLayer`.
-   **Human-in-the-Loop Collaboration:** Facilitating seamless handoffs between automated agents and human operators for tasks that require review, approval, or intervention.

---

## 🏗️ Architectural Principles

The Agency Layer is a critical component of the Cognitive Mesh's **Hexagonal (Ports and Adapters)** architecture, acting as the bridge between cognition and action.

-   **Orchestrated Execution:** The Agency Layer is primarily orchestrated by the `BusinessApplications` layer, which dispatches tasks via well-defined ports like `IAgentOrchestrationPort`. This ensures that all agentic work is initiated in response to a clear business need.
-   **Informed Action:** Agents within this layer consume services from the `ReasoningLayer` (e.g., to make ethical decisions) and the `MetacognitiveLayer` (e.g., to be aware of their own performance) to guide their actions. This ensures that execution is not just efficient but also intelligent and compliant.
-   **Decoupled Capabilities:** The `ToolIntegration` framework allows agents to be equipped with new capabilities without altering their core logic. New tools can be added as simple adapters, making the system highly extensible.

---

## 🧩 Major Components

The Agency Layer is composed of several key components that enable its autonomous capabilities.

| Component | Description | Key Files |
| :--- | :--- | :--- |
| **MultiAgentOrchestration** | The core engine responsible for managing and coordinating teams of agents. It handles task decomposition, agent selection, and workflow execution. | `MultiAgentOrchestrationEngine.cs` |
| **ToolIntegration** | A comprehensive framework that allows agents to use a wide variety of tools. It includes a base class for all tools and numerous implementations. | `BaseTool.cs`, `ClassificationTool.cs`, `WebSearchTool.cs` |
| **SecurityAgents** | A suite of specialized agents dedicated to security tasks. The `AutomatedResponseAgent` is a prime example, performing immediate incident response. | `AutomatedResponseAgent.cs` |
| **ActionPlanning** | Contains the logic for agents to create detailed, step-by-step plans to achieve a given goal. | `ActionPlanning.csproj` (conceptual) |
| **DecisionExecution** | Manages the execution of individual steps within an agent's plan, including tool calls and state management. | `DecisionExecution.csproj` (conceptual) |
| **HumanCollaboration** | Provides the infrastructure for human-in-the-loop workflows, allowing agents to pause and request human input or approval. | `HumanCollaboration.csproj` (conceptual) |

---

## 🚀 Usage Examples

The Agency Layer is typically invoked from the `BusinessApplications` layer through a port, such as `IAgentOrchestrationPort`.

### Example: Dispatching a Research Task to an Agent

Here is how a service in the `BusinessApplications` layer might dispatch a research task to be handled by the Agency Layer.

```csharp
// In a service class within the BusinessApplications layer

public class ResearchService
{
    private readonly IAgentOrchestrationPort _agentOrchestrationPort;
    private readonly ILogger<ResearchService> _logger;

    public ResearchService(IAgentOrchestrationPort agentOrchestrationPort, ILogger<ResearchService> logger)
    {
        _agentOrchestrationPort = agentOrchestrationPort;
        _logger = logger;
    }

    public async Task<string> ConductResearchAsync(string topic)
    {
        _logger.LogInformation("Dispatching research task for topic: {Topic}", topic);

        var agentTask = new AgentTaskRequest
        {
            // The orchestrator will select the appropriate agent (e.g., "ResearchAgent")
            TaskDescription = $"Conduct a comprehensive research report on the topic: {topic}",
            Parameters = new Dictionary<string, object>
            {
                { "depth", "deep" },
                { "sources", new[] { "web", "internal_knowledge_base" } }
            },
            Priority = 5 // Normal priority
        };

        // Dispatch the task to the Agency Layer for execution
        var response = await _agentOrchestrationPort.ExecuteTaskAsync(agentTask);

        if (response.IsSuccess)
        {
            _logger.LogInformation("Research task completed successfully.");
            // The result is in the response output, typically as a structured string or JSON
            return response.Output.GetValueOrDefault("report")?.ToString();
        }
        else
        {
            _logger.LogError("Research task failed: {ErrorMessage}", response.Message);
            // Handle the failure
            throw new InvalidOperationException("Failed to conduct research.");
        }
    }
}
```

---

## 🤝 How to Contribute

The Agency Layer is designed for extensibility. You can contribute by adding new agents or tools.

### Adding a New Agent

1.  Create a new class that implements the `IAgent` interface.
2.  Implement the `AgentId` property and the `ExecuteTaskAsync` method.
3.  Register your new agent with the `AgentRegistry` so it can be discovered by the `MultiAgentOrchestrationEngine`.
4.  Provide unit tests for your agent's logic.

### Adding a New Tool

1.  Create a new class that inherits from `BaseTool`.
2.  Implement the `Name`, `Description`, and `ExecuteAsync` methods.
3.  Register the new tool in the `ToolRegistry` so agents can use it.
4.  Add integration tests to verify that the tool works as expected.

Please refer to the main [CONTRIBUTING.md](../../CONTRIBUTING.md) file for general contribution guidelines.
