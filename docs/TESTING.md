# API Testing Guide

This document provides an overview of the testing strategy and instructions for running different types of tests for the Cognitive Mesh Convener Backend API.

## Table of Contents

- [Test Types](#test-types)
- [Prerequisites](#prerequisites)
- [Running Tests](#running-tests)
- [Test Descriptions](#test-descriptions)
- [Performance Testing](#performance-testing)
- [Concurrency Testing](#concurrency-testing)
- [Rate Limiting Tests](#rate-limiting-tests)
- [Test Data](#test-data)
- [Continuous Integration](#continuous-integration)
- [Best Practices](#best-practices)

## Test Types

1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test interactions between components
3. **Performance Tests**: Test API performance under load
4. **Concurrency Tests**: Test handling of concurrent requests
5. **Rate Limiting Tests**: Test API rate limiting functionality

## Prerequisites

- Node.js 16+
- npm or yarn
- k6 (for performance testing)
- Access to a running instance of the API

## Running Tests

### Install Dependencies

```bash
npm install
```

### Run Unit Tests

```bash
npm test
```

### Run Performance Tests

Run all performance tests:

```bash
npm run test:performance
```

Run specific test scenarios:

```bash
# Large payload tests
k6 run tests/performance/large-payloads.test.js

# Concurrency tests
k6 run tests/performance/concurrency.test.js

# Rate limiting tests
k6 run tests/performance/rate-limiting.test.js
```

### Environment Variables

Configure tests using environment variables:

```bash
export BASE_URL=http://localhost:3000/v1
export AUTH_TOKEN=your-auth-token
export RATE_LIMIT=100  # requests per minute
```

## Test Descriptions

### Unit Tests

- **Location**: `__tests__/unit/`
- **Purpose**: Test individual functions and components in isolation
- **Coverage**:
  - Input validation
  - Business logic
  - Error handling
  - Utility functions

### Integration Tests

- **Location**: `__tests__/integration/`
- **Purpose**: Test API endpoints and their interactions
- **Coverage**:
  - API endpoints
  - Database interactions
  - External service integrations
  - Authentication and authorization

## Performance Testing

### Large Payload Tests

Tests the API's ability to handle requests with large payloads.

**Scenarios**:
- Small payloads (10KB)
- Medium payloads (100KB)
- Large payloads (1MB+)

**Metrics Tracked**:
- Response times
- Success rates
- Error rates
- Memory usage

### Concurrency Tests

Tests how the API handles multiple simultaneous requests.

**Scenarios**:
- Low concurrency (10 concurrent users)
- Medium concurrency (50 concurrent users)
- High concurrency (100+ concurrent users)

**Metrics Tracked**:
- Requests per second
- Error rates
- Response time percentiles
- System resource usage

## Rate Limiting Tests

Tests the API's rate limiting functionality.

**Scenarios**:
- Requests within rate limit
- Requests exceeding rate limit
- Rate limit reset behavior
- Multiple clients with different rate limits

**Metrics Tracked**:
- Rate limit headers
- 429 responses
- Retry-after headers
- Successful vs. rate-limited requests

## Test Data

Test data is generated dynamically for most tests. For specific test cases, you can find test data in the `test-data/` directory.

## Continuous Integration

Tests are automatically run on pull requests and merges to the main branch. The CI pipeline includes:

1. Linting
2. Unit tests
3. Integration tests
4. Performance tests (on schedule)
5. Security scanning

## Best Practices

1. **Isolate Tests**: Each test should be independent and not rely on the state from other tests.
2. **Clean Up**: Always clean up test data after tests complete.
3. **Use Mocks**: Mock external services to make tests more reliable and faster.
4. **Test Edge Cases**: Include tests for edge cases and error conditions.
5. **Performance Baselines**: Establish performance baselines and monitor for regressions.
6. **Documentation**: Keep test documentation up to date.
7. **Security**: Include security tests in your test suite.

## Troubleshooting

### Common Issues

1. **Tests Failing**:
   - Check the test logs for specific error messages
   - Verify that all services are running
   - Check for network connectivity issues

2. **Performance Test Failures**:
   - Check server resource usage
   - Verify network latency
   - Check for external service dependencies

3. **Rate Limiting Issues**:
   - Verify rate limit headers in responses
   - Check for proper 429 responses
   - Verify rate limit reset behavior

### Getting Help

If you encounter issues not covered in this guide, please:

1. Check the project's issue tracker
2. Review the API documentation
3. Contact the development team

## License

This testing documentation is part of the Cognitive Mesh project and is licensed under the [MIT License](LICENSE).
