# Cognitive Mesh

## Overview

The Cognitive Mesh project is a comprehensive solution that integrates various layers and components to provide advanced cognitive capabilities. The project is organized into multiple layers, each containing specific features and modules.

## Project Structure

The project is structured as follows:

* `src/AgencyLayer`: Contains components related to action planning, decision execution, human collaboration, process automation, and tool integration.
  * `src/AgencyLayer/ActionPlanning`: Contains the `src/AgencyLayer/ActionPlanning/ActionPlanner.cs` file.
  * `src/AgencyLayer/DecisionExecution`: Contains the `src/AgencyLayer/DecisionExecution/DecisionExecutor.cs` file.
  * `src/AgencyLayer/HumanCollaboration`: Contains the `src/AgencyLayer/HumanCollaboration/CollaborationManager.cs` file.
  * `src/AgencyLayer/ProcessAutomation`: Contains the `src/AgencyLayer/ProcessAutomation/ProcessAutomator.cs` file.
  * `src/AgencyLayer/ToolIntegration`: Contains various tool integration files such as `src/AgencyLayer/ToolIntegration/BaseTool.cs`, `src/AgencyLayer/ToolIntegration/ClassificationTool.cs`, `src/AgencyLayer/ToolIntegration/ClusteringTool.cs`, `src/AgencyLayer/ToolIntegration/DataAnalysisTool.cs`, `src/AgencyLayer/ToolIntegration/DataCleaningTool.cs`, `src/AgencyLayer/ToolIntegration/DataVisualizationTool.cs`, `src/AgencyLayer/ToolIntegration/NamedEntityRecognitionTool.cs`, `src/AgencyLayer/ToolIntegration/PatternRecognitionTool.cs`, `src/AgencyLayer/ToolIntegration/PredictiveAnalyticsTool.cs`, `src/AgencyLayer/ToolIntegration/RecommendationSystemTool.cs`, `src/AgencyLayer/ToolIntegration/SentimentAnalysisTool.cs`, `src/AgencyLayer/ToolIntegration/TextGenerationTool.cs`, `src/AgencyLayer/ToolIntegration/ToolIntegrator.cs`, `src/AgencyLayer/ToolIntegration/WebScrapingTool.cs`, and `src/AgencyLayer/ToolIntegration/WebSearchTool.cs`.

* `src/BusinessApplications`: Contains components related to customer intelligence, decision support, knowledge management, process automation, and research analysis.
  * `src/BusinessApplications/CustomerIntelligence`: Contains the `src/BusinessApplications/CustomerIntelligence/CustomerIntelligenceManager.cs` file.
  * `src/BusinessApplications/DecisionSupport`: Contains the `src/BusinessApplications/DecisionSupport/DecisionSupportManager.cs` file.
  * `src/BusinessApplications/KnowledgeManagement`: Contains the `src/BusinessApplications/KnowledgeManagement/KnowledgeManager.cs` file.
  * `src/BusinessApplications/ProcessAutomation`: Contains the `src/BusinessApplications/ProcessAutomation/BusinessProcessAutomator.cs` file.
  * `src/BusinessApplications/ResearchAnalysis`: Contains the `src/BusinessApplications/ResearchAnalysis/ResearchAnalyst.cs` file.

* `src/FoundationLayer`: Contains components related to Azure Blob Storage, Azure Cosmos DB, document processing, enterprise connectors, knowledge graph, semantic search, vector database, and OneLake integration.
  * `src/FoundationLayer/AzureBlobStorage`: Contains the `src/FoundationLayer/AzureBlobStorage/BlobStorageManager.cs` file.
  * `src/FoundationLayer/AzureCosmosDB`: Contains the `src/FoundationLayer/AzureCosmosDB/CosmosDBManager.cs` file.
  * `src/FoundationLayer/DocumentProcessing`: Contains the `DocumentProcessor.cs` file.
  * `src/FoundationLayer/EnterpriseConnectors`: Contains the `src/FoundationLayer/EnterpriseConnectors/EnterpriseConnector.cs` file.
  * `src/FoundationLayer/KnowledgeGraph`: Contains the `src/FoundationLayer/KnowledgeGraph/KnowledgeGraphManager.cs` file.
  * `src/FoundationLayer/SemanticSearch`: Contains the `src/FoundationLayer/SemanticSearch/SemanticSearchManager.cs` file.
  * `src/FoundationLayer/VectorDatabase`: Contains the `src/FoundationLayer/VectorDatabase/VectorDatabaseManager.cs` file.
  * `src/FoundationLayer/OneLakeIntegration`: Contains the `src/FoundationLayer/OneLakeIntegration/OneLakeIntegrationManager.cs` file.

* `src/MetacognitiveLayer`: Contains components related to continuous learning, performance monitoring, reasoning transparency, self-evaluation, and uncertainty quantification.
  * `src/MetacognitiveLayer/ContinuousLearning`: Contains the `src/MetacognitiveLayer/ContinuousLearning/LearningManager.cs` file.
  * `src/MetacognitiveLayer/PerformanceMonitoring`: Contains the `src/MetacognitiveLayer/PerformanceMonitoring/PerformanceMonitor.cs` file.
  * `src/MetacognitiveLayer/ReasoningTransparency`: Contains the `src/MetacognitiveLayer/ReasoningTransparency/TransparencyManager.cs` file.
  * `src/MetacognitiveLayer/SelfEvaluation`: Contains the `src/MetacognitiveLayer/SelfEvaluation/SelfEvaluator.cs` file.
  * `src/MetacognitiveLayer/UncertaintyQuantification`: Contains the `src/MetacognitiveLayer/UncertaintyQuantification/UncertaintyQuantifier.cs` file.

* `src/ReasoningLayer`: Contains components related to analytical reasoning, creative reasoning, critical reasoning, domain-specific reasoning, ethical reasoning, and systems reasoning.
  * `src/ReasoningLayer/AnalyticalReasoning`: Contains the `src/ReasoningLayer/AnalyticalReasoning/AnalyticalReasoner.cs` file.
  * `src/ReasoningLayer/CreativeReasoning`: Contains the `src/ReasoningLayer/CreativeReasoning/CreativeReasoner.cs` file.
  * `src/ReasoningLayer/CriticalReasoning`: Contains the `src/ReasoningLayer/CriticalReasoning/CriticalReasoner.cs` file.
  * `src/ReasoningLayer/DomainSpecificReasoning`: Contains the `src/ReasoningLayer/DomainSpecificReasoning/DomainSpecificReasoner.cs` file.
  * `src/ReasoningLayer/EthicalReasoning`: Contains the `src/ReasoningLayer/EthicalReasoning/EthicalReasoner.cs` file.
  * `src/ReasoningLayer/SystemsReasoning`: Contains the `src/ReasoningLayer/SystemsReasoning/SystemsReasoner.cs` file.

* `tests/AgencyLayer/ToolIntegration`: Contains test files for various tool integration components such as `tests/AgencyLayer/ToolIntegration/ClassificationToolTests.cs`, `tests/AgencyLayer/ToolIntegration/ClusteringToolTests.cs`, `tests/AgencyLayer/ToolIntegration/DataAnalysisToolTests.cs`, `tests/AgencyLayer/ToolIntegration/DataCleaningToolTests.cs`, `tests/AgencyLayer/ToolIntegration/DataVisualizationToolTests.cs`, and others.

* Root directory: Contains additional files such as `README.md`, `SynthesisRequest.cs`, `SynthesisResult.cs`, and others.

## Solution File

The solution file `CognitiveMesh.sln` has been created in the root directory. It includes projects for each layer and feature in the `src` directory, as well as test projects in the `tests` directory.

## Prerequisites

To build and run the Cognitive Mesh project, you will need the following tools and software:

* .NET 6.0 SDK or later
* Visual Studio 2019 or later (or Visual Studio Code)
* Azure Subscription (for Azure services integration)
* Azure CLI (for managing Azure resources)

## Quick Start

Follow these steps to set up and run the Cognitive Mesh project locally:

1. Clone the repository:
   ```sh
   git clone https://github.com/phoenixvc/cognitive-mesh.git
   ```

2. Open the solution file `CognitiveMesh.sln` in Visual Studio.

3. Build the solution to restore the NuGet packages and compile the projects.

4. Run the tests to ensure everything is working correctly.

5. Configure the necessary Azure services and update the `appsettings.json` file with your Azure credentials and settings.

6. Start the project by running the desired application (e.g., `CustomerServiceController`, `DecisionSupportController`).

## OneLake Integration

The Cognitive Mesh project now includes integration with OneLake, providing enhanced storage management capabilities. The `OneLakeIntegrationManager` class offers methods for uploading, downloading, and deleting files in OneLake.

### Usage Examples

#### Uploading a File

```csharp
using System.IO;
using Microsoft.Extensions.Logging;
using OneLakeIntegration;

public class Example
{
    private readonly OneLakeIntegrationManager _oneLakeIntegrationManager;

    public Example(OneLakeIntegrationManager oneLakeIntegrationManager)
    {
        _oneLakeIntegrationManager = oneLakeIntegrationManager;
    }

    public async Task UploadFileAsync()
    {
        using var fileStream = File.OpenRead("path/to/your/file.txt");
        bool result = await _oneLakeIntegrationManager.UploadFileAsync("your-container-name", "file.txt", fileStream);

        if (result)
        {
            Console.WriteLine("File uploaded successfully.");
        }
        else
        {
            Console.WriteLine("Failed to upload file.");
        }
    }
}
```

#### Downloading a File

```csharp
using System.IO;
using Microsoft.Extensions.Logging;
using OneLakeIntegration;

public class Example
{
    private readonly OneLakeIntegrationManager _oneLakeIntegrationManager;

    public Example(OneLakeIntegrationManager oneLakeIntegrationManager)
    {
        _oneLakeIntegrationManager = oneLakeIntegrationManager;
    }

    public async Task DownloadFileAsync()
    {
        Stream fileStream = await _oneLakeIntegrationManager.DownloadFileAsync("your-container-name", "file.txt");

        using var file = File.Create("path/to/save/file.txt");
        await fileStream.CopyToAsync(file);

        Console.WriteLine("File downloaded successfully.");
    }
}
```

#### Deleting a File

```csharp
using Microsoft.Extensions.Logging;
using OneLakeIntegration;

public class Example
{
    private readonly OneLakeIntegrationManager _oneLakeIntegrationManager;

    public Example(OneLakeIntegrationManager oneLakeIntegrationManager)
    {
        _oneLakeIntegrationManager = oneLakeIntegrationManager;
    }

    public async Task DeleteFileAsync()
    {
        bool result = await _oneLakeIntegrationManager.DeleteFileAsync("your-container-name", "file.txt");

        if (result)
        {
            Console.WriteLine("File deleted successfully.");
        }
        else
        {
            Console.WriteLine("Failed to delete file.");
        }
    }
}
```

## ADK Integration

The Cognitive Mesh project now includes integration with the ADK framework, providing advanced agent orchestration and dynamic routing capabilities. The `ArgenticAgentComponent` and `CognitiveMeshCoordinator` classes have been updated to conditionally initialize and use ADK features based on the `enable_ADK` feature flag.

### Feature Flag

The `enable_ADK` feature flag controls whether the ADK framework is enabled. When this flag is set to `true`, the ADK features are initialized and used. When the flag is set to `false`, the system falls back to basic agent logic.

### Configuration

The feature flag can be set in the `appsettings.json` file or through an environment variable.

#### Example `appsettings.json` Configuration

```json
{
  "FeatureFlags": {
    "enable_ADK": true
  }
}
```

### Usage Examples

#### Conditional Initialization

In the `ArgenticAgentComponent` class, the ADK features are conditionally initialized based on the `enable_ADK` feature flag.

```csharp
public async Task<PlanningResult> CreatePlanAsync(string task, Dictionary<string, string> context)
{
    if (_featureFlagManager.EnableADK)
    {
        // Initialize ADK orchestrator with modular workflow
        var orchestrator = new ADK.AgentOrchestrator(
            workflowType: "sequential",
            tools: new ADK.ToolRegistry(prebuilt: true, custom: true, openapi: true),
            evaluationGuardrails: true,
            multimodalSupport: true,
            cloudIntegration: _featureFlagManager.UseOneLake
        );

        // Set up Gemini/Vertex AI integration if cloud enabled
        if (_featureFlagManager.UseOneLake)
        {
            orchestrator.IntegrateGemini();
        }

        // Use orchestrator for multi-agent workflows and real-time automation
        return await orchestrator.CreatePlanAsync(task, context);
    }
    else
    {
        // Fallback: basic agent logic
        var strategicPlan = await GenerateStrategicPlanAsync(task, context);
        var steps = await GenerateStepsAsync(task, strategicPlan, context);
        var toolCalls = new List<ToolCallPlan>();

        foreach (var step in steps)
        {
            var toolCall = await PlanToolCallAsync(step, _availableTools.Values.ToList());
            toolCalls.Add(toolCall);
        }

        // Integrate with Microsoft Fabric data endpoints
        await IntegrateWithFabricDataEndpointsAsync(context);

        // Orchestrate Data Factory pipelines
        await OrchestrateDataFactoryPipelinesAsync(context);

        return new PlanningResult
        {
            Task = task,
            StrategicPlan = strategicPlan,
            Steps = steps,
            ToolCalls = toolCalls
        };
    }
}
```

#### Conditional Execution

In the `CognitiveMeshCoordinator` class, the ADK features are conditionally executed based on the `enable_ADK` feature flag.

```csharp
public async Task<CognitiveMeshResponse> ProcessQueryAsync(string query, QueryOptions options = null)
{
    options ??= new QueryOptions();

    // Create execution context
    var context = new ExecutionContext
    {
        QueryId = Guid.NewGuid().ToString(),
        Query = query,
        Options = options,
        StartTime = DateTimeOffset.UtcNow
    };

    try
    {
        // Log query start
        _logger.LogInformation("Processing query {QueryId}: {Query}", context.QueryId, query);

        // Publish query received event
        await PublishEventAsync("QueryReceived", context);

        // Step 1: Retrieve relevant knowledge
        var knowledgeTask = _ragSystem.SearchAsync(query, options.MaxKnowledgeItems);

        // Step 2: Generate multi-perspective analysis
        var perspectivesTask = _mpcSystem.AnalyzeFromMultiplePerspectivesAsync(
            query, options.Perspectives);

        // Wait for both tasks to complete
        await Task.WhenAll(knowledgeTask, perspectivesTask);

        var knowledgeResults = knowledgeTask.Result;
        var perspectiveAnalysis = perspectivesTask.Result;

        // Update context with results
        context.KnowledgeResults = knowledgeResults;
        context.PerspectiveAnalysis = perspectiveAnalysis;

        // Publish intermediate results event
        await PublishEventAsync("IntermediateResultsGenerated", context);

        // Step 3: Determine if agent execution is needed
        if (options.EnableAgentExecution && RequiresAgentExecution(query, perspectiveAnalysis))
        {
            if (_featureFlagManager.EnableADK)
            {
                // Initialize ADK orchestrator with modular workflow
                var orchestrator = new ADK.AgentOrchestrator(
                    workflowType: "sequential",
                    tools: new ADK.ToolRegistry(prebuilt: true, custom: true, openapi: true),
                    evaluationGuardrails: true,
                    multimodalSupport: true,
                    cloudIntegration: _featureFlagManager.CloudEnabled
                );

                // Set up Gemini/Vertex AI integration if cloud enabled
                if (_featureFlagManager.CloudEnabled)
                {
                    orchestrator.IntegrateGemini();
                }

                // Use orchestrator for multi-agent workflows and real-time automation
                var agentContext = new Dictionary<string, string>
                {
                    { "query", query },
                    { "knowledge", FormatKnowledgeForContext(knowledgeResults) },
                    { "perspectives", FormatPerspectivesForContext(perspectiveAnalysis) }
                };

                var plan = await orchestrator.CreatePlanAsync(query, agentContext);
                var executionResult = await orchestrator.ExecutePlanAsync(plan);

                context.AgentPlan = plan;
                context.AgentResult = executionResult;

                await PublishEventAsync("AgentExecutionCompleted", context);
            }
            else
            {
                // Fallback: basic agent logic
                var agentContext = new Dictionary<string, string>
                {
                    { "query", query },
                    { "knowledge", FormatKnowledgeForContext(knowledgeResults) },
                    { "perspectives", FormatPerspectivesForContext(perspectiveAnalysis) }
                };

                var plan = await _agentSystem.CreatePlanAsync(query, agentContext);
                var executionResult = await _agentSystem.ExecutePlanAsync(plan);

                context.AgentPlan = plan;
                context.AgentResult = executionResult;

                await PublishEventAsync("AgentExecutionCompleted", context);
            }
        }

        // Step 4: Generate final response
        string finalResponse;

        if (context.AgentResult != null)
        {
            // Use agent result as primary response
            finalResponse = await GenerateFinalResponseWithAgentResultAsync(
                query, knowledgeResults, perspectiveAnalysis, context.AgentResult);
        }
        else
        {
            // Generate response from knowledge and perspectives
            finalResponse = await GenerateFinalResponseAsync(
                query, knowledgeResults, perspectiveAnalysis);
        }

        // Create response object
        var response = new CognitiveMeshResponse
        {
            QueryId = context.QueryId,
            Query = query,
            Response = finalResponse,
            KnowledgeResults = knowledgeResults,
            PerspectiveAnalysis = perspectiveAnalysis,
            AgentResult = context.AgentResult,
            ProcessingTime = DateTimeOffset.UtcNow - context.StartTime
        };

        // Publish response generated event
        await PublishEventAsync("ResponseGenerated", context);

        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing query {QueryId}: {Message}", context.QueryId, ex.Message);

        // Publish error event
        context.Error = ex.Message;
        await PublishEventAsync("QueryError", context);

        // Return error response
        return new CognitiveMeshResponse
        {
            QueryId = context.QueryId,
            Query = query,
            Error = ex.Message,
            ProcessingTime = DateTimeOffset.UtcNow - context.StartTime
        };
    }
}
```

## Contributing

We welcome contributions to the Cognitive Mesh project. If you would like to contribute, please follow these guidelines:

1. Fork the repository.

2. Create a new branch for your feature or bug fix:
   ```sh
   git checkout -b my-feature-branch
   ```

3. Make your changes and commit them with a descriptive message.

4. Push your changes to your forked repository:
   ```sh
   git push origin my-feature-branch
   ```

5. Create a pull request to the main repository.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Changelog

### [Unreleased]

- Added integration with OneLake for enhanced storage management.
- Added integration with ADK framework for advanced agent orchestration.
- Improved error handling and logging in `CustomerServiceController` and `DecisionSupportController`.
- Added comments to `AnalyticalReasoner` and `ArgenticAgentComponent` for better code readability.
- Refactored `CognitiveMeshCoordinator` to reduce complexity and improve readability.
- Added more detailed explanations and examples in `docs/IntegrationPlan.md`.

## FAQ

### What is the Cognitive Mesh project?

The Cognitive Mesh project is a comprehensive solution that integrates various layers and components to provide advanced cognitive capabilities.

### How do I get started with the Cognitive Mesh project?

Follow the steps in the "Quick Start" section to set up and run the project locally.

### What are the prerequisites for building and running the project?

You will need .NET 6.0 SDK or later, Visual Studio 2019 or later (or Visual Studio Code), an Azure Subscription, and Azure CLI.

### How can I contribute to the project?

Follow the guidelines in the "Contributing" section to contribute to the project.

### Where can I find more information about the Azure services used in the project?

Refer to the official documentation for Azure services and libraries used in the project:

- [Azure Blob Storage](https://docs.microsoft.com/en-us/azure/storage/blobs/)
- [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/)
- [Azure Data Factory](https://docs.microsoft.com/en-us/azure/data-factory/)
- [Azure OpenAI](https://docs.microsoft.com/en-us/azure/cognitive-services/openai/)
- [Azure Text Analytics](https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/)
- [Azure Translator](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/)
- [Azure AI Search](https://docs.microsoft.com/en-us/azure/search/)
- [Azure AI Studio](https://docs.microsoft.com/en-us/azure/machine-learning/)
- [Azure Service Fabric](https://docs.microsoft.com/en-us/azure/service-fabric/)
- [Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/)

## Badges

![Build Status](https://img.shields.io/github/workflow/status/phoenixvc/cognitive-mesh/CI)
![License](https://img.shields.io/github/license/phoenixvc/cognitive-mesh)
