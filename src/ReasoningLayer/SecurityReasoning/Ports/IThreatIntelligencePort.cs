namespace CognitiveMesh.ReasoningLayer.SecurityReasoning.Ports;

/// <summary>
/// Represents a single security event or log entry to be analyzed.
/// </summary>
public class SecurityEvent
{
    /// <summary>Gets or sets the unique event identifier.</summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    /// <summary>Gets or sets the timestamp of the event.</summary>
    public DateTimeOffset Timestamp { get; set; }
    /// <summary>Gets or sets the source of the event (e.g., "Firewall", "ApplicationLog").</summary>
    public string Source { get; set; } = string.Empty;
    /// <summary>Gets or sets the type of the event (e.g., "LoginAttempt", "PolicyViolation").</summary>
    public string EventType { get; set; } = string.Empty;
    /// <summary>Gets or sets the event data payload.</summary>
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Represents a request to analyze a collection of security events for potential threats.
/// </summary>
public class ThreatAnalysisRequest
{
    /// <summary>
    /// A collection of correlated security events to be analyzed.
    /// </summary>
    public IEnumerable<SecurityEvent> Events { get; set; } = [];
    /// <summary>
    /// Additional context for the analysis, such as the user session or transaction ID.
    /// </summary>
    public Dictionary<string, string> Context { get; set; } = new();
}

/// <summary>
/// Represents the outcome of a threat analysis operation.
/// </summary>
public class ThreatAnalysisResponse
{
    /// <summary>Gets or sets whether a threat was detected.</summary>
    public bool IsThreatDetected { get; set; }
    /// <summary>
    /// A description of the detected threat, if any.
    /// </summary>
    public string ThreatDescription { get; set; } = string.Empty;
    /// <summary>
    /// The severity level of the detected threat (e.g., "Low", "Medium", "High", "Critical").
    /// </summary>
    public string Severity { get; set; } = string.Empty;
    /// <summary>
    /// A list of recommended actions to mitigate the detected threat.
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();
}

/// <summary>
/// Represents a request to check a set of artifacts against a database of known Indicators of Compromise (IOCs).
/// </summary>
public class IOCDetectionRequest
{
    /// <summary>
    /// A dictionary of artifacts to check, where the key is the artifact type (e.g., "ip_address", "file_hash", "domain")
    /// and the value is a list of the artifacts themselves.
    /// </summary>
    public Dictionary<string, List<string>> Artifacts { get; set; } = new();
}

/// <summary>
/// Represents the result of an IOC detection operation.
/// </summary>
public class IOCDetectionResponse
{
    /// <summary>
    /// A list of artifacts that were identified as known Indicators of Compromise.
    /// </summary>
    public List<DetectedIOC> DetectedIOCs { get; set; } = new();
}

/// <summary>
/// Describes a single detected Indicator of Compromise.
/// </summary>
public class DetectedIOC
{
    /// <summary>Gets or sets the type of the artifact (e.g., "ip_address", "file_hash").</summary>
    public string ArtifactType { get; set; } = string.Empty;
    /// <summary>Gets or sets the value of the detected artifact.</summary>
    public string ArtifactValue { get; set; } = string.Empty;
    /// <summary>
    /// Information about the threat associated with this IOC.
    /// </summary>
    public string ThreatInfo { get; set; } = string.Empty;
}

/// <summary>
/// Represents a request to calculate a risk score for a specific event or entity.
/// </summary>
public class RiskScoringRequest
{
    /// <summary>
    /// The unique identifier of the subject (user, service, agent) being scored.
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>
    /// The action being performed by the subject.
    /// </summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>
    /// The resource being accessed.
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;
    /// <summary>
    /// Additional context used for scoring, such as time, location, and recent behavior.
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Represents the calculated risk score and its contributing factors.
/// </summary>
public class RiskScoringResponse
{
    /// <summary>
    /// A numerical risk score, typically normalized (e.g., 0 to 100).
    /// </summary>
    public int RiskScore { get; set; }
    /// <summary>
    /// A qualitative risk level (e.g., "Low", "Medium", "High").
    /// </summary>
    public string RiskLevel { get; set; } = string.Empty;
    /// <summary>
    /// A list of factors that contributed to the calculated risk score.
    /// </summary>
    public List<string> ContributingFactors { get; set; } = new();
}

/// <summary>
/// Defines the contract for the Threat Intelligence Port, a key component of the ReasoningLayer
/// within the Zero-Trust Security Framework. This port provides advanced reasoning capabilities
/// for analyzing security data, detecting threats, and assessing risk in real-time.
/// </summary>
public interface IThreatIntelligencePort
{
    /// <summary>
    /// Analyzes a stream of security events to identify complex threat patterns and anomalies
    /// that may not be apparent from individual events.
    /// </summary>
    /// <param name="request">The request containing the security events and context for analysis.</param>
    /// <returns>A response summarizing any detected threats and recommended mitigation actions.</returns>
    Task<ThreatAnalysisResponse> AnalyzeThreatPatternsAsync(ThreatAnalysisRequest request);

    /// <summary>
    /// Scans a set of artifacts (e.g., IP addresses, file hashes) against a threat intelligence
    /// feed to detect known Indicators of Compromise (IOCs).
    /// </summary>
    /// <param name="request">The request containing the artifacts to be checked.</param>
    /// <returns>A response listing any artifacts that match known IOCs.</returns>
    Task<IOCDetectionResponse> DetectIndicatorsOfCompromiseAsync(IOCDetectionRequest request);

    /// <summary>
    /// Calculates a dynamic risk score for a given action or entity based on a variety of contextual
    /// factors, such as user behavior, location, time of day, and the sensitivity of the resource.
    /// This score is a critical input for dynamic access control decisions in the Zero-Trust model.
    /// </summary>
    /// <param name="request">The request containing the context for risk scoring.</param>
    /// <returns>A response containing the calculated risk score and the factors that influenced it.</returns>
    Task<RiskScoringResponse> CalculateRiskScoreAsync(RiskScoringRequest request);
}