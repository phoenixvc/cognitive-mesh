using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Azure.Messaging.EventGrid;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

[ApiController]
[Route("api/[controller]")]
public class DecisionSupportController : ControllerBase
{
    private readonly DecisionSupportManager _decisionSupportManager;
    private readonly DataVisualizationTool _dataVisualizationTool;
    private readonly ILogger<DecisionSupportController> _logger;
    private readonly IHubContext<FeedbackHub> _hubContext;
    private readonly EventGridPublisherClient _eventGridClient;

    public DecisionSupportController(
        DecisionSupportManager decisionSupportManager,
        DataVisualizationTool dataVisualizationTool,
        ILogger<DecisionSupportController> logger,
        IHubContext<FeedbackHub> hubContext,
        EventGridPublisherClient eventGridClient)
    {
        _decisionSupportManager = decisionSupportManager;
        _dataVisualizationTool = dataVisualizationTool;
        _logger = logger;
        _hubContext = hubContext;
        _eventGridClient = eventGridClient;
    }

    [HttpPost("feedback")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> CollectFeedback([FromBody] UserFeedback feedback)
    {
        try
        {
            await _decisionSupportManager.StoreFeedbackAsync(feedback.ScenarioId, feedback);
            await _hubContext.Clients.All.SendAsync("ReceiveFeedback", feedback);
            var eventGridEvent = new EventGridEvent(
                subject: "NewFeedback",
                eventType: "FeedbackReceived",
                dataVersion: "1.0",
                data: feedback);
            await _eventGridClient.SendEventAsync(eventGridEvent);
            return Ok("Feedback collected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting feedback");
            return StatusCode(500, "An error occurred while collecting feedback");
        }
    }

    [HttpGet("feedback/analysis")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> AnalyzeFeedback()
    {
        try
        {
            await _decisionSupportManager.AnalyzeFeedbackAsync();
            return Ok("Feedback analysis completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing feedback");
            return StatusCode(500, "An error occurred while analyzing feedback");
        }
    }

    [HttpPost("visualize")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> GenerateVisualization([FromBody] VisualizationRequest request)
    {
        try
        {
            var visualization = await _dataVisualizationTool.GenerateVisualizationAsync(request.Data);
            return Ok(visualization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating visualization");
            return StatusCode(500, "An error occurred while generating visualization");
        }
    }

    [HttpPost("visualize/report")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> GenerateVisualizationReport([FromBody] VisualizationReportRequest request)
    {
        try
        {
            var report = await _dataVisualizationTool.GenerateVisualizationReportAsync(request.Data, request.Format);
            return File(report, "application/octet-stream", $"report.{request.Format}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating visualization report");
            return StatusCode(500, "An error occurred while generating visualization report");
        }
    }

    [HttpPost("integrate")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> IntegrateExternalData([FromBody] ExternalDataRequest request)
    {
        try
        {
            await _decisionSupportManager.IntegrateExternalDataAsync(request.DataSource);
            return Ok("External data integrated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error integrating external data");
            return StatusCode(500, "An error occurred while integrating external data");
        }
    }

    [HttpGet("recommendations")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> GetRecommendations([FromQuery] string scenario, [FromQuery] Dictionary<string, string> context)
    {
        try
        {
            var recommendations = await _decisionSupportManager.GenerateRecommendationsAsync(scenario, context);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations");
            return StatusCode(500, "An error occurred while generating recommendations");
        }
    }

    [HttpGet("feedback/visualize")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> VisualizeFeedback()
    {
        try
        {
            var feedbackData = await _decisionSupportManager.GetFeedbackDataAsync();
            var visualization = await _dataVisualizationTool.GenerateVisualizationAsync(feedbackData);
            return Ok(visualization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error visualizing feedback");
            return StatusCode(500, "An error occurred while visualizing feedback");
        }
    }
}

public class VisualizationRequest
{
    public string Data { get; set; }
}

public class VisualizationReportRequest
{
    public string Data { get; set; }
    public string Format { get; set; }
}

public class ExternalDataRequest
{
    public string DataSource { get; set; }
}

public class UserFeedback
{
    public string ScenarioId { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; }
}

public class FeedbackHub : Hub
{
}
