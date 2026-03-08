namespace AgencyLayer.Tools.Ports;

/// <summary>
/// Content modality.
/// </summary>
public enum ContentModality
{
    /// <summary>Text content.</summary>
    Text,
    /// <summary>Image content.</summary>
    Image,
    /// <summary>Audio content.</summary>
    Audio,
    /// <summary>Video content.</summary>
    Video,
    /// <summary>Document (PDF, etc.).</summary>
    Document
}

/// <summary>
/// Multimodal content.
/// </summary>
public class MultimodalContent
{
    /// <summary>Content identifier.</summary>
    public string ContentId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Content modality.</summary>
    public ContentModality Modality { get; init; }

    /// <summary>Content data (base64 for binary).</summary>
    public required string Data { get; init; }

    /// <summary>MIME type.</summary>
    public required string MimeType { get; init; }

    /// <summary>File name (if applicable).</summary>
    public string? FileName { get; init; }

    /// <summary>Size in bytes.</summary>
    public long Size { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Image analysis result.
/// </summary>
public class ImageAnalysisResult
{
    /// <summary>Description of the image.</summary>
    public required string Description { get; init; }

    /// <summary>Detected objects.</summary>
    public IReadOnlyList<DetectedObject> Objects { get; init; } = Array.Empty<DetectedObject>();

    /// <summary>Detected text (OCR).</summary>
    public IReadOnlyList<DetectedText> TextRegions { get; init; } = Array.Empty<DetectedText>();

    /// <summary>Image classification.</summary>
    public IReadOnlyList<Classification> Classifications { get; init; } = Array.Empty<Classification>();

    /// <summary>Image dimensions.</summary>
    public (int Width, int Height) Dimensions { get; init; }

    /// <summary>Confidence score.</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// A detected object in an image.
/// </summary>
public class DetectedObject
{
    /// <summary>Object label.</summary>
    public required string Label { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>Bounding box (x, y, width, height).</summary>
    public (int X, int Y, int Width, int Height)? BoundingBox { get; init; }
}

/// <summary>
/// Detected text region.
/// </summary>
public class DetectedText
{
    /// <summary>Text content.</summary>
    public required string Text { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>Bounding box.</summary>
    public (int X, int Y, int Width, int Height)? BoundingBox { get; init; }
}

/// <summary>
/// Classification result.
/// </summary>
public class Classification
{
    /// <summary>Category.</summary>
    public required string Category { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// Multimodal processing configuration.
/// </summary>
public class MultimodalConfiguration
{
    /// <summary>Supported modalities.</summary>
    public IReadOnlyList<ContentModality> SupportedModalities { get; init; } = new[]
    {
        ContentModality.Text,
        ContentModality.Image
    };

    /// <summary>Maximum image size.</summary>
    public long MaxImageSize { get; init; } = 20 * 1024 * 1024;

    /// <summary>Whether to enable OCR.</summary>
    public bool EnableOCR { get; init; } = true;

    /// <summary>Whether to enable object detection.</summary>
    public bool EnableObjectDetection { get; init; } = true;

    /// <summary>Model for image analysis.</summary>
    public string? ImageAnalysisModel { get; init; }
}

/// <summary>
/// Port for visual AI multimodal integration.
/// Implements the "Visual AI Multimodal Integration" pattern.
/// </summary>
public interface IMultimodalPort
{
    /// <summary>
    /// Analyzes an image.
    /// </summary>
    Task<ImageAnalysisResult> AnalyzeImageAsync(
        MultimodalContent image,
        string? prompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Answers a question about an image.
    /// </summary>
    Task<string> AnswerImageQuestionAsync(
        MultimodalContent image,
        string question,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts text from an image (OCR).
    /// </summary>
    Task<IReadOnlyList<DetectedText>> ExtractTextAsync(
        MultimodalContent image,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes multiple modalities together.
    /// </summary>
    Task<string> ProcessMultimodalAsync(
        IEnumerable<MultimodalContent> contents,
        string prompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares multiple images.
    /// </summary>
    Task<string> CompareImagesAsync(
        IEnumerable<MultimodalContent> images,
        string? comparisonPrompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts data from a document.
    /// </summary>
    Task<Dictionary<string, string>> ExtractDocumentDataAsync(
        MultimodalContent document,
        IEnumerable<string> fieldsToExtract,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets supported modalities.
    /// </summary>
    Task<IReadOnlyList<ContentModality>> GetSupportedModalitiesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates content for processing.
    /// </summary>
    Task<(bool Valid, string? Error)> ValidateContentAsync(
        MultimodalContent content,
        CancellationToken cancellationToken = default);
}
