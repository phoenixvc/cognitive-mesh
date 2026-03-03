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
    /// Orchestrator for ConclAIve structured reasoning.
    /// This is the main entry point that coordinates between different reasoning recipes
    /// to turn raw model outputs into structured, auditable reasoning.
    /// </summary>
    public class ConclAIveOrchestrator : IConclAIveOrchestratorPort
    {
        private readonly ILogger<ConclAIveOrchestrator> _logger;
        private readonly IDebateReasoningPort _debateReasoning;
        private readonly ISequentialReasoningPort _sequentialReasoning;
        private readonly IStrategicSimulationPort _strategicSimulation;
        private readonly ILLMClient _llmClient;

        // Pre-compiled regex for better performance
        private static readonly Regex NumberedLineRegex = new Regex(
            @"^\d+[\.\)]\s*",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="ConclAIveOrchestrator"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <param name="debateReasoning">The debate reasoning engine port.</param>
        /// <param name="sequentialReasoning">The sequential reasoning engine port.</param>
        /// <param name="strategicSimulation">The strategic simulation engine port.</param>
        /// <param name="llmClient">The LLM client used for recipe selection.</param>
        public ConclAIveOrchestrator(
            ILogger<ConclAIveOrchestrator> logger,
            IDebateReasoningPort debateReasoning,
            ISequentialReasoningPort sequentialReasoning,
            IStrategicSimulationPort strategicSimulation,
            ILLMClient llmClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _debateReasoning = debateReasoning ?? throw new ArgumentNullException(nameof(debateReasoning));
            _sequentialReasoning = sequentialReasoning ?? throw new ArgumentNullException(nameof(sequentialReasoning));
            _strategicSimulation = strategicSimulation ?? throw new ArgumentNullException(nameof(strategicSimulation));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        /// <inheritdoc />
        public async Task<ReasoningOutput> ReasonAsync(
            string query,
            ReasoningRecipeType? recipeType = null,
            Dictionary<string, string>? context = null)
        {
            _logger.LogInformation("ConclAIve orchestrator processing query: {Query}", query);

            context ??= new Dictionary<string, string>();

            // Determine the best reasoning recipe if not specified
            var selectedRecipe = recipeType ?? await SelectReasoningRecipeAsync(query, context);

            _logger.LogInformation("Selected reasoning recipe: {Recipe}", selectedRecipe);

            return selectedRecipe switch
            {
                ReasoningRecipeType.DebateAndVote => await ExecuteDebateReasoningAsync(query, context),
                ReasoningRecipeType.Sequential => await ExecuteSequentialReasoningAsync(query, context),
                ReasoningRecipeType.StrategicSimulation => await ExecuteStrategicSimulationAsync(query, context),
                _ => throw new ArgumentException($"Unknown reasoning recipe type: {selectedRecipe}")
            };
        }

        private async Task<ReasoningRecipeType> SelectReasoningRecipeAsync(string query, Dictionary<string, string> context)
        {
            _logger.LogDebug("Auto-selecting reasoning recipe for query");

            var selectionPrompt = $@"Analyze the following query and determine the most appropriate reasoning approach:

Query: {query}

Context: {string.Join(", ", context.Select(kv => $"{kv.Key}: {kv.Value}"))}

Available reasoning recipes:
1. DEBATE_AND_VOTE: Best for controversial topics, ethical dilemmas, or when multiple valid perspectives exist. Uses opposing viewpoints to generate ideas.
2. SEQUENTIAL: Best for complex problems that benefit from step-by-step decomposition into specialized phases.
3. STRATEGIC_SIMULATION: Best for future planning, risk analysis, or when exploring multiple possible scenarios.

Which recipe is most appropriate? Respond with ONLY the recipe name (DEBATE_AND_VOTE, SEQUENTIAL, or STRATEGIC_SIMULATION).";

            // Use very low temperature for deterministic recipe selection
            var response = await _llmClient.GenerateCompletionAsync(
                selectionPrompt,
                maxTokens: 50,
                temperature: 0.1f
            );
            var recipeName = response.Trim().ToUpperInvariant();

            if (recipeName.Contains("DEBATE") || recipeName.Contains("VOTE"))
            {
                return ReasoningRecipeType.DebateAndVote;
            }
            else if (recipeName.Contains("STRATEGIC") || recipeName.Contains("SIMULATION"))
            {
                return ReasoningRecipeType.StrategicSimulation;
            }
            else
            {
                // Default to Sequential for most complex queries
                return ReasoningRecipeType.Sequential;
            }
        }

        private async Task<ReasoningOutput> ExecuteDebateReasoningAsync(string query, Dictionary<string, string> context)
        {
            _logger.LogDebug("Executing debate reasoning");

            // Generate diverse perspectives for the debate
            var perspectives = await GeneratePerspectivesAsync(query, context);

            var debateRequest = new DebateRequest
            {
                Question = query,
                Perspectives = perspectives,
                Context = context,
                VotingMechanism = "consensus"
            };

            return await _debateReasoning.ExecuteDebateAsync(debateRequest);
        }

        private async Task<ReasoningOutput> ExecuteSequentialReasoningAsync(string query, Dictionary<string, string> context)
        {
            _logger.LogDebug("Executing sequential reasoning");

            var sequentialRequest = new SequentialReasoningRequest
            {
                Question = query,
                Phases = new List<string>(), // Let the engine auto-decompose
                Context = context
            };

            return await _sequentialReasoning.ExecuteSequentialReasoningAsync(sequentialRequest);
        }

        private async Task<ReasoningOutput> ExecuteStrategicSimulationAsync(string query, Dictionary<string, string> context)
        {
            _logger.LogDebug("Executing strategic simulation");

            // Determine relevant strategic patterns
            var patterns = await DetermineStrategicPatternsAsync(query);

            var simulationRequest = new StrategicSimulationRequest
            {
                Scenario = query,
                Patterns = patterns,
                Constraints = context.ContainsKey("constraints")
                    ? context["constraints"].Split(',').Select(c => c.Trim()).ToList()
                    : new List<string>(),
                DataPoints = context,
                ScenarioCount = 3
            };

            return await _strategicSimulation.ExecuteStrategicSimulationAsync(simulationRequest);
        }

        private async Task<List<string>> GeneratePerspectivesAsync(string query, Dictionary<string, string> context)
        {
            var perspectivePrompt = $@"For the following question, identify 4-6 diverse perspectives or ideological viewpoints that would provide valuable, contrasting insights:

Question: {query}

Context: {string.Join(", ", context.Select(kv => $"{kv.Key}: {kv.Value}"))}

List the perspectives, one per line. Examples: ""Progressive Social Perspective"", ""Free Market Economics"", ""Environmental Sustainability"", ""Traditional Values"", ""Technological Optimist"", ""Risk-Averse Pragmatist""

Perspectives:";

            var response = await _llmClient.GenerateCompletionAsync(
                perspectivePrompt,
                maxTokens: 300,
                temperature: 0.7f
            );

            var perspectives = response
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("Perspectives:", StringComparison.OrdinalIgnoreCase))
                .Select(line => NumberedLineRegex.Replace(line, ""))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Take(6)
                .ToList();

            // Ensure we have at least 2 perspectives
            if (perspectives.Count < 2)
            {
                perspectives = new List<string> { "Supportive Perspective", "Critical Perspective", "Balanced Perspective" };
            }

            return perspectives;
        }

        private async Task<List<string>> DetermineStrategicPatternsAsync(string query)
        {
            var patternPrompt = $@"For the following strategic scenario, identify 2-3 relevant strategic analysis frameworks or patterns:

Scenario: {query}

Common patterns include: SWOT Analysis, Porter's Five Forces, PESTEL Analysis, Scenario Planning, Risk Assessment, Value Chain Analysis, Blue Ocean Strategy, etc.

List only the pattern names, one per line:";

            var response = await _llmClient.GenerateCompletionAsync(
                patternPrompt,
                maxTokens: 200,
                temperature: 0.5f
            );

            var patterns = response
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => NumberedLineRegex.Replace(line, ""))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Take(3)
                .ToList();

            // Default patterns if none identified
            if (!patterns.Any())
            {
                patterns.Add("SWOT Analysis");
                patterns.Add("Risk Assessment");
            }

            return patterns;
        }
    }
}
