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

## Getting Started

To get started with the Cognitive Mesh project, follow these steps:

1. Clone the repository:
   ```sh
   git clone https://github.com/phoenixvc/cognitive-mesh.git
   ```

2. Open the solution file `CognitiveMesh.sln` in Visual Studio.

3. Build the solution to restore the NuGet packages and compile the projects.

4. Run the tests to ensure everything is working correctly.

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
