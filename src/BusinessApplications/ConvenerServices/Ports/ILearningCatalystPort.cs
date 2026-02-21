namespace CognitiveMesh.BusinessApplications.ConvenerServices.Ports;

/// <summary>
/// Port interface for the Learning Catalyst feature.
/// Curates, tags, and pushes learning recommendations;
/// links contributions to learning outcomes.
/// </summary>
public interface ILearningCatalystPort
{
    /// <summary>
    /// Generates personalized learning catalyst recommendations for a user.
    /// </summary>
    /// <param name="request">The recommendation request with user and tenant context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A set of curated learning recommendations.</returns>
    Task<LearningCatalystResponse> GetRecommendationsAsync(
        LearningCatalystRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request DTO for learning catalyst recommendations.
/// </summary>
public class LearningCatalystRequest
{
    /// <summary>Tenant scope for data isolation.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>User requesting learning recommendations.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Optional skill areas to focus recommendations on.</summary>
    public List<string> FocusAreas { get; set; } = new();

    /// <summary>Maximum number of recommendations to return.</summary>
    public int MaxRecommendations { get; set; } = 5;
}

/// <summary>
/// Response DTO containing curated learning catalyst recommendations.
/// </summary>
public class LearningCatalystResponse
{
    /// <summary>The user these recommendations are for.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Curated learning recommendations, ordered by relevance.</summary>
    public List<LearningRecommendation> Recommendations { get; set; } = new();

    /// <summary>Skill gaps identified from the user's profile.</summary>
    public List<SkillGap> IdentifiedGaps { get; set; } = new();
}

/// <summary>
/// A single learning recommendation from the catalyst engine.
/// </summary>
public class LearningRecommendation
{
    /// <summary>Title of the recommended learning activity.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Description of what the learner will gain.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Type of learning activity (Article, Course, Mentorship, Project, PeerSession).</summary>
    public LearningActivityType ActivityType { get; set; }

    /// <summary>Relevance score (0.0–1.0).</summary>
    public double RelevanceScore { get; set; }

    /// <summary>Skill area this recommendation targets.</summary>
    public string TargetSkill { get; set; } = string.Empty;

    /// <summary>Estimated time commitment in minutes.</summary>
    public int EstimatedMinutes { get; set; }

    /// <summary>Champion who contributed this knowledge, if applicable.</summary>
    public string? ContributorUserId { get; set; }
}

/// <summary>
/// Represents an identified skill gap for a user.
/// </summary>
public class SkillGap
{
    /// <summary>Name of the skill with a gap.</summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>Current proficiency level (0.0–1.0).</summary>
    public double CurrentLevel { get; set; }

    /// <summary>Target proficiency level (0.0–1.0).</summary>
    public double TargetLevel { get; set; }

    /// <summary>Priority of closing this gap (Critical, High, Medium, Low).</summary>
    public string Priority { get; set; } = "Medium";
}

/// <summary>
/// Types of learning activities that can be recommended.
/// </summary>
public enum LearningActivityType
{
    /// <summary>Written article or blog post.</summary>
    Article,
    /// <summary>Structured course or training module.</summary>
    Course,
    /// <summary>Mentorship session with a champion.</summary>
    Mentorship,
    /// <summary>Hands-on project or exercise.</summary>
    Project,
    /// <summary>Peer learning session or workshop.</summary>
    PeerSession
}
