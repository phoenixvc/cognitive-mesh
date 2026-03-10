using CognitiveMesh.BusinessApplications.CustomerIntelligence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.CustomerIntelligence.Controllers;

/// <summary>
/// Controller for customer intelligence operations, providing endpoints for
/// customer profile retrieval, segment queries, insight generation,
/// and behavioral prediction.
/// </summary>
[ApiController]
[Route("api/v1/customer-intelligence")]
public class CustomerServiceController : ControllerBase
{
    private readonly ILogger<CustomerServiceController> _logger;
    private readonly ICustomerIntelligenceManager _intelligenceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerServiceController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for structured logging.</param>
    /// <param name="intelligenceManager">The customer intelligence manager.</param>
    public CustomerServiceController(
        ILogger<CustomerServiceController> logger,
        ICustomerIntelligenceManager intelligenceManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _intelligenceManager = intelligenceManager ?? throw new ArgumentNullException(nameof(intelligenceManager));
    }

    /// <summary>
    /// Retrieves a customer profile by identifier.
    /// </summary>
    /// <param name="customerId">The unique customer identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The customer profile.</returns>
    /// <response code="200">Returns the customer profile.</response>
    /// <response code="404">If the customer is not found.</response>
    [HttpGet("profiles/{customerId}")]
    [ProducesResponseType(typeof(CustomerProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerProfile>> GetProfileAsync(string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return BadRequest("Customer ID is required.");
        }

        try
        {
            _logger.LogInformation("Retrieving profile for customer {CustomerId}", customerId);

            var profile = await _intelligenceManager.GetCustomerProfileAsync(customerId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(profile);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Customer not found: {customerId}");
        }
    }

    /// <summary>
    /// Queries customer segments based on filter criteria.
    /// </summary>
    /// <param name="request">The segment query parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of matching customer segments.</returns>
    /// <response code="200">Returns the matching segments.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost("segments/query")]
    [ProducesResponseType(typeof(IEnumerable<CustomerSegment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CustomerSegment>>> QuerySegmentsAsync(
        [FromBody] CustomerSegmentQuery request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Querying customer segments with filter: {NameFilter}, limit: {Limit}",
            request.NameContains ?? "(none)", request.Limit);

        var segments = await _intelligenceManager.GetCustomerSegmentsAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(segments);
    }

    /// <summary>
    /// Generates insights for a specific customer.
    /// </summary>
    /// <param name="customerId">The unique customer identifier.</param>
    /// <param name="insightType">The type of insights to generate (defaults to All).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of generated customer insights.</returns>
    /// <response code="200">Returns the generated insights.</response>
    /// <response code="400">If the customer ID is invalid.</response>
    [HttpGet("insights/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerInsight>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CustomerInsight>>> GenerateInsightsAsync(
        string customerId,
        [FromQuery] InsightType insightType = InsightType.All,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return BadRequest("Customer ID is required.");
        }

        _logger.LogInformation("Generating insights for customer {CustomerId}, type: {InsightType}",
            customerId, insightType);

        var insights = await _intelligenceManager.GenerateCustomerInsightsAsync(customerId, insightType, cancellationToken)
            .ConfigureAwait(false);

        return Ok(insights);
    }

    /// <summary>
    /// Predicts customer behavior for a specific prediction type.
    /// </summary>
    /// <param name="customerId">The unique customer identifier.</param>
    /// <param name="predictionType">The type of behavior to predict.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The behavior prediction result.</returns>
    /// <response code="200">Returns the prediction.</response>
    /// <response code="400">If the customer ID is invalid.</response>
    [HttpGet("predictions/{customerId}")]
    [ProducesResponseType(typeof(CustomerPrediction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerPrediction>> PredictBehaviorAsync(
        string customerId,
        [FromQuery] PredictionType predictionType = PredictionType.Churn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return BadRequest("Customer ID is required.");
        }

        _logger.LogInformation("Predicting behavior for customer {CustomerId}, type: {PredictionType}",
            customerId, predictionType);

        var prediction = await _intelligenceManager.PredictCustomerBehaviorAsync(customerId, predictionType, cancellationToken)
            .ConfigureAwait(false);

        return Ok(prediction);
    }
}
