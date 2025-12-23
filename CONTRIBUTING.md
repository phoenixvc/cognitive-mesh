# Contributing to Cognitive Mesh

Thank you for your interest in contributing to **Cognitive Mesh**! We're excited to have you join us in building an enterprise-grade AI transformation framework. This document provides guidelines and instructions for contributing to the project.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [PRD-Driven Development](#prd-driven-development)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Pull Request Process](#pull-request-process)
- [Architecture Guidelines](#architecture-guidelines)
- [Documentation](#documentation)
- [Community and Support](#community-and-support)

---

## 📜 Code of Conduct

By participating in this project, you agree to maintain a respectful, inclusive, and professional environment. We are committed to:

- **Respect**: Treat all contributors with respect and courtesy
- **Inclusivity**: Welcome contributors from all backgrounds and experience levels
- **Collaboration**: Work together constructively to improve the project
- **Professionalism**: Maintain high standards of technical and interpersonal conduct

---

## 🤝 How Can I Contribute?

There are many ways to contribute to Cognitive Mesh:

### Reporting Bugs

- **Check existing issues** to avoid duplicates
- **Use the issue template** (if available) to provide all necessary information
- **Include**:
  - Clear description of the problem
  - Steps to reproduce
  - Expected vs. actual behavior
  - Environment details (.NET version, OS, etc.)
  - Relevant logs or error messages

### Suggesting Enhancements

- **Search existing issues** to see if your idea has been proposed
- **Open a new issue** with the `enhancement` label
- **Provide**:
  - Clear use case and motivation
  - Proposed solution or approach
  - Potential impact and benefits
  - Examples or mockups (if applicable)

### Contributing Code

- **Fix bugs** listed in issues
- **Implement features** aligned with our PRDs
- **Improve documentation**
- **Enhance test coverage**
- **Optimize performance**

### Improving Documentation

- Fix typos or clarify existing documentation
- Add examples or tutorials
- Update outdated information
- Translate documentation (if multilingual support is planned)

---

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/) with the C# Dev Kit extension
- [Git](https://git-scm.com/)
- (Optional) [Azure Subscription](https://azure.microsoft.com) for cloud-native feature development

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub

2. **Clone your fork** locally:
   ```sh
   git clone https://github.com/YOUR_USERNAME/cognitive-mesh.git
   cd cognitive-mesh
   ```

3. **Add upstream remote**:
   ```sh
   git remote add upstream https://github.com/JustAGhosT/cognitive-mesh.git
   ```

4. **Install dependencies and build**:
   ```sh
   dotnet restore CognitiveMesh.sln
   dotnet build CognitiveMesh.sln
   ```

5. **Run tests** to verify your setup:
   ```sh
   dotnet test CognitiveMesh.sln
   ```

6. **Set up environment variables** (if needed):
   - Copy `.env.example` to `.env`
   - Configure any necessary API keys or settings

---

## 🔄 Development Workflow

### Creating a Branch

Always create a new branch for your work:

```sh
# Update your local main branch
git checkout main
git pull upstream main

# Create a new feature branch
git checkout -b feature/your-feature-name
# or for bug fixes
git checkout -b fix/issue-description
```

### Branch Naming Conventions

- `feature/` - New features or enhancements
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring
- `test/` - Test additions or improvements
- `chore/` - Maintenance tasks

Example: `feature/add-ethical-reasoning-engine` or `fix/security-policy-null-check`

### Making Changes

1. **Make your changes** following the [coding standards](#coding-standards)
2. **Write or update tests** for your changes
3. **Run tests locally** to ensure everything passes
4. **Update documentation** if needed
5. **Commit your changes** using [proper commit messages](#commit-message-guidelines)

### Keeping Your Branch Updated

Regularly sync your branch with the upstream main:

```sh
git fetch upstream
git rebase upstream/main
```

---

## 📑 PRD-Driven Development

Cognitive Mesh follows a **PRD-Driven Development** approach. All major features and architectural decisions are guided by Product Requirement Documents (PRDs).

### Before Starting Development

1. **Review the [PRD Priority & Status](./docs/prds/PRD-PRIORITY-STATUS.md)** document
2. **Check if there's a PRD** for the feature you want to implement
3. **If no PRD exists** for a significant feature, consider opening an issue to discuss creating one
4. **Understand the architecture** - review the relevant layer's README in `./src/`

### PRD Structure

When working on PRD-tracked features:

- Understand the **objectives and requirements**
- Follow the **architectural patterns** specified
- Consider **dependencies** on other PRDs
- Align with **security and compliance** requirements

---

## 💻 Coding Standards

### C# Style Guidelines

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use **meaningful names** for variables, methods, and classes
- Keep methods **focused and concise** (single responsibility principle)
- Use **XML documentation comments** for public APIs
- Prefer **async/await** for asynchronous operations

### Architecture Principles

Cognitive Mesh follows a **Layered Hexagonal Architecture**:

1. **FoundationLayer**: Core infrastructure (security, persistence, logging)
2. **ReasoningLayer**: Cognitive engines (analytical, creative, ethical reasoning)
3. **MetacognitiveLayer**: Self-monitoring and continuous learning
4. **AgencyLayer**: Autonomous agents and task execution
5. **BusinessApplications**: APIs and business logic

**Key Principles**:
- Maintain **separation of concerns** between layers
- Use **dependency injection** for loose coupling
- Implement **ports and adapters** pattern for external integrations
- Follow **Zero-Trust security** principles

### Code Organization

- Place new code in the appropriate layer
- Use namespaces that reflect the folder structure
- Keep files focused on a single responsibility
- Organize related functionality into cohesive modules

---

## 🧪 Testing Guidelines

Testing is crucial for maintaining quality and reliability.

### Test Types

1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test interactions between components
3. **End-to-End Tests**: Test complete workflows

### Writing Tests

- **Every new feature** should include tests
- **Bug fixes** should include regression tests
- Use **descriptive test names** that explain what is being tested
- Follow **Arrange-Act-Assert (AAA)** pattern
- Mock external dependencies to ensure test isolation

### Running Tests

```sh
# Run all tests
dotnet test CognitiveMesh.sln

# Run tests for a specific project
dotnet test tests/YourTestProject/YourTestProject.csproj

# Run tests with coverage
dotnet test CognitiveMesh.sln /p:CollectCoverage=true
```

### Test Location

Tests should be placed in the `tests/` directory, mirroring the structure of the `src/` directory:

```
tests/
  FoundationLayer/
  ReasoningLayer/
  MetacognitiveLayer/
  AgencyLayer/
  BusinessApplications/
```

For more details, see [docs/TESTING.md](./docs/TESTING.md).

---

## 📝 Commit Message Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks (dependencies, build, etc.)
- `perf`: Performance improvements
- `ci`: CI/CD configuration changes

### Examples

```
feat(reasoning): add ethical reasoning engine

Implement the ethical reasoning engine as specified in PRD-03.
The engine evaluates decisions against ethical frameworks and
compliance requirements.

Closes #123
```

```
fix(security): resolve null reference in policy evaluation

Handle null context objects in security policy engine to prevent
crashes during authorization checks.

Fixes #456
```

### Guidelines

- Use **present tense** ("add feature" not "added feature")
- Use **imperative mood** ("move cursor to..." not "moves cursor to...")
- Keep the subject line **under 72 characters**
- Reference issues and PRs in the footer

---

## 🔀 Pull Request Process

### Before Submitting

1. ✅ **All tests pass** locally
2. ✅ **Code follows** coding standards
3. ✅ **Documentation** is updated (if applicable)
4. ✅ **Commits are clean** and follow commit message guidelines
5. ✅ **Branch is up-to-date** with upstream main

### Creating a Pull Request

1. **Push your branch** to your fork:
   ```sh
   git push origin feature/your-feature-name
   ```

2. **Open a Pull Request** on GitHub from your fork to the upstream repository

3. **Fill out the PR template** with:
   - Clear description of changes
   - Related issue numbers
   - Testing performed
   - Screenshots or examples (if applicable)

### PR Title Format

Follow the same format as commit messages:
```
feat(scope): brief description of changes
```

### Review Process

- **Maintainers will review** your PR
- **Address feedback** promptly and professionally
- **Make requested changes** in new commits (don't force-push during review)
- **Respond to comments** to facilitate discussion
- Once approved, a maintainer will **merge your PR**

### CI/CD Checks

Your PR must pass:
- ✅ Build verification
- ✅ All tests
- ✅ Code linting (if configured)
- ✅ Security scans

---

## 🏗️ Architecture Guidelines

### Layer-Specific Guidelines

#### FoundationLayer
- Focus on **infrastructure concerns** (security, persistence, communication)
- Ensure **Zero-Trust security** principles are maintained
- Provide **clear abstractions** for other layers

#### ReasoningLayer
- Implement **cognitive engines** for various reasoning types
- Maintain **statistical rigor** in reasoning algorithms
- Ensure **auditability** of reasoning processes

#### MetacognitiveLayer
- Focus on **self-monitoring** and **continuous improvement**
- Implement **performance optimization** strategies
- Handle **incident response** and anomaly detection

#### AgencyLayer
- Create **autonomous agents** that execute tasks
- Integrate with **external tools** safely and securely
- Implement **workflow orchestration**

#### BusinessApplications
- Expose **well-designed APIs**
- Follow **API versioning** best practices (see [docs/API_VERSIONING.md](./docs/API_VERSIONING.md))
- Implement **proper error handling** and validation

### Security Considerations

- **Never commit secrets** or sensitive information
- Use **secure coding practices** to prevent vulnerabilities
- Follow **Zero-Trust security** principles
- Validate and sanitize **all inputs**
- Implement **proper authentication and authorization**

---

## 📚 Documentation

Good documentation is essential for the success of the project.

### Types of Documentation

1. **Code Comments**: For complex logic or non-obvious implementations
2. **XML Documentation**: For all public APIs
3. **README files**: In each major directory explaining the component
4. **Architecture docs**: In the `./docs/` directory
5. **API Documentation**: Generated from code comments

### Documentation Standards

- Use **clear, concise language**
- Include **examples** where helpful
- Keep documentation **up-to-date** with code changes
- Follow **markdown best practices**

### Updating Documentation

When making changes that affect:
- **Public APIs**: Update XML documentation
- **Architecture**: Update relevant docs in `./docs/`
- **User-facing features**: Update README or user guides
- **Configuration**: Update configuration documentation

---

## 🌐 Community and Support

### Getting Help

- **Documentation**: Start with the [README](./README.md) and docs in `./docs/`
- **Issues**: Search existing issues for similar questions
- **Discussions**: Use GitHub Discussions (if enabled) for general questions

### Staying Connected

- **Watch** the repository for updates
- **Star** the repository if you find it useful
- **Follow** the project for announcements

### Recognition

All contributors are valued and recognized:
- Contributors are listed in the project
- Significant contributions are highlighted in release notes
- We appreciate all forms of contribution, from code to documentation to bug reports

---

## 🎉 Thank You!

Thank you for contributing to Cognitive Mesh! Your efforts help build a better, more robust AI transformation framework for the enterprise community. Every contribution, no matter how small, makes a difference.

If you have questions or need assistance, don't hesitate to reach out by opening an issue.

**Happy Coding!** 🚀

---

## 📄 License

By contributing to Cognitive Mesh, you agree that your contributions will be licensed under the [MIT License](./LICENSE) that covers this project.
