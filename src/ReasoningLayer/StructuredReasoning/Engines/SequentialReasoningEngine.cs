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
    /// Engine for Sequential Reasoning.
    /// Decomposes complex questions into specialized phases (human-like reasoning steps)
    /// and integrates the results into a comprehensive conclusion.
    /// </summary>
    public class SequentialReasoningEngine : ISequentialReasoningPort
    {
        private readonly ILogger<SequentialReasoningEngine> _logger;
        private readonly ILLMClient _llmClient;

        // Pre-compiled regex for better performance
        private static readonly Regex ConfidenceRegex = new Regex(
            @"confidence[:\s]+(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Regex NumberedLineRegex = new Regex(
            @"^\d+[\.\)]\s*",
            RegexOptions.Compiled
        );

        public SequentialReasoningEngine(
            ILogger<SequentialReasoningEngine> logger,
            ILLMClient llmClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        /// <inheritdoc />
        public async Task<ReasoningOutput> ExecuteSequentialReasoningAsync(SequentialReasoningRequest request)
        {
            _logger.LogInformation("Starting sequential reasoning for question: {Question}", request.Question);

            var output = new ReasoningOutput
            {
                RecipeType = ReasoningRecipeType.Sequential,
                Metadata =
                {
                    ["question"] = request.Question
                }
            };

            // Step 1: Determine the phases (auto-decompose if not provided)
            var phases = request.Phases.Any() ? request.Phases : await DecomposeQuestionAsync(request.Question);
            output.Metadata["phaseCount"] = phases.Count.ToString();

            _logger.LogDebug("Sequential reasoning will proceed through {PhaseCount} phases", phases.Count);

            // Step 2: Execute each phase sequentially, building on previous results
            var phaseResults = new List<string>();
            int stepNumber = 1;

            foreach (var phase in phases)
            {
                _logger.LogDebug("Executing phase: {Phase}", phase);

                var previousContext = phaseResults.Any()
                    ? $"\n\nResults from previous phases:\n{string.Join("\n\n", phaseResults.Select((r, i) => $"Phase {i + 1} ({phases[i]}): {r}"))}"
                    : "";

                var phasePrompt = $@"You are working on the following complex question:

Question: {request.Question}

Context: {string.Join(", ", request.Context.Select(kv => $"{kv.Key}: {kv.Value}"))}

Current Phase: {phase}

Your task in this phase is to focus specifically on: {phase}
{previousContext}

Provide a thorough analysis for this specific phase. Build upon previous phase results if applicable.";

                // Use focused temperature for analytical reasoning
                var phaseResponse = await _llmClient.GenerateCompletionAsync(
                    phasePrompt,
                    maxTokens: 1000,
                    temperature: 0.6f
                );
                phaseResults.Add(phaseResponse);

                output.ReasoningTrace.Add(new ReasoningStep
                {
                    StepNumber = stepNumber++,
                    StepName = phase,
                    Input = phasePrompt,
                    Output = phaseResponse,
                    Metadata =
                    {
                        ["phaseIndex"] = (phases.IndexOf(phase) + 1).ToString(),
                        ["totalPhases"] = phases.Count.ToString()
                    }
                });
            }

            // Step 3: Integrate all phase results into a global conclusion
            _logger.LogDebug("Integrating phase results into final conclusion");

            var integrationPrompt = $@"You have completed a multi-phase analysis of the following question:

Question: {request.Question}

The analysis proceeded through {phases.Count} specialized phases:

{string.Join("\n\n", phases.Select((phase, i) => $"**Phase {i + 1}: {phase}**\n{phaseResults[i]}"))}

Now, integrate these phase results into a comprehensive, coherent conclusion that:
1. Synthesizes insights from all phases
2. Addresses the original question completely
3. Identifies key findings and recommendations
4. Acknowledges any limitations or uncertainties

Provide your confidence level (0-100) in this integrated conclusion.";

            // Use higher tokens for comprehensive integration
            var integrationResponse = await _llmClient.GenerateCompletionAsync(
                integrationPrompt,
                maxTokens: 1500,
                temperature: 0.7f
            );

            output.ReasoningTrace.Add(new ReasoningStep
            {
                StepNumber = stepNumber++,
                StepName = "Integration and Final Conclusion",
                Input = integrationPrompt,
                Output = integrationResponse,
                Metadata = { ["type"] = "integration" }
            });

            // Parse conclusion and confidence
            var (conclusion, confidence) = ParseConclusionWithConfidence(integrationResponse);
            output.Conclusion = conclusion;
            output.Confidence = confidence;

            _logger.LogInformation("Sequential reasoning completed with confidence: {Confidence}", confidence);

            return output;
        }

        private async Task<List<string>> DecomposeQuestionAsync(string question)
        {
            _logger.LogDebug("Auto-decomposing question into phases");

            var decompositionPrompt = $@"Break down the following complex question into 3-5 specialized reasoning phases:

Question: {question}

Each phase should represent a distinct aspect or stage of reasoning needed to fully address this question.
Common phases might include: Understanding the Context, Analyzing Root Causes, Evaluating Options, Considering Constraints, Forming Recommendations, etc.

List the phases, one per line, without numbering:";

            // Use low temperature for phase decomposition
            var response = await _llmClient.GenerateCompletionAsync(
                decompositionPrompt,
                maxTokens: 400,
                temperature: 0.5f
            );

            var phases = response
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => NumberedLineRegex.Replace(line, ""))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            // Ensure we have at least one phase
            if (!phases.Any())
            {
                phases.Add("Comprehensive Analysis");
            }

            return phases;
        }

        private (string conclusion, double confidence) ParseConclusionWithConfidence(string response)
        {
            // Look for confidence level in the response using pre-compiled regex
            var confidenceMatch = ConfidenceRegex.Match(response);

            double confidence = 0.75; // Default confidence for sequential reasoning
            if (confidenceMatch.Success && int.TryParse(confidenceMatch.Groups[1].Value, out int confValue))
            {
                confidence = confValue / 100.0;
            }

            return (response, confidence);
        }
    }
}
