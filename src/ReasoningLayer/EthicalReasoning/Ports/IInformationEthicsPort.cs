namespace CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports;

/// <summary>
/// Represents a request to assess whether a proposed data action respects a subject's informational dignity.
/// This is a core concept in Floridi's Information Ethics, focusing on the well-being of an informational self.
/// </summary>
public class DignityAssessmentRequest
{
    /// <summary>
    /// The unique identifier of the subject whose information is being processed.
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>
    /// The type of data being processed (e.g., "Profile", "Behavioral", "Inferred").
    /// </summary>
    public string DataType { get; set; } = string.Empty;
    /// <summary>
    /// The action being performed on the data (e.g., "Store", "Analyze", "Share", "Delete").
    /// </summary>
    public string ProposedAction { get; set; } = string.Empty;
    /// <summary>
    /// The context or justification for the proposed action.
    /// </summary>
    public string ActionContext { get; set; } = string.Empty;
}

/// <summary>
/// Represents the response from an informational dignity assessment.
/// </summary>
public class DignityAssessmentResponse
{
    /// <summary>
    /// Indicates whether the proposed action is deemed to preserve the subject's informational dignity.
    /// </summary>
    public bool IsDignityPreserved { get; set; }
    /// <summary>
    /// A list of potential violations or ethical concerns if dignity is not preserved.
    /// Examples: "Risk of re-identification," "Processing exceeds original consent scope," "Action may lead to informational harm."
    /// </summary>
    public List<string> PotentialViolations { get; set; } = new();
}

/// <summary>
/// Represents a request to validate a data handling action against established stewardship principles.
/// </summary>
public class DataStewardshipValidationRequest
{
    /// <summary>
    /// The data handling action being performed (e.g., "Collect", "Process", "Share", "Delete").
    /// </summary>
    public string DataAction { get; set; } = string.Empty;
    /// <summary>
    /// The type of data involved (e.g., "PII", "Anonymized", "Aggregated").
    /// </summary>
    public string DataType { get; set; } = string.Empty;
    /// <summary>
    /// The unique identifier of the data subject, if applicable.
    /// </summary>
    public string DataSubjectId { get; set; } = string.Empty;
    /// <summary>
    /// A list of policy identifiers to validate the action against.
    /// </summary>
    public List<string> PolicyIds { get; set; } = new();
}

/// <summary>
/// Represents the response from a data stewardship validation check.
/// </summary>
public class DataStewardshipValidationResponse
{
    /// <summary>
    /// Indicates whether the proposed data action is compliant with stewardship policies.
    /// </summary>
    public bool IsCompliant { get; set; }
    /// <summary>
    /// A list of compliance issues or reasons for non-compliance.
    /// </summary>
    public List<string> ComplianceIssues { get; set; } = new();
}

/// <summary>
/// Represents a request to check a piece of information for its epistemic quality and reliability.
/// This is central to ensuring the mesh acts as a responsible epistemic agent.
/// </summary>
public class EpistemicResponsibilityCheckRequest
{
    /// <summary>
    /// The information or content to be evaluated.
    /// </summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>
    /// The source of the information (e.g., a URL, a document ID, an agent ID).
    /// </summary>
    public string Source { get; set; } = string.Empty;
    /// <summary>
    /// The type of content being checked (e.g., "FactualClaim", "Opinion", "Prediction").
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// Represents the response from an epistemic responsibility check.
/// </summary>
public class EpistemicResponsibilityCheckResponse
{
    /// <summary>
    /// A normalized score (0.0 to 1.0) representing the assessed reliability of the information.
    /// </summary>
    public double ReliabilityScore { get; set; }
    /// <summary>
    /// A list of identified epistemic risks.
    /// Examples: "Potential for misinformation," "Lacks sufficient supporting evidence," "Source is unverified or has low reputation."
    /// </summary>
    public List<string> IdentifiedRisks { get; set; } = new();
}

/// <summary>
/// Represents a single entry in the provenance chain of a piece of content.
/// </summary>
public class ProvenanceRecord
{
    /// <summary>Gets or sets the unique identifier of the content this record describes.</summary>
    public string ContentId { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC timestamp when the provenance action occurred.</summary>
    public DateTimeOffset Timestamp { get; set; }
    /// <summary>Gets or sets the action performed on the content (e.g., "Created", "DerivedFrom", "Modified").</summary>
    public string Action { get; set; } = string.Empty;
    /// <summary>Gets or sets the identifiers of source content from which this content was derived.</summary>
    public List<string> SourceContentIds { get; set; } = new();
    /// <summary>Gets or sets the identifier of the agent that performed the action.</summary>
    public string AgentId { get; set; } = string.Empty;
    /// <summary>Gets or sets the justification or description of why the action was taken.</summary>
    public string Justification { get; set; } = string.Empty;
}

/// <summary>
/// Represents a request to register a new piece of AI-generated content and its attribution.
/// </summary>
public class RegisterAttributionRequest
{
    /// <summary>Gets or sets the unique identifier assigned to the generated content.</summary>
    public string ContentId { get; set; } = string.Empty;
    /// <summary>Gets or sets the actual generated content being registered.</summary>
    public string GeneratedContent { get; set; } = string.Empty;
    /// <summary>Gets or sets a description of the process used to generate the content.</summary>
    public string GenerationProcessDescription { get; set; } = string.Empty;
    /// <summary>Gets or sets the identifiers of source content used during generation.</summary>
    public List<string> SourceContentIds { get; set; } = new();
    /// <summary>Gets or sets the identifier of the agent that generated the content.</summary>
    public string AgentId { get; set; } = string.Empty;
}

/// <summary>
/// Represents the response from a content registration operation.
/// </summary>
public class RegisterAttributionResponse
{
    /// <summary>Gets or sets a value indicating whether the attribution registration succeeded.</summary>
    public bool IsSuccess { get; set; }
    /// <summary>Gets or sets the identifier of the created provenance record.</summary>
    public string ProvenanceRecordId { get; set; } = string.Empty;
    /// <summary>Gets or sets the error message if the registration failed.</summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Represents a request to retrieve the provenance chain for a piece of content.
/// </summary>
public class GetProvenanceRequest
{
    /// <summary>Gets or sets the identifier of the content whose provenance chain is requested.</summary>
    public string ContentId { get; set; } = string.Empty;
}

/// <summary>
/// Represents the response containing the provenance history of a piece of content.
/// </summary>
public class GetProvenanceResponse
{
    /// <summary>Gets or sets a value indicating whether the provenance retrieval succeeded.</summary>
    public bool IsSuccess { get; set; }
    /// <summary>Gets or sets the ordered chain of provenance records tracing the content's history.</summary>
    public List<ProvenanceRecord> ProvenanceChain { get; set; } = new();
    /// <summary>Gets or sets the error message if the provenance retrieval failed.</summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Defines the contract for the Information Ethics Port. This port provides reasoning capabilities
/// based on the philosophical framework of Luciano Floridi, focusing on the ethical management
/// of information itself. It ensures that the Cognitive Mesh respects informational dignity,
/// acts as a responsible data steward, maintains epistemic integrity, and provides clear
/// attribution for all generated content.
/// </summary>
public interface IInformationEthicsPort
{
    /// <summary>
    /// Assesses whether a proposed action on a subject's data respects their informational dignity.
    /// This involves checking for potential harms like misrepresentation, denial of access, or loss of control.
    /// </summary>
    /// <param name="request">The request detailing the proposed action and its context.</param>
    /// <returns>A response indicating if dignity is preserved and any potential violations.</returns>
    Task<DignityAssessmentResponse> AssessInformationalDignityAsync(DignityAssessmentRequest request);

    /// <summary>
    /// Validates a proposed data handling action against the principles of responsible data stewardship.
    /// This includes ensuring the action is necessary, proportionate, and aligned with its stated purpose.
    /// </summary>
    /// <param name="request">The request describing the data action to be validated.</param>
    /// <returns>A response indicating compliance with stewardship principles.</returns>
    Task<DataStewardshipValidationResponse> ValidateDataStewardshipAsync(DataStewardshipValidationRequest request);

    /// <summary>
    /// Checks a piece of information for its epistemic quality, ensuring the mesh acts as a
    /// responsible agent in the information ecosystem by not polluting it with unreliable or false content.
    /// </summary>
    /// <param name="request">The request containing the information to be checked.</param>
    /// <returns>A response with a reliability score and a list of identified epistemic risks.</returns>
    Task<EpistemicResponsibilityCheckResponse> CheckEpistemicResponsibilityAsync(EpistemicResponsibilityCheckRequest request);

    /// <summary>
    /// Registers a new piece of AI-generated content, creating an immutable record of its creation,
    /// sources, and the agent responsible. This is fundamental for accountability and transparency.
    /// </summary>
    /// <param name="request">The request containing the content and its attribution details.</param>
    /// <returns>A confirmation of the registration.</returns>
    Task<RegisterAttributionResponse> RegisterAttributionAsync(RegisterAttributionRequest request);

    /// <summary>
    /// Retrieves the complete, traceable history (provenance) of a piece of content, showing how it was
    /// created and what sources it was derived from.
    /// </summary>
    /// <param name="request">The request identifying the content to be traced.</param>
    /// <returns>A response containing the full provenance chain.</returns>
    Task<GetProvenanceResponse> GetProvenanceAsync(GetProvenanceRequest request);
}