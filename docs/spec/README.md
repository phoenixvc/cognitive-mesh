# Cognitive Mesh API Specification

This directory contains the OpenAPI specification for the Cognitive Mesh Convener Backend API.

## Structure

- `common/` - Shared components, security schemes, and error responses
  - `components.yaml` - Shared schemas and models
  - `securitySchemes.yaml` - Authentication and authorization schemes
  - `errorResponses.yaml` - Common error responses

- `services/` - API endpoints grouped by service/domain
  - `champion-discovery.yaml` - Champion discovery and matchmaking
  - `community-pulse.yaml` - Community metrics and analytics
  - `learning-catalyst.yaml` - Learning experiments and resources
  - `innovation-spread.yaml` - Innovation tracking and virality
  - `approval-consent.yaml` - User approvals and consents
  - `provenance.yaml` - Data provenance and lineage
  - `notifications.yaml` - Event notifications system

- `index.yaml` - Root document that combines all components and paths

## Development

### Prerequisites

- Node.js 16+
- npm or yarn

### Setup

1. Install dependencies:
   ```bash
   npm install -g @apidevtools/swagger-cli
   ```

2. Build the bundled OpenAPI specification:
   ```bash
   npm run build:openapi
   ```

### Adding a New Endpoint

1. Identify the appropriate service file in `services/`
2. Add your endpoint definition following the existing patterns
3. Reference shared components from `common/` when possible
4. Run the build script to verify your changes

### Best Practices

- Use `$ref` to reference shared components
- Follow RESTful principles for endpoint design
- Document all parameters, request bodies, and responses
- Include examples for all schemas
- Keep service files focused on a single domain

## Validation

To validate the OpenAPI specification:

```bash
swagger-cli validate docs/spec/index.yaml
```
