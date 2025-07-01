using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.LLM
{
    /// <summary>
    /// A mock LLM provider for testing purposes.
    /// </summary>
    public class MockLLMProvider : ILLMProvider
    {
        private readonly ILogger<MockLLMProvider> _logger;
        private readonly Random _random = new Random();

        public MockLLMProvider(ILogger<MockLLMProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Completes a prompt with mock data.
        /// </summary>
        public async Task<string> CompletePromptAsync(string prompt, LLMOptions options = null)
        {
            _logger.LogInformation("Mock LLM completing prompt with {Length} characters", prompt.Length);
            
            // Simulate processing time
            await Task.Delay(500 + _random.Next(1000));
            
            // Generate a mock response based on prompt content
            var response = GenerateMockResponse(prompt, options);
            
            _logger.LogInformation("Mock LLM generated response with {Length} characters", response.Length);
            return response;
        }

        /// <summary>
        /// Creates a mock embedding.
        /// </summary>
        public async Task<string> CreateEmbeddingAsync(string text)
        {
            _logger.LogInformation("Mock LLM creating embedding for text with {Length} characters", text.Length);
            
            // Simulate processing time
            await Task.Delay(200 + _random.Next(300));
            
            // Generate a mock embedding (32-dim vector of random values)
            var embedding = new float[32];
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)(_random.NextDouble() * 2 - 1); // Values between -1 and 1
            }
            
            return JsonSerializer.Serialize(embedding);
        }

        /// <summary>
        /// Generates a mock response based on the prompt content.
        /// </summary>
        private string GenerateMockResponse(string prompt, LLMOptions options)
        {
            var responseBuilder = new StringBuilder();
            
            // Identify if this is a task/instruction
            if (prompt.Contains("# Task") || prompt.Contains("instructions"))
            {
                responseBuilder.AppendLine("I'll help you with that task. Here's what I can do:");
                responseBuilder.AppendLine();
                
                if (prompt.Contains("design") || prompt.Contains("architecture"))
                {
                    responseBuilder.AppendLine("## Architecture Design");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("Based on your requirements, I recommend a microservices architecture with the following components:");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("1. Frontend service (React.js)");
                    responseBuilder.AppendLine("2. API Gateway (Express.js)");
                    responseBuilder.AppendLine("3. Authentication Service (Node.js)");
                    responseBuilder.AppendLine("4. Data Processing Service (Python)");
                    responseBuilder.AppendLine("5. Storage Service (MongoDB)");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("Here's a diagram of how they connect:");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("```");
                    responseBuilder.AppendLine("Frontend → API Gateway → Auth Service");
                    responseBuilder.AppendLine("                      ↓");
                    responseBuilder.AppendLine("                Data Service → Storage");
                    responseBuilder.AppendLine("```");
                }
                else if (prompt.Contains("code") || prompt.Contains("implement"))
                {
                    responseBuilder.AppendLine("## Implementation");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("Here's an implementation of the functionality you requested:");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("```csharp");
                    responseBuilder.AppendLine("public class DataProcessor");
                    responseBuilder.AppendLine("{");
                    responseBuilder.AppendLine("    public async Task<Result> ProcessDataAsync(string input)");
                    responseBuilder.AppendLine("    {");
                    responseBuilder.AppendLine("        // Parse input");
                    responseBuilder.AppendLine("        var data = JsonSerializer.Deserialize<InputData>(input);");
                    responseBuilder.AppendLine("        ");
                    responseBuilder.AppendLine("        // Process");
                    responseBuilder.AppendLine("        var result = new Result { Success = true };");
                    responseBuilder.AppendLine("        ");
                    responseBuilder.AppendLine("        // Return");
                    responseBuilder.AppendLine("        return result;");
                    responseBuilder.AppendLine("    }");
                    responseBuilder.AppendLine("}");
                    responseBuilder.AppendLine("```");
                }
                else if (prompt.Contains("svg") || prompt.Contains("image"))
                {
                    responseBuilder.AppendLine("I'll generate an SVG image for you using the SVG Generator tool:");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("<tool:svg_generator>");
                    responseBuilder.AppendLine("{");
                    responseBuilder.AppendLine("  \"type\": \"circle\",");
                    responseBuilder.AppendLine("  \"size\": 200,");
                    responseBuilder.AppendLine("  \"color\": \"#3498db\",");
                    responseBuilder.AppendLine("  \"text\": \"CM\"");
                    responseBuilder.AppendLine("}");
                    responseBuilder.AppendLine("</tool>");
                }
                else
                {
                    responseBuilder.AppendLine("## Analysis");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("Based on my analysis, here are the key points to consider:");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("1. Main requirement: Performance and scalability");
                    responseBuilder.AppendLine("2. Constraints: Budget and timeline");
                    responseBuilder.AppendLine("3. Recommended approach: Iterative development");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine("Let me know if you need more specific information about any aspect of this analysis.");
                }
            }
            else
            {
                responseBuilder.AppendLine("I'm not sure what you're asking for. Could you provide more details about your requirements?");
                responseBuilder.AppendLine();
                responseBuilder.AppendLine("I can help with:");
                responseBuilder.AppendLine("- Software architecture design");
                responseBuilder.AppendLine("- Code implementation");
                responseBuilder.AppendLine("- Data analysis");
                responseBuilder.AppendLine("- SVG image generation");
            }
            
            return responseBuilder.ToString();
        }
    }
}