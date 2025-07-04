# Package Versions

This document outlines the key package versions used across the Cognitive Mesh solution. All versions are centrally managed in `Directory.Build.props`.

## Core Dependencies

### .NET
- **Target Framework**: `net9.0`
- **Microsoft Extensions**: `10.0.0-preview.5.25277.114`
  - Configuration
  - DependencyInjection
  - Logging
  - Options
  - Hosting

### AI/ML
- **Azure.AI.OpenAI**: `2.2.0-beta.4`
- **Azure.Identity**: `1.12.0`

### Data & Caching
- **StackExchange.Redis**: `2.8.41`
- **NRedisStack**: `1.0.0`

### JSON
- **System.Text.Json**: `8.0.4`
- **Newtonsoft.Json**: `13.0.3`

## Development Dependencies

### Testing
- **xUnit**: `2.8.0`
- **Moq**: `4.20.70`
- **FluentAssertions**: `6.12.0`

### Build & Deployment
- **Microsoft.NET.Test.Sdk**: `17.10.0`
- **coverlet.collector**: `6.0.0`

## Version Management

All package versions are centrally managed in `Directory.Build.props`. To update a version:

1. Update the corresponding version variable in `Directory.Build.props`
2. Run `dotnet restore`
3. Test the solution thoroughly
4. Update this document if necessary

## Security Notes

- Always verify package hashes when restoring
- Regularly check for security updates to dependencies
- Review dependency licenses for compliance
