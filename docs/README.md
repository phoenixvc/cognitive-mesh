# Cognitive Mesh API Documentation

This directory contains the OpenAPI specification for the Cognitive Mesh API, organized in a modular structure for better maintainability.

## Project Structure

```
docs/
├── openapi.yaml             # Bundled OpenAPI specification (auto-generated)
├── spec/
│   ├── README.md           # This file
│   ├── index.yaml          # Root OpenAPI document that imports all components
│   ├── common/             # Shared schemas and components
│   │   ├── components.yaml
│   │   ├── securitySchemes.yaml
│   │   └── errorResponses.yaml
│   └── services/           # API endpoints grouped by domain
│       ├── champion-discovery.yaml
│       ├── community-pulse.yaml
│       ├── learning-catalyst.yaml
│       ├── innovation-spread.yaml
│       ├── approval-consent.yaml
│       ├── provenance.yaml
│       └── notifications.yaml
└── tools/
    └── build-openapi.js    # Script to bundle the OpenAPI spec
```

## Getting Started

### Prerequisites

- Node.js 16+
- npm or yarn

### Installation

1. Install dependencies:

```bash
cd docs
npm install
```

### Development

1. **Build the OpenAPI specification**:
   ```bash
   npm run build
   ```
   This generates the bundled `openapi.yaml` file.

2. **Watch for changes** (auto-rebuild on file changes):
   ```bash
   npm run build:watch
   ```

3. **Validate the specification**:
   ```bash
   npm run validate
   ```

4. **Preview documentation** (requires Redocly CLI):
   ```bash
   npm run serve
   ```
   Then open http://localhost:8080 in your browser.

## Adding a New Endpoint

1. **Choose the appropriate service file** in `spec/services/` based on the domain of your endpoint.
2. **Add your endpoint definition** following the existing patterns.
3. **Reference shared components** from `spec/common/` when possible.
4. **Run the build script** to verify your changes:
   ```bash
   npm run build
   ```

## Best Practices

- **Organization**: Keep related endpoints together in the same service file.
- **Reusability**: Define common schemas in `common/components.yaml`.
- **Consistency**: Follow the same patterns for similar endpoints.
- **Documentation**: Include clear descriptions and examples for all endpoints and schemas.
- **Validation**: Run the validation script before committing changes.

## Version Control

- The `openapi.yaml` file is generated and should not be modified directly.
- All changes should be made to the source files in the `spec/` directory.
- The build script is run automatically as a pre-commit hook to ensure the generated file is always up to date.

## Deployment

The OpenAPI specification is automatically built and deployed as part of the CI/CD pipeline. The generated `openapi.yaml` file is used to generate API documentation and client libraries.

## License

Proprietary - © Cognitive Mesh, Inc. All rights reserved.
