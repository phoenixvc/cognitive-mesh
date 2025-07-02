using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports.Models
{
    /// <summary>
    /// Represents a single step in an agent's reasoning process.
    /// </summary>
    public class ReasoningStep
    {
        /// <summary>
        /// A description of the reasoning step or premise.
        /// </summary>
        public string Premise { get; set; }
        /// <summary>
        /// The conclusion drawn from this step.
        /// </summary>
        public string Conclusion { get; set; }
        /// <summary>
        /// The type of inference used (e.g., "Deductive", "Inductive", "Abductive").
        /// </summary>
        public string InferenceType { get; set; }
    }

    /// <summary>
    /// Represents a request to validate the ethical and logical integrity of an agent's action.
    /// This is based on Brandom's philosophy that agency is defined by the ability to justify actions with reasons.
    /// </summary>
    public class NormativeActionValidationRequest
    {
        /// <summary>
        /// A unique identifier for the agent proposing the action.
        /// </summary>
        public string AgentId { get; set; }
        /// <summary>
        /// A description of the action the agent intends to perform.
        /// </summary>
        public string ProposedAction { get; set; }
        /// <summary>
        /// The explicit reasons or justifications provided by the agent for the proposed action.
        /// </summary>
        public List<string> Justifications { get; set; } = new();
        /// <summary>
        /// The context in which the action is being considered, including user state and environmental factors.
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new();
    }

    /// <summary>
    /// Represents the response from a normative action validation check.
    /// </summary>
    public class NormativeActionValidationResponse
    {
        /// <summary>
        /// Indicates whether the proposed action is considered ethically and logically permissible.
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// A list of normative violations or ethical concerns identified if the action is not valid.
        /// </summary>
        public List<string> Violations { get; set; } = new();
    }

    /// <summary>
    /// Represents a request to validate a chain of reasoning steps for logical coherence and ethical soundness.
    /// </summary>
    public class ReasoningChainValidationRequest
    {
        /// <summary>
        /// The sequence of reasoning steps that led to a conclusion or action.
        /// </summary>
        public List<ReasoningStep> Chain { get; set; } = new();
    }

    /// <summary>
    /// Represents the response from a reasoning chain validation.
    /// </summary>
    public class ReasoningChainValidationResponse
    {
        /// <summary>
        /// Indicates whether the reasoning chain is logically and ethically sound.
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// A list of identified fallacies, biases, or ethical issues in the reasoning chain.
        /// </summary>
        public List<string> IdentifiedIssues { get; set; } = new();
    }

    /// <summary>
    /// Represents a request to assess the integrity of a user consent interaction.
    /// This ensures that consent is obtained transparently and without manipulation, respecting user autonomy.
    /// </summary>
    public class ConsentIntegrityAssessmentRequest
    {
        /// <summary>
        /// The text of the prompt presented to the user.
        /// </summary>
        public string PromptText { get; set; }
        /// <summary>
        /// The options that were presented to the user.
        /// </summary>
        public List<string> PresentedOptions { get; set; } = new();
        /// <summary>
        /// The user's selected option.
        /// </summary>
        public string UserSelection { get; set; }
        /// <summary>
        /// The cultural context (e.g., Hofstede dimensions) of the user, which influences the interpretation of consent.
        /// </summary>
        public Dictionary<string, object> CulturalContext { get; set; } = new();
    }

    /// <summary>
    /// Represents the response from a consent integrity assessment.
    /// </summary>
    public class ConsentIntegrityAssessmentResponse
    {
        /// <summary>
        /// A score from 0 to 1 indicating the assessed integrity of the consent, where 1 is highest integrity.
        /// </summary>
        public double IntegrityScore { get; set; }
        /// <summary>
        /// A list of potential issues identified, such as the use of dark patterns, lack of clarity, or coercive language.
        /// </summary>
        public List<string> PotentialIssues { get; set; } = new();
    }
}

namespace CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports
{
    using CognitiveMesh.ReasoningLayer.EthicalReasoning.Ports.Models;

    /// <summary>
    /// Defines the contract for the Normative Agency Port. This port provides the core reasoning capabilities
    /// for ensuring that all agentic actions within the Cognitive Mesh are ethically sound, justifiable,
    /// and respectful of user autonomy, based on the philosophical principles of Robert Brandom.
    /// </summary>
    public interface INormativeAgencyPort
    {
        /// <summary>
        /// Evaluates a proposed agent action against its justifications to determine if it is normatively permissible.
        /// This method is central to the "game of giving and asking for reasons."
        /// </summary>
        /// <param name="request">The request containing the proposed action and its justifications.</param>
        /// <returns>A response indicating whether the action is valid and a list of any normative violations.</returns>
        Task<NormativeActionValidationResponse> ValidateActionAsync(NormativeActionValidationRequest request);

        /// <summary>
        /// Validates an agent's reasoning chain for logical coherence and ethical soundness. This ensures
        /// that the process by which an agent reaches a conclusion is as important as the conclusion itself.
        /// </summary>
        /// <param name="request">The request containing the sequence of reasoning steps.</param>
        /// <returns>A response indicating whether the chain is valid and a list of any identified issues.</returns>
        Task<ReasoningChainValidationResponse> ValidateReasoningChainAsync(ReasoningChainValidationRequest request);

        /// <summary>
        /// Assesses a user consent interaction to ensure it upholds the principles of user autonomy and
        /// informational dignity. It checks for manipulative patterns and ensures clarity.
        /// </summary>
        /// <param name="request">The request containing details of the consent interaction.</param>
        /// <returns>A response containing an integrity score and a list of any potential issues.</returns>
        Task<ConsentIntegrityAssessmentResponse> AssessConsentIntegrityAsync(ConsentIntegrityAssessmentRequest request);
    }
}
