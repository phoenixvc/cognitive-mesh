# UI Layer: Plugin-Based Dashboard Framework

## 🎯 Overview

Welcome to the **UI Layer**—the extensible, secure, and user-centric presentation framework for the Cognitive Mesh. This layer is not a monolithic frontend application but a sophisticated **plugin-based dashboard system** that allows for the dynamic composition of user interfaces from a secure, curated library of widgets.

The primary mission of the UI Layer is to provide a robust framework for developers to build, submit, and manage UI components (widgets) and for end-users to create personalized, role-based dashboards that give them a window into the power of the Cognitive Mesh.

---

## 🏛️ Core Concepts

The UI Layer is built on a set of powerful, decoupled concepts that ensure security, extensibility, and a consistent user experience.

1.  **Widgets as Secure Plugins:** Every piece of UI functionality, from a simple chart to a complex interactive panel, is encapsulated as a self-contained **Widget**. Each widget has a `WidgetDefinition` that describes its metadata, permissions, and dependencies. This plugin architecture allows for a vibrant ecosystem of capabilities.

2.  **Personalized Dashboard Layouts:** Users can create and customize their own dashboards by arranging instances of available widgets. The `DashboardLayoutService` manages the state, position, and configuration of each `WidgetInstance` for every user, providing a fully personalized experience.

3.  **Curated Plugin Marketplace:** To ensure security and quality, all new widgets must be submitted to a **Plugin Marketplace**. Here, they undergo a rigorous admin approval process, including code signing and security validation, before they are made available to users.

4.  **Sandwich Pattern Orchestration:** Widgets do not call backend APIs directly. Instead, they interact with the `PluginOrchestrator`, which uses a "sandwich pattern" to wrap every call with security, validation, and logging. This ensures that all plugin behavior is secure and compliant with the mesh's foundational policies.

---

## 🏗️ Architectural Principles

The UI Layer is designed as a set of backend services that support a thin, dynamic frontend client. It follows the **Hexagonal (Ports and Adapters)** architecture, where core services are decoupled from the web framework.

-   **Backend for Frontend (BFF) Pattern:** The UI Layer acts as a BFF, providing a tailored API for frontend clients to consume. It orchestrates calls to the inner layers (`BusinessApplications`, `MetacognitiveLayer`, etc.) to gather the data needed by the widgets.
-   **Extensibility by Design:** The entire system is built around the `IWidgetRegistry` and the plugin marketplace, making it easy to add new functionality without modifying the core framework.
-   **Security First:** The `PluginOrchestrator` and the admin approval workflow ensure that the extensible nature of the UI does not compromise the security and integrity of the Cognitive Mesh.

### End-to-End Workflow

1.  A frontend client authenticates the user and requests their dashboard layout from the `DashboardLayoutService`.
2.  The `DashboardLayoutService` retrieves the user's `DashboardLayout`, which contains a list of `WidgetInstance` configurations.
3.  For each widget in the layout, the service queries the `IWidgetRegistry` to get the corresponding `WidgetDefinition`.
4.  The service returns the complete layout and all necessary widget definitions to the client.
5.  The client dynamically renders the widgets based on the provided data.
6.  When a user interacts with a widget that needs backend data, the widget makes a call to the `PluginOrchestrator`, which securely handles the request to the appropriate inner-layer API.

---

## 🧩 Major Components

The UI Layer is composed of several core services and models that work together to deliver the dynamic dashboard experience.

| Component | Description | Key Files |
| :--- | :--- | :--- |
| **WidgetRegistry** | The central service for managing the lifecycle of all widgets. It handles registration, discovery, and versioning of `WidgetDefinition`s. | `IWidgetRegistry.cs`, `WidgetRegistry.cs` |
| **DashboardLayoutService** | Manages the creation, retrieval, and persistence of user-specific dashboard layouts. It orchestrates calls to the registry to assemble the final dashboard configuration. | `DashboardLayoutService.cs` |
| **PluginOrchestrator** | A secure gateway for widgets to interact with backend APIs. It implements the "sandwich pattern" by wrapping each call with pre-execution (auth, validation) and post-execution (logging, transformation) logic. | `PluginOrchestrator.cs` |
| **Plugin Marketplace** | A suite of services and models for managing the submission, review, and approval of new widgets. | `PluginSubmission.cs`, `MarketplaceEntry.cs` |
| **AgencyWidgets** | A collection of pre-built, specialized widgets that provide interfaces for core Cognitive Mesh capabilities (e.g., Adaptive Balance, NIST RMF). | `IAgencyWidgetAdapters.cs` (conceptual adapter interface) |
| **Core Models** | The data structures that define the UI Layer's entities. | `WidgetDefinition.cs`, `WidgetInstance.cs`, `DashboardLayout.cs` |

---

## 🚀 Developer Workflow: Creating a New Widget

Creating and deploying a new widget follows a clear, secure workflow:

1.  **Develop the Widget:** A developer builds the UI component using a standard frontend framework (e.g., React, Vue, Angular).
2.  **Create a Definition:** The developer creates a `WidgetDefinition` JSON object that describes the widget's name, version, permissions, and required configuration parameters.
3.  **Submit to Marketplace:** The developer submits the widget's code bundle and its `WidgetDefinition` as a `PluginSubmission`.
4.  **Admin Review:** A system administrator reviews the submission. This process includes:
    -   **Security Scan:** Automated scanning of the code for vulnerabilities.
    -   **Code Review:** Manual review for quality and adherence to standards.
    -   **Code Signing:** Signing the widget bundle to ensure its integrity.
5.  **Approval & Registration:** Once approved, a new `MarketplaceEntry` is created, and the widget is registered with the `IWidgetRegistry`.
6.  **User Installation:** Users can now discover the new widget in the marketplace and add it to their dashboards.

---

## 💻 Usage Example

A frontend client would interact with the UI Layer's services like this:

### Conceptual Frontend Code (e.g., TypeScript)

```typescript
// 1. Define types for our models
interface WidgetInstance {
  instanceId: string;
  widgetId: string;
  position: { x: number; y: number; };
  size: { width: number; height: number; };
}

interface WidgetDefinition {
  id: string;
  name: string;
  componentUrl: string; // URL to the widget's code bundle
}

interface DashboardLayoutResponse {
  layoutId: string;
  instances: WidgetInstance[];
  definitions: Record<string, WidgetDefinition>;
}

// 2. Fetch the user's dashboard layout
async function getDashboardLayout(userId: string, apiToken: string): Promise<DashboardLayoutResponse> {
  const response = await fetch(`/api/v1/uilayer/dashboard/${userId}`, {
    headers: { 'Authorization': `Bearer ${apiToken}` }
  });
  if (!response.ok) {
    throw new Error('Failed to fetch dashboard layout.');
  }
  return response.json();
}

// 3. Dynamically render the dashboard
async function renderDashboard(userId: string, apiToken: string) {
  try {
    const { instances, definitions } = await getDashboardLayout(userId, apiToken);
    const dashboardElement = document.getElementById('dashboard');
    dashboardElement.innerHTML = ''; // Clear previous content

    for (const instance of instances) {
      const definition = definitions[instance.widgetId];
      if (definition) {
        // Create a container for the widget
        const widgetContainer = document.createElement('div');
        widgetContainer.style.left = `${instance.position.x}px`;
        widgetContainer.style.top = `${instance.position.y}px`;
        
        // Dynamically load and mount the widget component from its URL
        // (This is a simplified example of dynamic component loading)
        const widgetComponent = await import(definition.componentUrl);
        widgetComponent.mount(widgetContainer, { /* props */ });
        
        dashboardElement.appendChild(widgetContainer);
      }
    }
  } catch (error) {
    console.error("Failed to render dashboard:", error);
  }
}
```

---

## 🤝 How to Contribute

The UI Layer is designed for community contribution. The primary way to contribute is by developing new widgets. Follow the **Developer Workflow** outlined above. For contributions to the core framework (e.g., improving the `WidgetRegistry` or `PluginOrchestrator`), please refer to the main [CONTRIBUTING.md](../../CONTRIBUTING.md) file.
