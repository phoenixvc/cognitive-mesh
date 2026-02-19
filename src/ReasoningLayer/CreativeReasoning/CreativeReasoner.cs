using CognitiveMesh.ReasoningLayer.LLMReasoning.Abstractions;

namespace CognitiveMesh.ReasoningLayer.CreativeReasoning
{
    /// <summary>
    /// Generates creative content, ideas, and brainstorming outputs.
    /// </summary>
    public class CreativeReasoner : IDisposable
    {
        private readonly ILLMClient _llmClient;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreativeReasoner"/> class.
        /// </summary>
        /// <param name="llmClient">The LLM client for generating creative content.</param>
        public CreativeReasoner(ILLMClient llmClient)
        {
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        /// <summary>
        /// Generates multiple creative ideas based on a given prompt.
        /// </summary>
        /// <param name="prompt">The base prompt or topic for idea generation.</param>
        /// <param name="count">Number of ideas to generate (default is 3).</param>
        /// <returns>A string containing the generated ideas.</returns>
        public async Task<string> GenerateIdeasAsync(string prompt, int count = 3)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
                
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

            try
            {
                var ideaPrompt = $"""
                    Generate {count} creative and innovative ideas based on the following prompt.
                    Format each idea with a title and a 1-2 sentence description.
                    
                    Prompt: {prompt}
                    
                    Ideas:
                    """;
                    
                return await _llmClient.GenerateCompletionAsync(ideaPrompt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to generate ideas. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Expands on a given idea with more details and variations.
        /// </summary>
        /// <param name="baseIdea">The initial idea to expand upon.</param>
        /// <param name="aspects">Specific aspects to consider in the expansion.</param>
        /// <returns>Detailed expansion of the idea.</returns>
        public async Task<string> ExpandIdeaAsync(string baseIdea, params string[] aspects)
        {
            if (string.IsNullOrWhiteSpace(baseIdea))
                throw new ArgumentException("Base idea cannot be null or whitespace.", nameof(baseIdea));

            try
            {
                var aspectText = aspects != null && aspects.Length > 0 
                    ? "Consider these aspects: " + string.Join(", ", aspects)
                    : string.Empty;
                    
                var expandPrompt = $"""
                    Expand on the following idea with detailed explanations and variations.
                    {aspectText}
                    
                    Idea: {baseIdea}
                    
                    Detailed Expansion:
                    """;
                    
                return await _llmClient.GenerateCompletionAsync(expandPrompt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to expand idea. See inner exception for details.", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_llmClient is IDisposable disposableClient)
                        disposableClient.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
