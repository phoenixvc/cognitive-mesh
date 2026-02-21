using CognitiveMesh.MetacognitiveLayer.Telemetry.Adapters;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Engines;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Infrastructure;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Models;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Ports;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CognitiveMesh.Tests.MetacognitiveLayer.Telemetry;

/// <summary>
/// Unit tests for <see cref="OpenTelemetryAdapter"/>, covering constructor validation,
/// the static <see cref="OpenTelemetryAdapter.ConfigureOpenTelemetry"/> method, and
/// the <see cref="ServiceCollectionExtensions.AddCognitiveMeshTelemetry"/> DI registration.
/// </summary>
public sealed class OpenTelemetryAdapterTests
{
    private readonly TelemetryEngine _engine;
    private readonly ILogger<OpenTelemetryAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OpenTelemetryAdapterTests"/> with default dependencies.
    /// </summary>
    public OpenTelemetryAdapterTests()
    {
        _engine = new TelemetryEngine(NullLogger<TelemetryEngine>.Instance);
        _logger = NullLogger<OpenTelemetryAdapter>.Instance;
    }

    // -----------------------------------------------------------------
    // Constructor tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies the constructor throws <see cref="ArgumentNullException"/> when engine is null.
    /// </summary>
    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        var act = () => new OpenTelemetryAdapter(null!, _logger);

        act.Should().Throw<ArgumentNullException>().WithParameterName("engine");
    }

    /// <summary>
    /// Verifies the constructor throws <see cref="ArgumentNullException"/> when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new OpenTelemetryAdapter(_engine, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Verifies the constructor succeeds with valid parameters.
    /// </summary>
    [Fact]
    public void Constructor_ValidParams_CreatesInstance()
    {
        var adapter = new OpenTelemetryAdapter(_engine, _logger);

        adapter.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the <see cref="OpenTelemetryAdapter.Engine"/> property returns the injected engine.
    /// </summary>
    [Fact]
    public void Engine_AfterConstruction_ReturnsSameEngineInstance()
    {
        var adapter = new OpenTelemetryAdapter(_engine, _logger);

        adapter.Engine.Should().BeSameAs(_engine);
    }

    // -----------------------------------------------------------------
    // ConfigureOpenTelemetry tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="OpenTelemetryAdapter.ConfigureOpenTelemetry"/> throws
    /// when services is null.
    /// </summary>
    [Fact]
    public void ConfigureOpenTelemetry_NullServices_ThrowsArgumentNullException()
    {
        var config = new TelemetryConfiguration();

        var act = () => OpenTelemetryAdapter.ConfigureOpenTelemetry(null!, config);

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    /// <summary>
    /// Verifies that <see cref="OpenTelemetryAdapter.ConfigureOpenTelemetry"/> throws
    /// when configuration is null.
    /// </summary>
    [Fact]
    public void ConfigureOpenTelemetry_NullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        var act = () => OpenTelemetryAdapter.ConfigureOpenTelemetry(services, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    /// <summary>
    /// Verifies that <see cref="OpenTelemetryAdapter.ConfigureOpenTelemetry"/> does not throw
    /// with valid default configuration.
    /// </summary>
    [Fact]
    public void ConfigureOpenTelemetry_DefaultConfig_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var config = new TelemetryConfiguration();

        var act = () => OpenTelemetryAdapter.ConfigureOpenTelemetry(services, config);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="OpenTelemetryAdapter.ConfigureOpenTelemetry"/> does not throw
    /// with custom configuration values.
    /// </summary>
    [Fact]
    public void ConfigureOpenTelemetry_CustomConfig_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var config = new TelemetryConfiguration
        {
            OtlpEndpoint = "http://custom-collector:4317",
            ServiceName = "TestService",
            ServiceVersion = "2.0.0",
            EnableConsoleExporter = true,
            SamplingRatio = 0.5
        };

        var act = () => OpenTelemetryAdapter.ConfigureOpenTelemetry(services, config);

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------
    // AddCognitiveMeshTelemetry (ServiceCollectionExtensions) tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ServiceCollectionExtensions.AddCognitiveMeshTelemetry"/>
    /// registers <see cref="TelemetryEngine"/> as a singleton.
    /// </summary>
    [Fact]
    public void AddCognitiveMeshTelemetry_ValidArgs_RegistersTelemetryEngine()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = BuildConfiguration();

        services.AddCognitiveMeshTelemetry(configuration);

        services.Should().Contain(sd =>
            sd.ServiceType == typeof(TelemetryEngine) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that <see cref="ServiceCollectionExtensions.AddCognitiveMeshTelemetry"/>
    /// registers <see cref="ITelemetryPort"/> as a singleton.
    /// </summary>
    [Fact]
    public void AddCognitiveMeshTelemetry_ValidArgs_RegistersITelemetryPort()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = BuildConfiguration();

        services.AddCognitiveMeshTelemetry(configuration);

        services.Should().Contain(sd =>
            sd.ServiceType == typeof(ITelemetryPort) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that <see cref="ServiceCollectionExtensions.AddCognitiveMeshTelemetry"/>
    /// registers <see cref="OpenTelemetryAdapter"/> as a singleton.
    /// </summary>
    [Fact]
    public void AddCognitiveMeshTelemetry_ValidArgs_RegistersOpenTelemetryAdapter()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = BuildConfiguration();

        services.AddCognitiveMeshTelemetry(configuration);

        services.Should().Contain(sd =>
            sd.ServiceType == typeof(OpenTelemetryAdapter) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that <see cref="ITelemetryPort"/> resolves to <see cref="TelemetryEngine"/>
    /// from the built service provider.
    /// </summary>
    [Fact]
    public void AddCognitiveMeshTelemetry_BuildProvider_ITelemetryPortResolvesToTelemetryEngine()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = BuildConfiguration();
        services.AddCognitiveMeshTelemetry(configuration);

        using var provider = services.BuildServiceProvider();
        var port = provider.GetRequiredService<ITelemetryPort>();

        port.Should().BeOfType<TelemetryEngine>();
    }

    /// <summary>
    /// Verifies that the same <see cref="TelemetryEngine"/> singleton is returned
    /// for both the concrete type and the <see cref="ITelemetryPort"/> interface.
    /// </summary>
    [Fact]
    public void AddCognitiveMeshTelemetry_BuildProvider_SameInstanceForPortAndEngine()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = BuildConfiguration();
        services.AddCognitiveMeshTelemetry(configuration);

        using var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<TelemetryEngine>();
        var port = provider.GetRequiredService<ITelemetryPort>();

        port.Should().BeSameAs(engine);
    }

    /// <summary>
    /// Verifies that <see cref="ServiceCollectionExtensions.AddCognitiveMeshTelemetry"/>
    /// throws when services is null.
    /// </summary>
    [Fact]
    public void AddCognitiveMeshTelemetry_NullServices_ThrowsArgumentNullException()
    {
        IServiceCollection services = null!;
        var configuration = BuildConfiguration();

        var act = () => services.AddCognitiveMeshTelemetry(configuration);

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    /// <summary>
    /// Verifies that <see cref="ServiceCollectionExtensions.AddCognitiveMeshTelemetry"/>
    /// throws when configuration is null.
    /// </summary>
    [Fact]
    public void AddCognitiveMeshTelemetry_NullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        var act = () => services.AddCognitiveMeshTelemetry(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? overrides = null)
    {
        var defaults = new Dictionary<string, string?>
        {
            ["Telemetry:OtlpEndpoint"] = "http://localhost:4317",
            ["Telemetry:ServiceName"] = "CognitiveMesh.Tests",
            ["Telemetry:ServiceVersion"] = "0.0.1-test",
            ["Telemetry:EnableConsoleExporter"] = "false",
            ["Telemetry:SamplingRatio"] = "1.0"
        };

        if (overrides is not null)
        {
            foreach (var kv in overrides)
            {
                defaults[kv.Key] = kv.Value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(defaults)
            .Build();
    }
}
