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
    /// Engine for Strategic Simulation reasoning.
    /// Explores multiple scenarios using patterns, data, and constraints
    /// to anticipate possible outcomes and inform strategic decisions.
    /// </summary>
    public class StrategicSimulationEngine : IStrategicSimulationPort
    {
        private readonly ILogger<StrategicSimulationEngine> _logger;
        private readonly ILLMClient _llmClient;
        
        // Pre-compiled regex for better performance
        private static readonly Regex ConfidenceRegex = new Regex(
            @"confidence[:\s]+(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
        
        private static readonly Regex ProbabilityRegex = new Regex(
            @"\d+",
            RegexOptions.Compiled
        );

        public StrategicSimulationEngine(
            ILogger<StrategicSimulationEngine> logger,
            ILLMClient llmClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        /// <inheritdoc />
        public async Task<ReasoningOutput> ExecuteStrategicSimulationAsync(StrategicSimulationRequest request)
        {
            _logger.LogInformation("Starting strategic simulation for scenario: {Scenario}", request.Scenario);

            var output = new ReasoningOutput
            {
                RecipeType = ReasoningRecipeType.StrategicSimulation,
                Metadata =
                {
                    ["scenario"] = request.Scenario,
                    ["scenarioCount"] = request.ScenarioCount.ToString(),
                    ["patternCount"] = request.Patterns.Count.ToString()
                }
            };

            var exploredScenarios = new List<ExploredScenario>();
            int stepNumber = 1;

            // Step 1: Apply strategic patterns to understand the situation
            _logger.LogDebug("Applying strategic patterns: {Patterns}", string.Join(", ", request.Patterns));

            var patternAnalysisPrompt = $@"Apply the following strategic analysis patterns to understand this scenario:

Scenario: {request.Scenario}

Patterns to apply: {string.Join(", ", request.Patterns)}

Data Points:
{string.Join("\n", request.DataPoints.Select(kv => $"- {kv.Key}: {kv.Value}"))}

Constraints:
{string.Join("\n", request.Constraints.Select(c => $"- {c}"))}

For each pattern, provide:
1. Key insights from applying this pattern
2. Strategic factors identified
3. Critical assumptions";

            // Use low temperature for analytical pattern application
            var patternAnalysisResponse = await _llmClient.GenerateCompletionAsync(
                patternAnalysisPrompt,
                maxTokens: 1200,
                temperature: 0.4f
            );

            output.ReasoningTrace.Add(new ReasoningStep
            {
                StepNumber = stepNumber++,
                StepName = "Pattern Analysis",
                Input = patternAnalysisPrompt,
                Output = patternAnalysisResponse,
                Metadata = { ["type"] = "pattern_analysis" }
            });

            // Step 2: Generate and explore multiple scenarios
            for (int i = 1; i <= request.ScenarioCount; i++)
            {
                _logger.LogDebug("Exploring scenario {ScenarioNumber} of {TotalScenarios}", i, request.ScenarioCount);

                var scenarioPrompt = $@"Based on the strategic analysis, explore a distinct future scenario #{i}:

Original Scenario: {request.Scenario}

Pattern Analysis Results:
{patternAnalysisResponse}

Generate a specific, plausible scenario that:
1. Describes a distinct possible future outcome
2. Identifies key drivers and events leading to this outcome
3. Assesses the probability (0-100)
4. Lists 3-5 risk factors
5. Lists 3-5 opportunities

Format:
Description: [scenario description]
Probability: [0-100]
Risk Factors:
- [factor 1]
Opportunities:
- [opportunity 1]";

                // Use moderate temperature for scenario generation
                var scenarioResponse = await _llmClient.GenerateCompletionAsync(
                    scenarioPrompt,
                    maxTokens: 1000,
                    temperature: 0.6f
                );

                var exploredScenario = ParseScenarioResponse(scenarioResponse, i);
                exploredScenarios.Add(exploredScenario);

                output.ReasoningTrace.Add(new ReasoningStep
                {
                    StepNumber = stepNumber++,
                    StepName = $"Scenario {i}: {exploredScenario.Description.Substring(0, Math.Min(50, exploredScenario.Description.Length))}...",
                    Input = scenarioPrompt,
                    Output = scenarioResponse,
                    Metadata =
                    {
                        ["type"] = "scenario_exploration",
                        ["scenarioIndex"] = i.ToString(),
                        ["probability"] = exploredScenario.Probability.ToString("F2")
                    }
                });
            }

            // Step 3: Compare scenarios and recommend strategic actions
            _logger.LogDebug("Comparing scenarios and forming strategic recommendations");

            var comparisonPrompt = $@"You have explored {exploredScenarios.Count} potential scenarios:

{string.Join("\n\n", exploredScenarios.Select((s, i) => $"**Scenario {i + 1}** (Probability: {s.Probability:F0}%)\n{s.Description}\nRisks: {string.Join(", ", s.RiskFactors)}\nOpportunities: {string.Join(", ", s.Opportunities)}"))}

Provide a strategic analysis that:
1. Compares the scenarios and their relative likelihoods
2. Identifies common patterns across scenarios
3. Recommends strategic actions to:
   - Prepare for the most likely scenarios
   - Hedge against high-impact risks
   - Position to capitalize on opportunities
4. Prioritizes immediate, short-term, and long-term actions

Provide your confidence level (0-100) in these recommendations based on the quality of the analysis and available data.";

            // Use focused temperature for strategic recommendations
            var comparisonResponse = await _llmClient.GenerateCompletionAsync(
                comparisonPrompt,
                maxTokens: 1500,
                temperature: 0.5f
            );

            output.ReasoningTrace.Add(new ReasoningStep
            {
                StepNumber = stepNumber++,
                StepName = "Strategic Comparison and Recommendations",
                Input = comparisonPrompt,
                Output = comparisonResponse,
                Metadata = { ["type"] = "strategic_recommendation" }
            });

            // Parse conclusion and confidence
            var (conclusion, confidence) = ParseStrategicConclusion(comparisonResponse);
            output.Conclusion = conclusion;
            output.Confidence = confidence;

            _logger.LogInformation("Strategic simulation completed with confidence: {Confidence}", confidence);

            return output;
        }

        private ExploredScenario ParseScenarioResponse(string response, int scenarioNumber)
        {
            var scenario = new ExploredScenario
            {
                ScenarioId = $"scenario_{scenarioNumber}_{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            bool inRiskFactors = false;
            bool inOpportunities = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
                {
                    scenario.Description = trimmed.Substring("Description:".Length).Trim();
                }
                else if (trimmed.StartsWith("Probability:", StringComparison.OrdinalIgnoreCase))
                {
                    var probStr = trimmed.Substring("Probability:".Length).Trim();
                    var probMatch = ProbabilityRegex.Match(probStr);
                    if (probMatch.Success && double.TryParse(probMatch.Value, out double prob))
                    {
                        scenario.Probability = prob / 100.0;
                    }
                }
                else if (trimmed.StartsWith("Risk Factors:", StringComparison.OrdinalIgnoreCase))
                {
                    inRiskFactors = true;
                    inOpportunities = false;
                }
                else if (trimmed.StartsWith("Opportunities:", StringComparison.OrdinalIgnoreCase))
                {
                    inRiskFactors = false;
                    inOpportunities = true;
                }
                else if (trimmed.StartsWith("-"))
                {
                    var item = trimmed.Substring(1).Trim();
                    if (inRiskFactors)
                    {
                        scenario.RiskFactors.Add(item);
                    }
                    else if (inOpportunities)
                    {
                        scenario.Opportunities.Add(item);
                    }
                }
            }

            // Fallback values
            if (string.IsNullOrEmpty(scenario.Description))
            {
                scenario.Description = $"Scenario {scenarioNumber}: {response.Substring(0, Math.Min(100, response.Length))}";
            }

            if (scenario.Probability == 0)
            {
                scenario.Probability = 1.0 / 3.0; // Default equal probability
            }

            scenario.AnticipatedOutcome = scenario.Description;

            return scenario;
        }

        private (string conclusion, double confidence) ParseStrategicConclusion(string response)
        {
            // Look for confidence level in the response using pre-compiled regex
            var confidenceMatch = ConfidenceRegex.Match(response);

            double confidence = 0.65; // Default confidence for strategic simulation
            if (confidenceMatch.Success && int.TryParse(confidenceMatch.Groups[1].Value, out int confValue))
            {
                confidence = confValue / 100.0;
            }

            return (response, confidence);
        }
    }
}
