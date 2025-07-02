# Business Applications Layer

## 🎯 Overview

Welcome to the **Business Applications Layer**—the primary entry point and "front door" to the Cognitive Mesh platform. This layer exposes the powerful capabilities of the inner cognitive and agentic layers through a secure, well-defined, and robust RESTful API. It serves as the bridge between external clients (such as web frontends, mobile apps, or other enterprise services) and the core logic of the mesh.

The primary mission of the Business Applications Layer is to orchestrate complex business workflows and present them as simple, consumable API endpoints, handling all aspects of request validation, authorization, and data transformation.

---

## 🏛️ Core Responsibilities

This layer is responsible for the following critical functions:

-   **API Exposure:** Providing a comprehensive set of RESTful API endpoints that allow external clients to interact with the Cognitive Mesh.
-   **Request Handling:** Managing the entire lifecycle of an incoming HTTP request, including validation, deserialization into Data Transfer Objects (DTOs), and authentication/authorization checks.
-   **Workflow Orchestration:** Acting as a primary orchestrator by invoking the ports of the inner layers (`AgencyLayer`, `MetacognitiveLayer`, `ReasoningLayer`) in the correct sequence to fulfill a business use case.
-   **Data Transformation:** Mapping internal domain models to external-facing DTOs to ensure that the API contract remains stable and decoupled from the internal data structures.
-   **Business-Specific Logic:** Containing controllers and services that are tailored to specific business domains, such as customer intelligence, decision support, or security management.

---

## 🏗️ Architectural Principles

The Business Applications Layer is the outermost layer in our **Hexagonal (Ports and Adapters)** architecture. It functions as a primary **Adapter** for user-side interactions, specifically for the HTTP protocol.

-   **Dependency Inversion:** This layer depends on the abstractions (ports/interfaces) defined by the inner layers, not on their concrete implementations. This is achieved through dependency injection, which decouples the business logic from the core cognitive engines.
-   **Technology-Specific Implementation:** This layer is intentionally technology-specific, using **ASP.NET Core** to build the RESTful API. This allows the inner layers to remain pure, technology-agnostic domain logic.
-   **Separation of Concerns:** By handling all API-related concerns (routing, serialization, HTTP status codes), this layer allows the inner layers to focus solely on their core responsibilities, leading to a cleaner, more maintainable codebase.

---

## 🧩 Application Portfolio

This layer is organized into a portfolio of distinct business applications, each corresponding to a major functional area of the Cognitive Mesh.

| Application | Description | Key Controller(s) |
| :--- | :--- | :--- |
| **Security** | Exposes endpoints for the Zero-Trust Security Framework, including authentication, authorization, risk scoring, and compliance reporting. | [`SecurityController.cs`](./Security/Controllers/SecurityController.cs) |
| **Customer Intelligence** | Provides services for understanding and interacting with customers, including inquiry handling, conversation management, and troubleshooting. | [`CustomerServiceController.cs`](./CustomerIntelligence/CustomerServiceController.cs) |
| **Decision Support** | Offers advanced decision-making aids, such as situation analysis, options generation, and causal relationship modeling. | [`DecisionSupportController.cs`](./DecisionSupport/DecisionSupportController.cs) |
| **Knowledge Management** | Provides endpoints for interacting with the mesh's knowledge base, including document ingestion, querying, and management. | [`KnowledgeManager.cs`](./KnowledgeManagement/KnowledgeManager.cs) |
| **Process Automation** | Exposes workflows for automating complex business processes by leveraging the underlying agentic and cognitive capabilities of the mesh. | [`BusinessProcessAutomator.cs`](./ProcessAutomation/BusinessProcessAutomator.cs) |
| **Research & Analysis** | Provides services for automating knowledge work, such as conducting research, synthesizing documents, and generating content. | [`KnowledgeWorkController.cs`](./ResearchAnalysis/KnowledgeWorkController.cs) |

---

## 🚀 Usage Example (API Interaction)

External clients interact with the Business Applications Layer by making standard HTTP requests to its exposed endpoints.

### Example: Calculating a Dynamic Risk Score

Here is an example of how a client would call the `SecurityController` to get a dynamic risk score for a user action.

**Request:**

```bash
curl -X POST "https://api.cognitivemesh.com/api/v1/security/risk/score" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <your_jwt_token>" \
     -d '{
           "subjectId": "user-123",
           "action": "read",
           "resourceId": "sensitive-customer-record-456",
           "context": {
             "location": "Unknown",
             "resourceSensitivity": "High",
             "recentLoginFailures": 0
           }
         }'
```

**Response (200 OK):**

```json
{
  "riskScore": 75,
  "riskLevel": "High",
  "contributingFactors": [
    "Baseline score for authenticated user (+20)",
    "High-risk or unknown location (+25)",
    "Accessing high-sensitivity resource (+30)"
  ]
}
```

---

## 🤝 How to Contribute

Adding a new business application to this layer is a straightforward process that follows our established architectural patterns.

1.  **Create a New Application Folder:** Add a new directory under `src/BusinessApplications/` for your new domain (e.g., `src/BusinessApplications/MyNewApp/`).
2.  **Define the Controller:** Create a new ASP.NET Core controller within your application folder (e.g., `MyNewApp/Controllers/MyNewAppController.cs`).
3.  **Use Dependency Injection:** Inject the ports/interfaces from the inner layers that your controller needs to orchestrate its workflow.
4.  **Define DTOs:** Create any necessary Data Transfer Objects (DTOs) for your API's request and response models. It is best practice to place these in a `Models/` sub-folder.
5.  **Implement Endpoints:** Add methods to your controller for each API endpoint, ensuring you handle request validation, authorization, and error handling.
6.  **Add Unit and Integration Tests:** Provide comprehensive tests for your new controller to ensure it functions correctly and integrates properly with the inner layers.

Please refer to the main [CONTRIBUTING.md](../../CONTRIBUTING.md) file for general contribution guidelines.
