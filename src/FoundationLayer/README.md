# Foundation Layer

## 🎯 Overview

Welcome to the **Foundation Layer**—the bedrock of the Cognitive Mesh platform. This layer provides the core infrastructure services and cross-cutting concerns that all other layers depend on. It is designed to be robust, secure, and scalable, abstracting away the complexities of underlying technologies and providing a stable foundation for cognitive processing.

The primary mission of the Foundation Layer is to deliver enterprise-grade infrastructure capabilities with a focus on security, data management, and compliance, all while adhering to a clean, decoupled architectural model.

---

## 🏛️ Core Responsibilities

The Foundation Layer is responsible for a wide range of essential services:

-   **Security & Compliance:** Implementing the Zero-Trust security model, managing secrets, enforcing access policies, and providing comprehensive audit trails.
-   **Data Persistence:** Offering a unified interface for interacting with various data stores, including relational databases, NoSQL databases, vector databases, and blob storage.
-   **Knowledge Services:** Providing the infrastructure for knowledge representation, including semantic search, knowledge graphs, and document processing for Retrieval-Augmented Generation (RAG).
-   **Enterprise Integration:** Facilitating connectivity with enterprise systems, including data lakes like Microsoft OneLake and managing feature flags for controlled rollouts.
-   **Auditing & Logging:** Capturing immutable, auditable logs for all significant events across the mesh, which is critical for compliance, security forensics, and system analysis.

---

## 🏗️ Architectural Principles

This layer strictly adheres to the **Hexagonal (Ports and Adapters)** architecture.

-   **Ports:** We define technology-agnostic interfaces (Ports) that represent the contracts for our core services (e.g., `ISecurityPolicyPort`, `ISecretsManagementPort`, `IVectorDatabaseAdapter`). These ports are located in the `Ports/` directory of each component.
-   **Adapters:** We provide concrete implementations (Adapters) of these ports that connect to specific technologies (e.g., Azure Key Vault, Azure Cosmos DB, Azure AI Search). This allows us to swap out underlying technologies without impacting the core business logic of the higher layers.

This design ensures that the rest of the Cognitive Mesh remains decoupled from specific infrastructure choices, enhancing modularity, testability, and long-term maintainability.

---

## 🧩 Major Components

The Foundation Layer is organized into several key components, each responsible for a specific infrastructure concern.

### 🔒 Security & Compliance

This is the most critical part of the Foundation Layer, implementing the foundational Zero-Trust security model.

| Component | Description | Key Files |
| :--- | :--- | :--- |
| **SecurityPolicyEnforcementEngine** | The core engine for evaluating and enforcing security policies. It handles authentication (JWT validation) and authorization (RBAC/ABAC) for the entire mesh. | `ISecurityPolicyPort.cs`, `SecurityPolicyEnforcementEngine.cs` |
| **SecretsManagementEngine** | Provides a secure vault for managing secrets like API keys and connection strings, with support for rotation, versioning, and auditing. | `ISecretsManagementPort.cs`, `SecretsManagementEngine.cs` |
| **AuditLoggingAdapter** | A universal adapter for capturing and storing immutable audit logs. It ensures all significant actions are recorded for compliance and security analysis. | `IAuditLoggingPort.cs`, `AuditLoggingAdapter.cs` |

### 🗄️ Data Persistence & Management

| Component | Description | Key Files |
| :--- | :--- | :--- |
| **AzureCosmosDB** | Adapter for interacting with Azure Cosmos DB, used for storing structured, mission-critical data like user profiles and interaction records. | `ICosmosDbAdapter.cs` |
| **AzureBlobStorage** | Adapter for storing large binary objects, such as documents for RAG, evidence artifacts, and other unstructured data. | `IBlobStorageAdapter.cs` |
| **VectorDatabase** | Provides an abstraction for vector storage and similarity search, crucial for semantic search and RAG. Supports various backends (e.g., Azure AI Search, Redis). | `IVectorDatabaseAdapter.cs` |

### 🧠 Knowledge & Search

| Component | Description | Key Files |
| :--- | :--- | :--- |
| **SemanticSearch** | Implements the `EnhancedRAGSystem`, which combines vector search with LLMs to provide context-aware, accurate answers from a knowledge base. | `EnhancedRAGSystem.cs` |
| **KnowledgeGraph** | Provides infrastructure for storing and querying structured knowledge in a graph format, enabling complex relationship analysis. | `IKnowledgeGraphAdapter.cs` |
| **DocumentProcessing** | Contains services for ingesting, chunking, and indexing documents from various sources to populate the knowledge bases used by the RAG system. | `DocumentIngestionFunction.cs` |

### 🏢 Enterprise Integration

| Component | Description | Key Files |
| :--- | :--- | :--- |
| **OneLakeIntegration** | Provides a dedicated adapter for seamless integration with Microsoft Fabric's OneLake, serving as a unified data lake for the mesh. | `OneLakeIntegrationManager.cs` |
| **EnterpriseConnectors** | A collection of adapters for connecting to other enterprise systems. This also includes the `FeatureFlagManager` for controlling feature rollouts. | `FeatureFlagManager.cs` |

---

## 🚀 Usage Examples

Services in higher layers should **only** interact with the Foundation Layer through its defined ports, which are injected via dependency injection.

### Example: Securely Retrieving a Secret

Here is how a service in the `BusinessApplications` layer might retrieve a secret using the `ISecretsManagementPort`.

```csharp
// In a service class within a higher layer (e.g., BusinessApplications)

public class MyBusinessService
{
    private readonly ISecretsManagementPort _secretsPort;
    private readonly ILogger<MyBusinessService> _logger;

    public MyBusinessService(ISecretsManagementPort secretsPort, ILogger<MyBusinessService> logger)
    {
        _secretsPort = secretsPort;
        _logger = logger;
    }

    public async Task<string> GetExternalApiServiceKeyAsync()
    {
        var secretRequest = new SecretRequest
        {
            SecretName = "ExternalApiServiceKey"
            // VersionId can be specified if needed, otherwise latest is fetched.
        };

        var response = await _secretsPort.GetSecretAsync(secretRequest);

        if (response.IsSuccess)
        {
            _logger.LogInformation("Successfully retrieved API key.");
            return response.SecretValue;
        }
        else
        {
            _logger.LogError("Failed to retrieve API key: {ErrorMessage}", response.ErrorMessage);
            // Handle the error appropriately
            throw new InvalidOperationException("Could not retrieve required secret.");
        }
    }
}
```

---

## 🤝 How to Contribute

Contributions to the Foundation Layer are highly encouraged, especially for adding new adapters or improving the performance and security of existing ones. Please refer to the main [CONTRIBUTING.md](../../CONTRIBUTING.md) file for guidelines on how to submit pull requests.

When adding new components, ensure they follow the established Ports and Adapters pattern.
