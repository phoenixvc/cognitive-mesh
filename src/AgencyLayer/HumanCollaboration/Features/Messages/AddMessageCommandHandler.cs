using Microsoft.Extensions.Logging;

namespace AgencyLayer.HumanCollaboration.Features.Messages
{
    /// <summary>
    /// Handler for adding messages to a session.
    /// </summary>
    public class AddMessageCommandHandler : IRequestHandler<AddMessageCommand, CollaborationMessage>
    {
        private readonly IKnowledgeGraphManager _knowledgeGraphManager;
        private readonly ILogger<AddMessageCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddMessageCommandHandler"/> class.
        /// </summary>
        /// <param name="knowledgeGraphManager">The knowledge graph manager.</param>
        /// <param name="logger">The logger.</param>
        public AddMessageCommandHandler(
            IKnowledgeGraphManager knowledgeGraphManager,
            ILogger<AddMessageCommandHandler> logger)
        {
            _knowledgeGraphManager = knowledgeGraphManager ?? throw new ArgumentNullException(nameof(knowledgeGraphManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CollaborationMessage> Handle(AddMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var messageId = $"msg-{Guid.NewGuid()}";
                var timestamp = DateTime.UtcNow;

                var message = new CollaborationMessage
                {
                    Id = messageId,
                    SessionId = request.SessionId,
                    SenderId = request.SenderId,
                    Content = request.Content,
                    MessageType = request.MessageType,
                    Timestamp = timestamp,
                    Metadata = request.Metadata ?? new Dictionary<string, object>()
                };

                _logger.LogDebug("Persisting message {MessageId} for session {SessionId}", messageId, request.SessionId);

                // Create the message node
                await _knowledgeGraphManager.AddNodeAsync(
                    messageId,
                    message,
                    CollaborationNodeLabels.CollaborationMessage,
                    cancellationToken);

                // Link message to session (assuming session node exists or we just link blindly)
                // Relationship: (Session)-[:HAS_MESSAGE]->(Message)
                await _knowledgeGraphManager.AddRelationshipAsync(
                    request.SessionId,
                    messageId,
                    "HAS_MESSAGE",
                    null,
                    cancellationToken);

                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling AddMessageCommand for session {SessionId}", request.SessionId);
                throw;
            }
        }
    }
}
