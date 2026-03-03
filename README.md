# Cognitive Mesh: Enterprise AI Transformation Framework

[![Build and Analyze](https://github.com/phoenixvc/cognitive-mesh/actions/workflows/build.yml/badge.svg)](https://github.com/phoenixvc/cognitive-mesh/actions/workflows/build.yml)
[![Deploy](https://github.com/phoenixvc/cognitive-mesh/actions/workflows/deploy.yml/badge.svg)](https://github.com/phoenixvc/cognitive-mesh/actions/workflows/deploy.yml)
[![Code Coverage](https://github.com/phoenixvc/cognitive-mesh/actions/workflows/coverage.yml/badge.svg)](https://github.com/phoenixvc/cognitive-mesh/actions/workflows/coverage.yml)
[![codecov](https://codecov.io/gh/phoenixvc/cognitive-mesh/graph/badge.svg)](https://codecov.io/gh/phoenixvc/cognitive-mesh)
[![PRD Status](https://img.shields.io/badge/PRD%20Status-Tracked-blue?link=./docs/prds/PRD-PRIORITY-STATUS.md)](./docs/prds/PRD-PRIORITY-STATUS.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 🎯 Vision & Mission

**Cognitive Mesh** is an enterprise-grade AI transformation framework designed to orchestrate sophisticated, multi-agent cognitive systems. Our mission is to provide a secure, compliant, and philosophically-grounded platform that enables organizations to build, deploy, and govern advanced AI capabilities with confidence and transparency.

The platform integrates spectrum-adaptive intelligence, a complete NIST AI RMF compliance system, and a foundational Zero-Trust security architecture, making it the most advanced and reliable system for regulated and mission-critical enterprise environments.

---

## 🏛️ Core Pillars

The Cognitive Mesh architecture is built on four unwavering pillars that ensure robustness, security, and scalability.

1.  **Layered Hexagonal Architecture:** A clean, decoupled design separating core domain logic from infrastructure. Each of the 5 layers (Foundation, Reasoning, Metacognitive, Agency, Business Applications) has a distinct responsibility, communicating through well-defined ports and adapters.
2.  **Zero-Trust Security by Default:** Security is not an afterthought; it is the foundation. Every request, whether internal or external, is authenticated, authorized, and encrypted, enforcing the principle of least privilege across the entire mesh.
3.  **Ethical & Legal Compliance:** The framework is governed by a comprehensive ethical and legal compliance system, ensuring all operations align with global standards like the GDPR and the EU AI Act, and are grounded in established philosophical principles.
4.  **PRD-Driven Development:** Every component, feature, and architectural decision is guided by a comprehensive portfolio of Product Requirement Documents (PRDs). This ensures systematic, transparent, and priority-driven development.

---

## 🏗️ Architecture Overview

The Cognitive Mesh is organized into five distinct layers, each with a specific role in the cognitive processing pipeline. This separation of concerns ensures modularity, testability, and maintainability.

*   [**`FoundationLayer`**](./src/FoundationLayer/README.md): Provides the core infrastructure, including security, data persistence, audit logging, and communication protocols. It is the bedrock upon which all other layers are built.
*   [**`ReasoningLayer`**](./src/ReasoningLayer/README.md): Contains the cognitive engines responsible for various forms of reasoning—analytical, creative, ethical, and threat intelligence.
*   [**`MetacognitiveLayer`**](./src/MetacognitiveLayer/README.md): The "mind of the mesh," responsible for self-monitoring, continuous learning, performance optimization, and incident response.
*   [**`AgencyLayer`**](./src/AgencyLayer/README.md): Home to autonomous agents that execute tasks, interact with tools, and carry out automated workflows based on the decisions and plans formulated by the other layers.
*   [**`BusinessApplications`**](./src/BusinessApplications/README.md): The outermost layer, which exposes the mesh's capabilities to the outside world through controllers, APIs, and business-specific logic.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/) with the C# Dev Kit
- (Optional) [Azure Subscription](https://azure.microsoft.com) for cloud-native feature development

### Quick Start

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/phoenixvc/cognitive-mesh.git
    cd cognitive-mesh
    ```

2.  **Build the solution:**
    This will restore all NuGet dependencies and compile every project.
    ```sh
    dotnet build CognitiveMesh.sln
    ```

3.  **Run all tests:**
    Verify that the entire system is functioning correctly.
    ```sh
    dotnet test CognitiveMesh.sln
    ```

For advanced build, testing, and utility operations, please see the scripts in the [`./scripts/`](./scripts/) directory.

---

## 🗺️ PRD-Driven Development

All development in the Cognitive Mesh is meticulously planned and tracked through a comprehensive portfolio of **Product Requirement Documents (PRDs)**. This ensures every feature is well-defined, architecturally sound, and aligned with the project's strategic goals.

Our complete PRD portfolio, including implementation priorities, status, and dependencies, is tracked in the master document:

➡️ **[PRD Priority & Implementation Status](./docs/prds/PRD-PRIORITY-STATUS.md)**

---

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

Please refer to our [Contributing Guidelines](./CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests to us.

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for more information.
