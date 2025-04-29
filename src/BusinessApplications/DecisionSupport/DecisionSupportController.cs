using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class DecisionSupportController : ControllerBase
{
    private readonly CognitiveMeshCoordinator _coordinator;
    private readonly CausalUnderstandingComponent _causalComponent;
    private readonly ILogger<DecisionSupportController> _logger;
    private readonly FeatureFlagManager _featureFlagManager;

    public DecisionSupportController(
        CognitiveMeshCoordinator coordinator,
        CausalUnderstandingComponent causalComponent,
        ILogger<DecisionSupportController> logger,
        FeatureFlagManager featureFlagManager)
    {
        _coordinator = coordinator;
        _causalComponent = causalComponent;
        _logger = logger;
        _featureFlagManager = featureFlagManager;
    }

    [HttpPost("analyze")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> AnalyzeSituation([FromBody] SituationAnalysisRequest request)
    {
        try
        {
            if (!_featureFlagManager.EnableADK)
            {
                return BadRequest("Feature not enabled.");
            }

            // Analyze situation from multiple perspectives
            var analysisQuery = $"Analyze this situation from multiple perspectives: {request.Situation}";

            var options = new QueryOptions
            {
                Perspectives = request.Perspectives ?? new List<string> { "analytical", "critical", "creative", "practical" }
            };

            var analysisResponse = await _coordinator.ProcessQueryAsync(analysisQuery, options);

            // Extract key factors
            var factorsQuery = $"Identify the key factors and their relationships in this situation: {request.Situation}";
            var factorsResponse = await _coordinator.ProcessQueryAsync(factorsQuery);

            // Generate causal understanding
            var causalAnalysis = await _causalComponent.ExtractCausalRelationsAsync(
                request.Situation + "\n" + factorsResponse.Response,
                "decision-support");

            // Perform causal reasoning
            var causalReasoning = await _causalComponent.PerformCausalReasoningAsync(
                $"What are the likely outcomes and their causes in this situation: {request.Situation}?",
                "decision-support");

            // Compile result
            var result = new SituationAnalysisResponse
            {
                SituationId = Guid.NewGuid().ToString(),
                MultiPerspectiveAnalysis = analysisResponse.Response,
                KeyFactors = factorsResponse.Response,
                CausalReasoning = causalReasoning,
                Entities = causalAnalysis.Entities
                    .Select(e => new EntityInfo
                    {
                        Name = e.Name,
                        Type = e.Type,
                        Description = e.Description
                    })
                    .ToList(),
                Relationships = causalAnalysis.Relationships
                    .Select(r => {
                        var causeEntity = causalAnalysis.Entities.FirstOrDefault(e => e.Id == r.CauseEntityId);
                        var effectEntity = causalAnalysis.Entities.FirstOrDefault(e => e.Id == r.EffectEntityId);

                        return new RelationshipInfo
                        {
                            CauseEntity = causeEntity?.Name,
                            EffectEntity = effectEntity?.Name,
                            Type = r.RelationshipType,
                            Strength = r.Strength,
                            Evidence = r.Evidence
                        };
                    })
                    .ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing situation analysis");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpPost("options")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> GenerateOptions([FromBody] OptionsGenerationRequest request)
    {
        try
        {
            if (!_featureFlagManager.EnableLangGraph)
            {
                return BadRequest("Feature not enabled.");
            }

            // Generate options
            var optionsQuery = $"Generate {request.NumberOfOptions} options for addressing this situation: {request.Situation}";
            var optionsResponse = await _coordinator.ProcessQueryAsync(optionsQuery);

            // Analyze options from multiple perspectives
            var analysisOptions = new List<OptionAnalysis>();
            var parsedOptions = ParseOptions(optionsResponse.Response);

            foreach (var option in parsedOptions)
            {
                var analysisQuery = $"Analyze this option from multiple perspectives: {option}. " +
                                   $"Situation context: {request.Situation}";

                var perspectives = new QueryOptions
                {
                    Perspectives = new List<string> { "analytical", "critical", "creative", "practical" }
                };

                var analysisResponse = await _coordinator.ProcessQueryAsync(analysisQuery, perspectives);

                // Evaluate pros and cons
                var prosConsQuery = $"List the pros and cons of this option: {option}. " +
                                   $"Situation context: {request.Situation}";

                var prosConsResponse = await _coordinator.ProcessQueryAsync(prosConsQuery);

                // Predict outcomes
                var outcomesQuery = $"Predict potential outcomes if this option is chosen: {option}. " +
                                   $"Situation context: {request.Situation}";

                var outcomesResponse = await _coordinator.ProcessQueryAsync(outcomesQuery);

                analysisOptions.Add(new OptionAnalysis
                {
                    Option = option,
                    Analysis = analysisResponse.Response,
                    ProsAndCons = prosConsResponse.Response,
                    PotentialOutcomes = outcomesResponse.Response
                });
            }

            // Generate comparison
            var comparisonQuery = $"Compare and contrast these options for addressing the situation:\n" +
                                 $"Situation: {request.Situation}\n" +
                                 $"Options:\n{string.Join("\n", parsedOptions.Select((o, i) => $"{i+1}. {o}"))}";

            var comparisonResponse = await _coordinator.ProcessQueryAsync(comparisonQuery);

            // Compile result
            var result = new OptionsGenerationResponse
            {
                Situation = request.Situation,
                Options = analysisOptions,
                Comparison = comparisonResponse.Response
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing options generation");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpPost("scenarios")]
    [Authorize(Policy = "ReadAccess")]
    public async Task<IActionResult> ExploreScenarios([FromBody] ScenarioExplorationRequest request)
    {
        try
        {
            if (!_featureFlagManager.EnableCrewAI)
            {
                return BadRequest("Feature not enabled.");
            }

            // Generate scenarios
            var scenariosQuery = $"Generate {request.NumberOfScenarios} possible future scenarios for this situation: " +
                                $"{request.Situation}. " +
                                $"Consider these key uncertainties: {string.Join(", ", request.KeyUncertainties)}.";

            var scenariosResponse = await _coordinator.ProcessQueryAsync(scenariosQuery);

            // Parse scenarios
            var parsedScenarios = ParseScenarios(scenariosResponse.Response);

            // Analyze each scenario
            var analyzedScenarios = new List<ScenarioAnalysis>();

            foreach (var scenario in parsedScenarios)
            {
                // Analyze implications
                var implicationsQuery = $"Analyze the implications of this scenario: {scenario}. " +
                                       $"Situation context: {request.Situation}";

                var implicationsResponse = await _coordinator.ProcessQueryAsync(implicationsQuery);

                // Identify early indicators
                var indicatorsQuery = $"What early indicators would suggest this scenario is becoming more likely: {scenario}?";
                var indicatorsResponse = await _coordinator.ProcessQueryAsync(indicatorsQuery);

                // Recommend preparations
                var preparationsQuery = $"What preparations should be made for this scenario: {scenario}?";
                var preparationsResponse = await _coordinator.ProcessQueryAsync(preparationsQuery);

                analyzedScenarios.Add(new ScenarioAnalysis
                {
                    Scenario = scenario,
                    Implications = implicationsResponse.Response,
                    EarlyIndicators = indicatorsResponse.Response,
                    RecommendedPreparations = preparationsResponse.Response
                });
            }

            // Generate strategic recommendations
            var recommendationsQuery = $"Based on these scenarios, what strategic recommendations would you make? " +
                                      $"Situation: {request.Situation}\n" +
                                      $"Scenarios:\n{string.Join("\n", parsedScenarios.Select((s, i) => $"{i+1}. {s}"))}";

            var recommendationsResponse = await _coordinator.ProcessQueryAsync(recommendationsQuery);

            // Compile result
            var result = new ScenarioExplorationResponse
            {
                Situation = request.Situation,
                Scenarios = analyzedScenarios,
                StrategicRecommendations = recommendationsResponse.Response
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scenario exploration");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    private List<string> ParseOptions(string optionsText)
    {
        var options = new List<string>();

        // Split by numbered points or bullet points
        var lines = optionsText.Split('\n');
        var currentOption = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check if this is a new option
            if (Regex.IsMatch(trimmedLine, @"^(\d+\.|Option \d+:|•|\*|-)\s") && currentOption.Length > 0)
            {
                // Add previous option to list
                options.Add(currentOption.ToString().Trim());
                currentOption.Clear();
            }

            // Append to current option
            currentOption.AppendLine(trimmedLine);
        }

        // Add final option if any
        if (currentOption.Length > 0)
        {
            options.Add(currentOption.ToString().Trim());
        }

        return options;
    }

    private List<string> ParseScenarios(string scenariosText)
    {
        var scenarios = new List<string>();

        // Split by numbered points or scenario headings
        var lines = scenariosText.Split('\n');
        var currentScenario = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check if this is a new scenario
            if ((Regex.IsMatch(trimmedLine, @"^(\d+\.|Scenario \d+:|\*|-)\s") ||
                 trimmedLine.StartsWith("# ") ||
                 trimmedLine.StartsWith("## ")) &&
                currentScenario.Length > 0)
            {
                // Add previous scenario to list
                scenarios.Add(currentScenario.ToString().Trim());
                currentScenario.Clear();
            }

            // Append to current scenario
            currentScenario.AppendLine(trimmedLine);
        }

        // Add final scenario if any
        if (currentScenario.Length > 0)
        {
            scenarios.Add(currentScenario.ToString().Trim());
        }

        return scenarios;
    }
}
