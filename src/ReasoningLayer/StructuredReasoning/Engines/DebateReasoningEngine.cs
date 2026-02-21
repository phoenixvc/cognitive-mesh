using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Models;
using CognitiveMesh.ReasoningLayer.StructuredReasoning.Ports;
using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;

namespace CognitiveMesh.ReasoningLayer.StructuredReasoning.Engines
{
    /// <summary>
    /// Engine for Debate &amp; Vote reasoning.
    /// Implements an ideology collider brainstorming system that uses opposing
    /// ideological perspectives to generate and synthesize ideas.
    /// </summary>
    public class DebateReasoningEngine : IDebateReasoningPort
    {
        private readonly ILogger<DebateReasoningEngine> _logger;
        private readonly ILLMClient _llmClient;

        // Pre-compiled regex for better performance
        private static readonly Regex ConfidenceRegex = new Regex(
            @"confidence[:\s]+(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="DebateReasoningEngine"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <param name="llmClient">The LLM client used for generating debate perspectives and synthesis.</param>
        public DebateReasoningEngine(
            ILogger<DebateReasoningEngine> logger,
            ILLMClient llmClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        /// <inheritdoc />
        public async Task<ReasoningOutput> ExecuteDebateAsync(DebateRequest request)
        {
            _logger.LogInformation("Starting debate reasoning for question: {Question}", request.Question);

            var output = new ReasoningOutput
            {
                RecipeType = ReasoningRecipeType.DebateAndVote,
                Metadata =
                {
                    ["question"] = request.Question,
                    ["perspectiveCount"] = request.Perspectives.Count.ToString(),
                    ["votingMechanism"] = request.VotingMechanism
                }
            };

            var perspectives = new List<DebatePerspective>();

            // Step 1: Generate arguments from each perspective
            int stepNumber = 1;
            foreach (var perspective in request.Perspectives)
            {
                _logger.LogDebug("Generating argument from perspective: {Perspective}", perspective);

                var prompt = $@"You are reasoning from the perspective of: {perspective}

Question: {request.Question}

Context: {string.Join(", ", request.Context.Select(kv => $"{kv.Key}: {kv.Value}"))}

Provide a clear, well-reasoned argument from this perspective. Include:
1. Your main position
2. 3-5 supporting points
3. Key assumptions or values that inform this perspective

Format your response as:
Position: [your position]
Supporting Points:
- [point 1]
- [point 2]
- [point 3]";

                // Use moderate temperature for diverse but focused perspectives
                var response = await _llmClient.GenerateCompletionAsync(
                    prompt,
                    maxTokens: 800,
                    temperature: 0.7f
                );

                var debatePerspective = ParsePerspectiveResponse(perspective, response);
                perspectives.Add(debatePerspective);

                output.ReasoningTrace.Add(new ReasoningStep
                {
                    StepNumber = stepNumber++,
                    StepName = $"Perspective: {perspective}",
                    Input = prompt,
                    Output = response,
                    AgentId = perspective,
                    Metadata = { ["type"] = "argument_generation" }
                });
            }

            // Step 2: Cross-examination and critique
            _logger.LogDebug("Performing cross-examination of perspectives");
            var critiques = new List<string>();

            foreach (var perspective in perspectives)
            {
                var otherPerspectives = perspectives.Where(p => p.Name != perspective.Name).ToList();
                var critiquePrompt = $@"You are analyzing the following argument:

Perspective: {perspective.Name}
Position: {perspective.Argument}

Other perspectives have raised these positions:
{string.Join("\n", otherPerspectives.Select(p => $"- {p.Name}: {p.Argument}"))}

Provide a critical analysis:
1. Strengths of this argument
2. Potential weaknesses or blind spots
3. How it compares to other perspectives
4. Key points of agreement or disagreement";

                var critiqueResponse = await _llmClient.GenerateCompletionAsync(
                    critiquePrompt,
                    maxTokens: 600,
                    temperature: 0.6f
                );
                critiques.Add(critiqueResponse);

                output.ReasoningTrace.Add(new ReasoningStep
                {
                    StepNumber = stepNumber++,
                    StepName = $"Critique of {perspective.Name}",
                    Input = critiquePrompt,
                    Output = critiqueResponse,
                    Metadata = { ["type"] = "cross_examination" }
                });
            }

            // Step 3: Synthesis and voting
            _logger.LogDebug("Synthesizing perspectives and determining conclusion");

            var synthesisPrompt = $@"You have been presented with multiple perspectives on the following question:

Question: {request.Question}

Perspectives and Arguments:
{string.Join("\n\n", perspectives.Select(p => $"**{p.Name}**\nPosition: {p.Argument}\nSupporting Points:\n{string.Join("\n", p.SupportingPoints.Select(sp => $"- {sp}"))}"))}

Critiques:
{string.Join("\n\n", critiques)}

Synthesize these perspectives into a balanced conclusion that:
1. Acknowledges the validity in each perspective
2. Identifies common ground and key disagreements
3. Provides a nuanced answer that integrates the best insights
4. Clearly states your final recommendation or conclusion

Provide your confidence level (0-100) in this conclusion.";

            // Use higher tokens for comprehensive synthesis
            var synthesisResponse = await _llmClient.GenerateCompletionAsync(
                synthesisPrompt,
                maxTokens: 1200,
                temperature: 0.7f
            );

            output.ReasoningTrace.Add(new ReasoningStep
            {
                StepNumber = stepNumber++,
                StepName = "Synthesis and Conclusion",
                Input = synthesisPrompt,
                Output = synthesisResponse,
                Metadata = { ["type"] = "synthesis" }
            });

            // Parse conclusion and confidence
            var (conclusion, confidence) = ParseSynthesisResponse(synthesisResponse);
            output.Conclusion = conclusion;
            output.Confidence = confidence;

            _logger.LogInformation("Debate reasoning completed with confidence: {Confidence}", confidence);

            return output;
        }

        private DebatePerspective ParsePerspectiveResponse(string perspectiveName, string response)
        {
            var perspective = new DebatePerspective { Name = perspectiveName };

            // Simple parsing - look for "Position:" and "Supporting Points:"
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            bool inSupportingPoints = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Position:", StringComparison.OrdinalIgnoreCase))
                {
                    perspective.Argument = trimmed.Substring("Position:".Length).Trim();
                }
                else if (trimmed.StartsWith("Supporting Points:", StringComparison.OrdinalIgnoreCase))
                {
                    inSupportingPoints = true;
                }
                else if (inSupportingPoints && trimmed.StartsWith("-"))
                {
                    perspective.SupportingPoints.Add(trimmed.Substring(1).Trim());
                }
            }

            // Fallback if parsing fails
            if (string.IsNullOrEmpty(perspective.Argument))
            {
                perspective.Argument = response;
            }

            return perspective;
        }

        private (string conclusion, double confidence) ParseSynthesisResponse(string response)
        {
            // Look for confidence level in the response using pre-compiled regex
            var confidenceMatch = ConfidenceRegex.Match(response);

            double confidence = 0.7; // Default confidence
            if (confidenceMatch.Success && int.TryParse(confidenceMatch.Groups[1].Value, out int confValue))
            {
                confidence = confValue / 100.0;
            }

            // The conclusion is the full synthesis
            return (response, confidence);
        }
    }
}
