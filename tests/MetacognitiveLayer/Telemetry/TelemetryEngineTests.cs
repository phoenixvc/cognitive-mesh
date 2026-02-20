using System.Diagnostics;
using CognitiveMesh.MetacognitiveLayer.Telemetry.Engines;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CognitiveMesh.Tests.MetacognitiveLayer.Telemetry;

/// <summary>
/// Unit tests for <see cref="TelemetryEngine"/>, covering activity creation,
/// metric recording, exception handling, status setting, and well-known metric methods.
/// </summary>
public sealed class TelemetryEngineTests : IDisposable
{
    private readonly TelemetryEngine _sut;
    private readonly ActivityListener _listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryEngineTests"/> class.
    /// Registers an <see cref="ActivityListener"/> so that <see cref="ActivitySource.StartActivity(string, ActivityKind)"/>
    /// returns non-null activities during tests.
    /// </summary>
    public TelemetryEngineTests()
    {
        _sut = new TelemetryEngine(NullLogger<TelemetryEngine>.Instance);

        // Register a listener that samples every activity so StartActivity returns non-null
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == TelemetryEngine.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(_listener);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _listener.Dispose();
        _sut.Dispose();
    }

    // -----------------------------------------------------------------
    // Constructor tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies the constructor throws <see cref="ArgumentNullException"/> when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new TelemetryEngine(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Verifies the constructor succeeds with a valid logger.
    /// </summary>
    [Fact]
    public void Constructor_ValidLogger_CreatesInstance()
    {
        using var engine = new TelemetryEngine(NullLogger<TelemetryEngine>.Instance);

        engine.Should().NotBeNull();
    }

    // -----------------------------------------------------------------
    // StartActivity tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.StartActivity"/> returns an activity
    /// with the correct operation name when a listener is registered.
    /// </summary>
    [Fact]
    public void StartActivity_WithListener_ReturnsActivityWithCorrectName()
    {
        using var activity = _sut.StartActivity("TestOperation");

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be("TestOperation");
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.StartActivity"/> respects the specified <see cref="ActivityKind"/>.
    /// </summary>
    [Fact]
    public void StartActivity_WithKind_ReturnsActivityWithCorrectKind()
    {
        using var activity = _sut.StartActivity("ServerOperation", ActivityKind.Server);

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Server);
    }

    /// <summary>
    /// Verifies that activities default to <see cref="ActivityKind.Internal"/>.
    /// </summary>
    [Fact]
    public void StartActivity_DefaultKind_IsInternal()
    {
        using var activity = _sut.StartActivity("InternalOperation");

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Internal);
    }

    /// <summary>
    /// Verifies that the returned activity has a valid TraceId.
    /// </summary>
    [Fact]
    public void StartActivity_ReturnsActivity_HasValidTraceId()
    {
        using var activity = _sut.StartActivity("TracedOperation");

        activity.Should().NotBeNull();
        activity!.TraceId.Should().NotBe(default(ActivityTraceId));
    }

    // -----------------------------------------------------------------
    // RecordMetric tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordMetric"/> does not throw
    /// when recording a valid metric without tags.
    /// </summary>
    [Fact]
    public void RecordMetric_ValidNameAndValue_DoesNotThrow()
    {
        var act = () => _sut.RecordMetric("test.metric", 42.0);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordMetric"/> does not throw
    /// when recording a metric with tags.
    /// </summary>
    [Fact]
    public void RecordMetric_WithTags_DoesNotThrow()
    {
        var tags = new[]
        {
            new KeyValuePair<string, object?>("host", "server-01"),
            new KeyValuePair<string, object?>("region", "eu-west-1")
        };

        var act = () => _sut.RecordMetric("tagged.metric", 99.5, tags);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordMetric"/> does not throw with null tags.
    /// </summary>
    [Fact]
    public void RecordMetric_NullTags_DoesNotThrow()
    {
        var act = () => _sut.RecordMetric("no.tags.metric", 1.0, null);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that recording metrics with the same name multiple times works.
    /// </summary>
    [Fact]
    public void RecordMetric_SameNameMultipleTimes_DoesNotThrow()
    {
        var act = () =>
        {
            _sut.RecordMetric("repeated.metric", 1.0);
            _sut.RecordMetric("repeated.metric", 2.0);
            _sut.RecordMetric("repeated.metric", 3.0);
        };

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------
    // RecordException tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordException"/> adds an exception event
    /// to the activity.
    /// </summary>
    [Fact]
    public void RecordException_ValidActivityAndException_AddsExceptionEvent()
    {
        using var activity = _sut.StartActivity("FailingOperation");
        var exception = new InvalidOperationException("Something went wrong");

        _sut.RecordException(activity, exception);

        activity.Should().NotBeNull();
        activity!.Events.Should().ContainSingle(e => e.Name == "exception");
        var exceptionEvent = activity.Events.First(e => e.Name == "exception");
        exceptionEvent.Tags.Should().Contain(t =>
            t.Key == "exception.type" && (string)t.Value! == typeof(InvalidOperationException).FullName);
        exceptionEvent.Tags.Should().Contain(t =>
            t.Key == "exception.message" && (string)t.Value! == "Something went wrong");
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordException"/> sets the activity status to Error.
    /// </summary>
    [Fact]
    public void RecordException_ValidActivity_SetsStatusToError()
    {
        using var activity = _sut.StartActivity("ErrorOperation");
        var exception = new InvalidOperationException("Test error");

        _sut.RecordException(activity, exception);

        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("Test error");
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordException"/> is a no-op when activity is null.
    /// </summary>
    [Fact]
    public void RecordException_NullActivity_DoesNotThrow()
    {
        var exception = new InvalidOperationException("No activity");

        var act = () => _sut.RecordException(null, exception);

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------
    // SetActivityStatus tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.SetActivityStatus"/> sets the correct status.
    /// </summary>
    [Fact]
    public void SetActivityStatus_ValidActivity_SetsCorrectStatus()
    {
        using var activity = _sut.StartActivity("StatusOperation");

        _sut.SetActivityStatus(activity, ActivityStatusCode.Ok, "All good");

        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
        activity.StatusDescription.Should().Be("All good");
    }

    /// <summary>
    /// Verifies that setting status to Error works correctly.
    /// </summary>
    [Fact]
    public void SetActivityStatus_ErrorStatus_SetsErrorWithDescription()
    {
        using var activity = _sut.StartActivity("ErrorStatusOperation");

        _sut.SetActivityStatus(activity, ActivityStatusCode.Error, "Something failed");

        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("Something failed");
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.SetActivityStatus"/> is a no-op when activity is null.
    /// </summary>
    [Fact]
    public void SetActivityStatus_NullActivity_DoesNotThrow()
    {
        var act = () => _sut.SetActivityStatus(null, ActivityStatusCode.Ok);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that status can be set without a description.
    /// </summary>
    [Fact]
    public void SetActivityStatus_NullDescription_SetsStatusWithoutDescription()
    {
        using var activity = _sut.StartActivity("NoDescOperation");

        _sut.SetActivityStatus(activity, ActivityStatusCode.Ok);

        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    // -----------------------------------------------------------------
    // CreateCounter / CreateHistogram tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.CreateCounter{T}"/> returns a non-null counter.
    /// </summary>
    [Fact]
    public void CreateCounter_ValidName_ReturnsNonNullCounter()
    {
        var counter = _sut.CreateCounter<long>("test.counter", "items", "A test counter");

        counter.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.CreateHistogram{T}"/> returns a non-null histogram.
    /// </summary>
    [Fact]
    public void CreateHistogram_ValidName_ReturnsNonNullHistogram()
    {
        var histogram = _sut.CreateHistogram<double>("test.histogram", "ms", "A test histogram");

        histogram.Should().NotBeNull();
    }

    // -----------------------------------------------------------------
    // Well-known metric recording methods
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordRequestDuration"/> does not throw.
    /// </summary>
    [Fact]
    public void RecordRequestDuration_ValidArgs_DoesNotThrow()
    {
        var act = () => _sut.RecordRequestDuration(150.5, "/api/agents");

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordRequestCount"/> does not throw.
    /// </summary>
    [Fact]
    public void RecordRequestCount_ValidArgs_DoesNotThrow()
    {
        var act = () => _sut.RecordRequestCount("/api/agents", 200);

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordActiveAgents"/> does not throw for positive delta.
    /// </summary>
    [Fact]
    public void RecordActiveAgents_PositiveDelta_DoesNotThrow()
    {
        var act = () => _sut.RecordActiveAgents(1, "DebateAgent");

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordActiveAgents"/> does not throw for negative delta.
    /// </summary>
    [Fact]
    public void RecordActiveAgents_NegativeDelta_DoesNotThrow()
    {
        var act = () => _sut.RecordActiveAgents(-1, "DebateAgent");

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordReasoningLatency"/> does not throw.
    /// </summary>
    [Fact]
    public void RecordReasoningLatency_ValidArgs_DoesNotThrow()
    {
        var act = () => _sut.RecordReasoningLatency(250.0, "Debate");

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordWorkflowStepDuration"/> does not throw.
    /// </summary>
    [Fact]
    public void RecordWorkflowStepDuration_ValidArgs_DoesNotThrow()
    {
        var act = () => _sut.RecordWorkflowStepDuration(80.0, "DataIngestion", "ParseStep");

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that <see cref="TelemetryEngine.RecordError"/> does not throw.
    /// </summary>
    [Fact]
    public void RecordError_ValidArgs_DoesNotThrow()
    {
        var act = () => _sut.RecordError("TimeoutException", "HttpAdapter");

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------
    // Concurrency tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that multiple concurrent activities do not interfere with each other.
    /// </summary>
    [Fact]
    public void StartActivity_MultipleConcurrent_DoNotInterfere()
    {
        using var activity1 = _sut.StartActivity("Operation1");
        using var activity2 = _sut.StartActivity("Operation2");
        using var activity3 = _sut.StartActivity("Operation3");

        activity1.Should().NotBeNull();
        activity2.Should().NotBeNull();
        activity3.Should().NotBeNull();

        activity1!.OperationName.Should().Be("Operation1");
        activity2!.OperationName.Should().Be("Operation2");
        activity3!.OperationName.Should().Be("Operation3");

        // Each activity should have its own TraceId or SpanId
        activity1.SpanId.Should().NotBe(activity2.SpanId);
        activity2.SpanId.Should().NotBe(activity3.SpanId);
    }

    /// <summary>
    /// Verifies that concurrent activities from multiple threads do not cause exceptions.
    /// </summary>
    [Fact]
    public async Task StartActivity_ConcurrentThreads_DoNotThrow()
    {
        var tasks = Enumerable.Range(0, 50).Select(i =>
            Task.Run(() =>
            {
                using var activity = _sut.StartActivity($"ConcurrentOp_{i}");
                activity.Should().NotBeNull();
                activity!.OperationName.Should().Be($"ConcurrentOp_{i}");
                _sut.SetActivityStatus(activity, ActivityStatusCode.Ok);
            }));

        var act = () => Task.WhenAll(tasks);

        await act.Should().NotThrowAsync();
    }

    // -----------------------------------------------------------------
    // Dispose tests
    // -----------------------------------------------------------------

    /// <summary>
    /// Verifies that disposing the engine does not throw.
    /// </summary>
    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var engine = new TelemetryEngine(NullLogger<TelemetryEngine>.Instance);

        var act = () =>
        {
            engine.Dispose();
            engine.Dispose();
        };

        act.Should().NotThrow();
    }
}
