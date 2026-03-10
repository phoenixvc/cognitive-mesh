# Audit Logging Component

A robust, scalable, and configurable audit logging solution for .NET applications, built with Entity Framework Core and following SOLID principles.

## Features

- **Flexible Event Logging**: Log any type of audit event with rich metadata
- **Resilient Retry Mechanism**: Built-in exponential backoff with jitter for transient fault handling
- **Health Monitoring**: Integrated health checks for monitoring the audit logging system
- **Async/Await Support**: Fully asynchronous API for high-performance scenarios
- **Dependency Injection**: Seamless integration with .NET's built-in dependency injection
- **Configurable**: Highly configurable through standard .NET configuration
- **Entity Framework Core Support**: Built on top of EF Core for reliable data persistence

## Getting Started

### Installation

Add the AuditLogging package to your project:

```bash
dotnet add package CognitiveMesh.FoundationLayer.AuditLogging
```

### Configuration

1. Add the audit logging services to your application's service collection:

```csharp
// In Startup.cs or Program.cs
services.AddAuditLogging(Configuration);
```

2. Configure the audit logging options in your `appsettings.json`:

```json
{
  "AuditLogging": {
    "ConnectionString": "YourConnectionString",
    "MaxRetryAttempts": 5,
    "InitialRetryDelayMs": 1000,
    "RetryQueueProcessingIntervalSeconds": 60,
    "CommandTimeoutSeconds": 30
  }
}
```

### Usage

#### Basic Logging

```csharp
public class MyService
{
    private readonly IAuditLoggingAdapter _auditLogger;

    public MyService(IAuditLoggingAdapter auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public async Task PerformActionAsync(string userId, string action)
    {
        var auditEvent = new AuditEvent
        {
            EventType = "UserAction",
            UserId = userId,
            Details = $"User performed action: {action}",
            Timestamp = DateTimeOffset.UtcNow
        };

        await _auditLogger.LogEventAsync(auditEvent);
    }
}
```

#### Searching Audit Events

```csharp
public async Task<IEnumerable<AuditEvent>> GetUserActionsAsync(string userId, DateTimeOffset fromDate, DateTimeOffset toDate)
{
    var criteria = new AuditSearchCriteria
    {
        UserId = userId,
        FromDate = fromDate,
        ToDate = toDate,
        EventType = "UserAction",
        SortOrder = SortOrder.Descending,
        PageNumber = 1,
        PageSize = 50
    };

    return await _auditLogger.SearchEventsAsync(criteria);
}
```

### Health Checks

The audit logging component exposes health check endpoints that can be used to monitor the health of the system:

- `/health/audit-db`: Checks the database connection
- `/health/audit-logging`: Performs a comprehensive health check of the audit logging system

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| ConnectionString | The database connection string | Required |
| MaxRetryAttempts | Maximum number of retry attempts for transient failures | 5 |
| InitialRetryDelayMs | Initial delay between retry attempts in milliseconds | 1000 |
| RetryQueueProcessingIntervalSeconds | Interval for processing the retry queue in seconds | 60 |
| CommandTimeoutSeconds | Database command timeout in seconds | 30 |

### Advanced Topics

#### Custom Retry Policy

You can provide a custom retry policy by implementing the `IRetryPolicy` interface and registering it with the service collection:

```csharp
services.AddSingleton<IRetryPolicy, CustomRetryPolicy>();
```

#### Custom Repository

To use a different storage provider, implement the `IAuditEventRepository` interface and register it:

```csharp
services.AddScoped<IAuditEventRepository, CustomAuditEventRepository>();
```

## Best Practices

1. **Use Appropriate Log Levels**: 
   - Use `Debug` for detailed troubleshooting information
   - Use `Information` for normal operational events
   - Use `Warning` for unexpected but handled conditions
   - Use `Error` for failures that prevent normal operation

2. **Handle Failures Gracefully**: 
   - Always implement proper error handling around audit logging calls
   - Use the built-in retry mechanism for transient failures

3. **Monitor Performance**: 
   - Keep an eye on the database performance
   - Consider archiving old audit logs if the table grows too large

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
