using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CustomerServiceController : ControllerBase
{
    private readonly CognitiveMeshCoordinator _coordinator;
    private readonly ILogger<CustomerServiceController> _logger;
    private readonly CustomerDataService _customerService;
    private readonly ProductDataService _productService;
    private readonly FeatureFlagManager _featureFlagManager;
    
    public CustomerServiceController(
        CognitiveMeshCoordinator coordinator,
        CustomerDataService customerService,
        ProductDataService productService,
        ILogger<CustomerServiceController> logger,
        FeatureFlagManager featureFlagManager)
    {
        _coordinator = coordinator;
        _customerService = customerService;
        _productService = productService;
        _logger = logger;
        _featureFlagManager = featureFlagManager;
    }
    
    [HttpPost("inquiry")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> HandleCustomerInquiry([FromBody] CustomerInquiryRequest request)
    {
        try
        {
            if (!_featureFlagManager.EnableADK)
            {
                return BadRequest("Feature not enabled.");
            }

            // Get customer context if available
            CustomerContext customerContext = null;
            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                customerContext = await _customerService.GetCustomerContextAsync(request.CustomerId);
            }
            
            // Categorize inquiry
            var categorizationQuery = $"Categorize this customer inquiry: {request.Inquiry}";
            var categorizationResponse = await _coordinator.ProcessQueryAsync(categorizationQuery);
            
            // Extract inquiry category
            var category = ExtractCategory(categorizationResponse.Response);
            
            // Get relevant product information if applicable
            List<ProductInfo> productInfo = new List<ProductInfo>();
            if (!string.IsNullOrEmpty(request.ProductId))
            {
                productInfo.Add(await _productService.GetProductInfoAsync(request.ProductId));
            }
            else if (category.Contains("product"))
            {
                // Try to identify product from inquiry
                var productQuery = $"Identify the product mentioned in this inquiry: {request.Inquiry}";
                var productResponse = await _coordinator.ProcessQueryAsync(productQuery);
                var productId = ExtractProductId(productResponse.Response);
                
                if (!string.IsNullOrEmpty(productId))
                {
                    productInfo.Add(await _productService.GetProductInfoAsync(productId));
                }
            }
            
            // Generate response options
            var responseQuery = $"Generate a helpful response to this customer inquiry: {request.Inquiry}";
            
            // Add context to query if available
            if (customerContext != null)
            {
                responseQuery += $"\nCustomer context: {JsonSerializer.Serialize(customerContext)}";
            }
            
            if (productInfo.Any())
            {
                responseQuery += $"\nProduct information: {JsonSerializer.Serialize(productInfo)}";
            }
            
            var options = new QueryOptions
            {
                Perspectives = new List<string> { "empathetic", "practical", "analytical" }
            };
            
            var responseOptions = await _coordinator.ProcessQueryAsync(responseQuery, options);
            
            // Check if this requires escalation
            var escalationQuery = $"Determine if this customer inquiry requires escalation: {request.Inquiry}";
            var escalationResponse = await _coordinator.ProcessQueryAsync(escalationQuery);
            var requiresEscalation = DetermineIfEscalationRequired(escalationResponse.Response);
            
            // Generate next steps
            var nextStepsQuery = $"What are the recommended next steps for this customer inquiry: {request.Inquiry}";
            var nextStepsResponse = await _coordinator.ProcessQueryAsync(nextStepsQuery);
            
            // Compile result
            var result = new CustomerInquiryResponse
            {
                InquiryId = Guid.NewGuid().ToString(),
                Category = category,
                ResponseOptions = responseOptions.Response,
                RequiresEscalation = requiresEscalation,
                NextSteps = nextStepsResponse.Response,
                RelevantKnowledge = responseOptions.KnowledgeResults
                    .Select(k => new KnowledgeReference
                    {
                        Title = k.Title,
                        Source = k.Source,
                        Snippet = TruncateContent(k.Content, 200)
                    })
                    .ToList()
            };
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing customer inquiry");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    
    [HttpPost("troubleshoot")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> TroubleshootIssue([FromBody] TroubleshootingRequest request)
    {
        try
        {
            if (!_featureFlagManager.EnableLangGraph)
            {
                return BadRequest("Feature not enabled.");
            }

            // Get product information
            var productInfo = await _productService.GetProductInfoAsync(request.ProductId);
            
            // Create troubleshooting context
            var troubleshootingContext = new
            {
                Product = productInfo,
                IssueDescription = request.IssueDescription,
                PreviousStepsTaken = request.PreviousStepsTaken
            };
            
            // Diagnose issue
            var diagnosticQuery = $"Diagnose this issue: {request.IssueDescription} " +
                                $"for product: {productInfo.Name}. " +
                                $"Previous steps taken: {request.PreviousStepsTaken}";
            
            var options = new QueryOptions
            {
                EnableAgentExecution = true,
                Perspectives = new List<string> { "analytical", "practical", "critical" }
            };
            
            var diagnosticResponse = await _coordinator.ProcessQueryAsync(diagnosticQuery, options);
            
            // Generate solution steps
            var solutionQuery = $"Provide step-by-step solution for: {request.IssueDescription} " +
                               $"with product: {productInfo.Name}. " +
                               $"Based on diagnosis: {diagnosticResponse.Response}";
            
            var solutionResponse = await _coordinator.ProcessQueryAsync(solutionQuery);
            
            // Perform causal analysis
            var causalQuery = $"What are the likely root causes of this issue: {request.IssueDescription} " +
                             $"with product: {productInfo.Name}?";
            
            var causalResponse = await _coordinator.ProcessQueryAsync(causalQuery);
            
            // Compile result
            var result = new TroubleshootingResponse
            {
                ProductId = request.ProductId,
                ProductName = productInfo.Name,
                Diagnosis = diagnosticResponse.Response,
                SolutionSteps = solutionResponse.Response,
                RootCauseAnalysis = causalResponse.Response,
                RelatedKnowledgeBase = diagnosticResponse.KnowledgeResults
                    .Select(k => new KnowledgeReference
                    {
                        Title = k.Title,
                        Source = k.Source,
                        Snippet = TruncateContent(k.Content, 200)
                    })
                    .ToList()
            };
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing troubleshooting request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    
    [HttpPost("conversation")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> HandleConversation([FromBody] ConversationRequest request)
    {
        try
        {
            if (!_featureFlagManager.EnableCrewAI)
            {
                return BadRequest("Feature not enabled.");
            }

            // Get customer context if available
            CustomerContext customerContext = null;
            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                customerContext = await _customerService.GetCustomerContextAsync(request.CustomerId);
            }
            
            // Format conversation history
            var conversationHistory = new StringBuilder();
            foreach (var message in request.ConversationHistory)
            {
                conversationHistory.AppendLine($"{message.Role}: {message.Content}");
            }
            
            // Generate response
            var responseQuery = $"Generate a helpful response to the latest message in this conversation:\n\n" +
                               $"Conversation history:\n{conversationHistory}\n\n" +
                               $"Latest message: {request.CurrentMessage}";
            
            // Add context to query if available
            if (customerContext != null)
            {
                responseQuery += $"\nCustomer context: {JsonSerializer.Serialize(customerContext)}";
            }
            
            var options = new QueryOptions
            {
                Perspectives = new List<string> { "empathetic", "practical" }
            };
            
            var responseResult = await _coordinator.ProcessQueryAsync(responseQuery, options);
            
            // Analyze customer sentiment
            var sentimentQuery = $"Analyze the customer sentiment in this conversation:\n{conversationHistory}\n\n" +
                                $"Latest message: {request.CurrentMessage}";
            
            var sentimentResponse = await _coordinator.ProcessQueryAsync(sentimentQuery);
            
            // Compile result
            var result = new ConversationResponse
            {
                Response = responseResult.Response,
                SentimentAnalysis = sentimentResponse.Response,
                SuggestedActions = ExtractSuggestedActions(responseResult.Response)
            };
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing conversation request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
    
    private string ExtractCategory(string categorizationText)
    {
        // In a real implementation, this would use more sophisticated parsing
        // For now, use simple keyword matching
        
        if (categorizationText.Contains("billing", StringComparison.OrdinalIgnoreCase))
            return "Billing";
            
        if (categorizationText.Contains("technical", StringComparison.OrdinalIgnoreCase))
            return "Technical Support";
            
        if (categorizationText.Contains("account", StringComparison.OrdinalIgnoreCase))
            return "Account Management";
            
        if (categorizationText.Contains("product", StringComparison.OrdinalIgnoreCase))
            return "Product Information";
            
        return "General Inquiry";
    }
    
    private string ExtractProductId(string productText)
    {
        // In a real implementation, this would use more sophisticated parsing
        // For now, return a dummy product ID if a product is detected
        
        if (productText.Contains("product", StringComparison.OrdinalIgnoreCase))
            return "PROD-001";
            
        return null;
    }
    
    private bool DetermineIfEscalationRequired(string escalationText)
    {
        // In a real implementation, this would use more sophisticated parsing
        // For now, check for keywords
        
        return escalationText.Contains("escalate", StringComparison.OrdinalIgnoreCase) ||
               escalationText.Contains("supervisor", StringComparison.OrdinalIgnoreCase) ||
               escalationText.Contains("manager", StringComparison.OrdinalIgnoreCase);
    }
    
    private List<string> ExtractSuggestedActions(string responseText)
    {
        // In a real implementation, this would use more sophisticated parsing
        // For now, look for numbered steps or bullet points
        
        var actions = new List<string>();
        var lines = responseText.Split('\n');
        
        foreach (var line in lines)
        {
            var trimmedLine = line.trim();
            
            if (Regex.IsMatch(trimmedLine, @"^\d+\.\s") || 
                trimmedLine.StartsWith("- ") || 
                trimmedLine.StartsWith("* "))
            {
                actions.Add(trimmedLine);
            }
        }
        
        return actions;
    }
    
    private string TruncateContent(string content, int maxLength)
    {
        if (content.Length <= maxLength)
            return content;
            
        return content.Substring(0, maxLength - 3) + "...";
    }
}

public class CustomerInquiryRequest
{
    public string Inquiry { get; set; }
    public string CustomerId { get; set; }
    public string ProductId { get; set; }
}

public class CustomerInquiryResponse
{
    public string InquiryId { get; set; }
    public string Category { get; set; }
    public string ResponseOptions { get; set; }
    public bool RequiresEscalation { get; set; }
    public string NextSteps { get; set; }
    public List<KnowledgeReference> RelevantKnowledge { get; set; } = new List<KnowledgeReference>();
}

public class TroubleshootingRequest
{
    public string ProductId { get; set; }
    public string IssueDescription { get; set; }
    public string PreviousStepsTaken { get; set; }
}

public class TroubleshootingResponse
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string Diagnosis { get; set; }
    public string SolutionSteps { get; set; }
    public string RootCauseAnalysis { get; set; }
    public List<KnowledgeReference> RelatedKnowledgeBase { get; set; } = new List<KnowledgeReference>();
}

public class ConversationRequest
{
    public string CustomerId { get; set; }
    public List<ConversationMessage> ConversationHistory { get; set; } = new List<ConversationMessage>();
    public string CurrentMessage { get; set; }
}

public class ConversationMessage
{
    public string Role { get; set; } // "Customer" or "Agent"
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ConversationResponse
{
    public string Response { get; set; }
    public string SentimentAnalysis { get; set; }
    public List<string> SuggestedActions { get; set; } = new List<string>();
}

public class KnowledgeReference
{
    public string Title { get; set; }
    public string Source { get; set; }
    public string Snippet { get; set; }
}

public class CustomerContext
{
    public string CustomerId { get; set; }
    public string Name { get; set; }
    public string AccountType { get; set; }
    public List<string> PurchasedProducts { get; set; } = new List<string>();
    public List<string> RecentInteractions { get; set; } = new List<string>();
}

public class ProductInfo
{
    public string ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
    public List<string> CommonIssues { get; set; } = new List<string>();
}
