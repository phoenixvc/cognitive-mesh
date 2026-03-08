namespace FoundationLayer.Security.Ports;

/// <summary>
/// Types of personally identifiable information (PII) that can be detected and tokenized.
/// </summary>
public enum PIIType
{
    /// <summary>Full name of a person.</summary>
    PersonName,
    /// <summary>Email address.</summary>
    Email,
    /// <summary>Phone number.</summary>
    PhoneNumber,
    /// <summary>Social Security Number or national ID.</summary>
    NationalId,
    /// <summary>Credit card number.</summary>
    CreditCard,
    /// <summary>Bank account number.</summary>
    BankAccount,
    /// <summary>Physical address.</summary>
    Address,
    /// <summary>Date of birth.</summary>
    DateOfBirth,
    /// <summary>IP address.</summary>
    IpAddress,
    /// <summary>Medical record number or health information.</summary>
    HealthInfo,
    /// <summary>Passport number.</summary>
    PassportNumber,
    /// <summary>Driver's license number.</summary>
    DriversLicense,
    /// <summary>Biometric data reference.</summary>
    BiometricData,
    /// <summary>Custom/unknown PII type.</summary>
    Custom
}

/// <summary>
/// Represents a detected PII entity in text.
/// </summary>
public class PIIEntity
{
    /// <summary>The type of PII detected.</summary>
    public required PIIType Type { get; init; }

    /// <summary>The original value (only available before tokenization).</summary>
    public string? OriginalValue { get; init; }

    /// <summary>Start position in the text.</summary>
    public int StartIndex { get; init; }

    /// <summary>End position in the text.</summary>
    public int EndIndex { get; init; }

    /// <summary>Confidence score of the detection (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>The token that replaces this entity.</summary>
    public string? Token { get; init; }
}

/// <summary>
/// Request to tokenize PII in text content.
/// </summary>
public class TokenizationRequest
{
    /// <summary>Unique identifier for this tokenization operation.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The text content to process.</summary>
    public required string Content { get; init; }

    /// <summary>The context or data subject identifier for token mapping.</summary>
    public required string DataSubjectId { get; init; }

    /// <summary>Specific PII types to detect and tokenize (null = all types).</summary>
    public IReadOnlyList<PIIType>? TargetTypes { get; init; }

    /// <summary>Minimum confidence threshold for detection (0.0 - 1.0).</summary>
    public double ConfidenceThreshold { get; init; } = 0.8;

    /// <summary>Whether to preserve format (e.g., email structure, phone format).</summary>
    public bool PreserveFormat { get; init; } = true;

    /// <summary>Whether tokens should be reversible (detokenizable).</summary>
    public bool Reversible { get; init; } = true;
}

/// <summary>
/// Result of a tokenization operation.
/// </summary>
public class TokenizationResult
{
    /// <summary>The request ID this result corresponds to.</summary>
    public required string RequestId { get; init; }

    /// <summary>The tokenized content with PII replaced.</summary>
    public required string TokenizedContent { get; init; }

    /// <summary>List of detected and tokenized entities.</summary>
    public IReadOnlyList<PIIEntity> Entities { get; init; } = Array.Empty<PIIEntity>();

    /// <summary>Count of entities by type.</summary>
    public Dictionary<PIIType, int> EntityCounts { get; init; } = new();

    /// <summary>Whether the tokenization is reversible.</summary>
    public bool IsReversible { get; init; }
}

/// <summary>
/// Request to detokenize content back to original values.
/// </summary>
public class DetokenizationRequest
{
    /// <summary>The tokenized content to restore.</summary>
    public required string TokenizedContent { get; init; }

    /// <summary>The data subject identifier used during tokenization.</summary>
    public required string DataSubjectId { get; init; }

    /// <summary>Specific tokens to detokenize (null = all tokens).</summary>
    public IReadOnlyList<string>? TargetTokens { get; init; }

    /// <summary>Reason for detokenization (for audit trail).</summary>
    public required string Reason { get; init; }

    /// <summary>Who is requesting detokenization.</summary>
    public required string RequestedBy { get; init; }
}

/// <summary>
/// Result of a detokenization operation.
/// </summary>
public class DetokenizationResult
{
    /// <summary>The restored content with original values.</summary>
    public required string RestoredContent { get; init; }

    /// <summary>Number of tokens that were restored.</summary>
    public int TokensRestored { get; init; }

    /// <summary>Tokens that could not be restored (expired, not found).</summary>
    public IReadOnlyList<string> UnrestoredTokens { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for PII tokenization and de-identification.
/// Implements the "PII Tokenization" pattern for GDPR compliance.
/// </summary>
/// <remarks>
/// This port provides capabilities to detect, tokenize, and optionally restore
/// personally identifiable information in text content. Tokens are deterministic
/// for a given data subject, enabling consistent anonymization across systems.
/// </remarks>
public interface IPIITokenizationPort
{
    /// <summary>
    /// Detects PII entities in text without tokenizing.
    /// </summary>
    /// <param name="content">The text to analyze.</param>
    /// <param name="targetTypes">Specific PII types to detect (null = all).</param>
    /// <param name="confidenceThreshold">Minimum confidence for detection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of detected PII entities.</returns>
    Task<IReadOnlyList<PIIEntity>> DetectPIIAsync(
        string content,
        IReadOnlyList<PIIType>? targetTypes = null,
        double confidenceThreshold = 0.8,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tokenizes PII in content, replacing sensitive values with tokens.
    /// </summary>
    /// <param name="request">The tokenization request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tokenization result.</returns>
    Task<TokenizationResult> TokenizeAsync(
        TokenizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores tokenized content to original values.
    /// </summary>
    /// <param name="request">The detokenization request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The detokenization result.</returns>
    Task<DetokenizationResult> DetokenizeAsync(
        DetokenizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes all tokens for a data subject (right to be forgotten).
    /// </summary>
    /// <param name="dataSubjectId">The data subject identifier.</param>
    /// <param name="reason">The reason for deletion.</param>
    /// <param name="requestedBy">Who requested the deletion.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of tokens deleted.</returns>
    Task<int> DeleteTokensForSubjectAsync(
        string dataSubjectId,
        string reason,
        string requestedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about tokenized data for a subject.
    /// </summary>
    /// <param name="dataSubjectId">The data subject identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token statistics.</returns>
    Task<Dictionary<PIIType, int>> GetTokenStatisticsAsync(
        string dataSubjectId,
        CancellationToken cancellationToken = default);
}
