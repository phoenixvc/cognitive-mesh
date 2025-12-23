using CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports;
using System.Text.RegularExpressions;

namespace CognitiveMesh.ReasoningLayer.EthicalReasoning.Engines
{
    /// <summary>
    /// Implements the core reasoning capabilities for ensuring that all agentic actions within the Cognitive Mesh
    /// are ethically sound, justifiable, and respectful of user autonomy. This engine is based on the philosophical
    /// principles of Robert Brandom's normative pragmatism, where agency is constituted by participating in the
    /// "game of giving and asking for reasons."
    /// </summary>
    public class NormativeAgencyEngine : INormativeAgencyPort
    {
        private readonly ILogger<NormativeAgencyEngine> _logger;

        // A predefined set of core normative principles the mesh must adhere to.
        private static readonly Dictionary<string, Regex> NormativePrinciples = new()
        {
            { "Do No Harm", new Regex(@"\b(prevent|avoid|minimize)\b.*\b(harm|damage|loss)\b", RegexOptions.IgnoreCase) },
            { "Respect Autonomy", new Regex(@"\b(user choice|consent|control|autonomy|option)\b", RegexOptions.IgnoreCase) },
            { "Promote Fairness", new Regex(@"\b(fair|unbiased|equitable|impartial|neutral)\b", RegexOptions.IgnoreCase) },
            { "Ensure Transparency", new Regex(@"\b(explain|transparent|clear|justify|rationale)\b", RegexOptions.IgnoreCase) }
        };

        public NormativeAgencyEngine(ILogger<NormativeAgencyEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<NormativeActionValidationResponse> ValidateActionAsync(NormativeActionValidationRequest request)
        {
            _logger.LogInformation("Validating normative action for agent '{AgentId}': {ProposedAction}", request.AgentId, request.ProposedAction);
            var response = new NormativeActionValidationResponse { IsValid = true };

            if (request.Justifications == null || !request.Justifications.Any())
            {
                response.IsValid = false;
                response.Violations.Add("Normative Violation: Action proposed without any justification. Agency requires giving reasons.");
                _logger.LogWarning("Action by agent '{AgentId}' is invalid due to missing justifications.", request.AgentId);
                return Task.FromResult(response);
            }

            var alignedPrinciples = new HashSet<string>();
            foreach (var justification in request.Justifications)
            {
                foreach (var principle in NormativePrinciples)
                {
                    if (principle.Value.IsMatch(justification))
                    {
                        alignedPrinciples.Add(principle.Key);
                    }
                }
            }

            if (alignedPrinciples.Count == 0)
            {
                response.IsValid = false;
                response.Violations.Add("Normative Violation: Justifications do not align with any core ethical principles (Harm Prevention, Autonomy, Fairness, Transparency).");
                _logger.LogWarning("Action by agent '{AgentId}' is invalid because its justifications do not align with core principles.", request.AgentId);
            }
            else
            {
                _logger.LogInformation("Action by agent '{AgentId}' aligns with the following principles: {Principles}", request.AgentId, string.Join(", ", alignedPrinciples));
            }

            // Additional checks could be added here, e.g., checking for contradictions between justifications.

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<ReasoningChainValidationResponse> ValidateReasoningChainAsync(ReasoningChainValidationRequest request)
        {
            _logger.LogInformation("Validating reasoning chain with {StepCount} steps.", request.Chain.Count);
            var response = new ReasoningChainValidationResponse { IsValid = true };

            if (request.Chain == null || request.Chain.Count == 0)
            {
                response.IsValid = false;
                response.IdentifiedIssues.Add("Reasoning chain is empty.");
                return Task.FromResult(response);
            }

            var issues = DetectFallacies(request.Chain);
            if (issues.Any())
            {
                response.IsValid = false;
                response.IdentifiedIssues.AddRange(issues);
                _logger.LogWarning("Reasoning chain found to be invalid with the following issues: {Issues}", string.Join("; ", issues));
            }
            else
            {
                _logger.LogInformation("Reasoning chain validated successfully with no logical fallacies detected.");
            }

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<ConsentIntegrityAssessmentResponse> AssessConsentIntegrityAsync(ConsentIntegrityAssessmentRequest request)
        {
            _logger.LogInformation("Assessing integrity of consent prompt: '{PromptText}'", request.PromptText);
            var response = new ConsentIntegrityAssessmentResponse { IntegrityScore = 1.0 }; // Start with a perfect score

            var darkPatterns = DetectDarkPatterns(request);
            if (darkPatterns.Any())
            {
                response.PotentialIssues.AddRange(darkPatterns);
                // Penalize the score for each dark pattern found.
                response.IntegrityScore -= darkPatterns.Count * 0.25;
                response.IntegrityScore = Math.Max(0, response.IntegrityScore); // Ensure score doesn't go below 0.
                _logger.LogWarning("Consent prompt contains potential dark patterns: {Patterns}", string.Join("; ", darkPatterns));
            }
            else
            {
                _logger.LogInformation("Consent prompt assessed to have high integrity.");
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// Analyzes a reasoning chain to detect common logical fallacies.
        /// </summary>
        private List<string> DetectFallacies(List<ReasoningStep> chain)
        {
            var issues = new List<string>();
            var premises = chain.Select(s => s.Premise.ToLower()).ToList();
            var conclusions = chain.Select(s => s.Conclusion.ToLower()).ToList();

            // Check for Circular Reasoning (Conclusion appears in Premises)
            if (conclusions.Any(c => premises.Contains(c)))
            {
                issues.Add("Logical Fallacy Detected: Circular Reasoning (the conclusion is used as a premise).");
            }

            // Check for Slippery Slope (simple pattern detection)
            var ifThenChains = chain.Where(s => s.Premise.StartsWith("if", StringComparison.OrdinalIgnoreCase) && s.InferenceType == "Deductive").ToList();
            if (ifThenChains.Count > 2)
            {
                issues.Add("Potential Logical Fallacy: Slippery Slope (a long chain of 'if-then' statements may indicate an unsupported causal leap).");
            }

            // Check for Ad Hominem (attacking the source)
            var adHominemRegex = new Regex(@"\b(source is unreliable|biased source|discredited)\b", RegexOptions.IgnoreCase);
            if (chain.Any(s => adHominemRegex.IsMatch(s.Premise)))
            {
                issues.Add("Potential Logical Fallacy: Ad Hominem (an argument appears to be based on attacking the source rather than the evidence).");
            }
            
            return issues;
        }

        /// <summary>
        /// Analyzes a consent request to detect common dark patterns that manipulate user choice.
        /// </summary>
        private List<string> DetectDarkPatterns(ConsentIntegrityAssessmentRequest request)
        {
            var patterns = new List<string>();

            // Check for Confirmshaming
            var confirmshamingRegex = new Regex(@"\b(no, i don't want|i prefer to miss out|decline the benefits)\b", RegexOptions.IgnoreCase);
            if (request.PresentedOptions.Any(o => confirmshamingRegex.IsMatch(o)))
            {
                patterns.Add("Dark Pattern Detected: Confirmshaming (using guilt to influence user choice).");
            }

            // Check for Forced Action (only one real option)
            if (request.PresentedOptions.Count < 2)
            {
                patterns.Add("Dark Pattern Detected: Forced Action (user is not given at least two distinct options).");
            }

            // Check for Misleading Language (using jargon or overly complex terms)
            var jargonRegex = new Regex(@"\b(synergistic leverage|paradigm shift|holistic integration)\b", RegexOptions.IgnoreCase);
            if (jargonRegex.IsMatch(request.PromptText))
            {
                patterns.Add("Dark Pattern Detected: Misleading Language (prompt contains confusing jargon that may obscure the true meaning).");
            }

            return patterns;
        }
    }
}
