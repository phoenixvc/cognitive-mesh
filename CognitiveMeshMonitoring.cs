using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Threading.Tasks;

public class CognitiveMeshMonitoring
{
    private readonly TelemetryClient _telemetryClient;
    private readonly FeatureFlagManager _featureFlagManager;

    public CognitiveMeshMonitoring(TelemetryClient telemetryClient, FeatureFlagManager featureFlagManager)
    {
        _telemetryClient = telemetryClient;
        _featureFlagManager = featureFlagManager;
    }

    public IOperationHolder<RequestTelemetry> StartQueryOperation(string query)
    {
        var operation = _telemetryClient.StartOperation<RequestTelemetry>("QueryOperation");
        operation.Telemetry.Properties["Query"] = query;
        operation.Telemetry.Start();
        return operation;
    }

    public void TrackComponentExecution(string componentName, TimeSpan duration, bool success)
    {
        var telemetry = new DependencyTelemetry
        {
            Name = componentName,
            Duration = duration,
            Success = success
        };
        _telemetryClient.TrackDependency(telemetry);
    }

    public void TrackQueryCompletion(IOperationHolder<RequestTelemetry> operation, bool success)
    {
        operation.Telemetry.Success = success;
        _telemetryClient.StopOperation(operation);
    }

    public void TrackException(Exception exception)
    {
        _telemetryClient.TrackException(exception);
    }

    public void TrackEvent(string eventName, string message)
    {
        var telemetry = new EventTelemetry(eventName)
        {
            Message = message
        };
        _telemetryClient.TrackEvent(telemetry);
    }

    public void TrackFeatureFlagUsage(string featureFlagName)
    {
        if (_featureFlagManager.GetType().GetProperty(featureFlagName)?.GetValue(_featureFlagManager) is bool isEnabled && isEnabled)
        {
            var telemetry = new EventTelemetry("FeatureFlagUsage")
            {
                Properties = { { "FeatureFlag", featureFlagName }, { "Status", "Enabled" } }
            };
            _telemetryClient.TrackEvent(telemetry);
        }
    }
}
