using Microsoft.Extensions.Logging;

namespace AgencyLayer.HumanCollaboration.Features.Messages
{
    /// <summary>
    /// Handler for retrieving session messages.
    /// </summary>
    public class GetSessionMessagesQueryHandler : IRequestHandler<GetSessionMessagesQuery, IEnumerable<CollaborationMessage>>
    {
        private readonly IKnowledgeGraphManager _knowledgeGraphManager;
        private readonly ILogger<GetSessionMessagesQueryHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSessionMessagesQueryHandler"/> class.
        /// </summary>
        /// <param name="knowledgeGraphManager">The knowledge graph manager.</param>
        /// <param name="logger">The logger.</param>
        public GetSessionMessagesQueryHandler(
            IKnowledgeGraphManager knowledgeGraphManager,
            ILogger<GetSessionMessagesQueryHandler> logger)
        {
            _knowledgeGraphManager = knowledgeGraphManager ?? throw new ArgumentNullException(nameof(knowledgeGraphManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CollaborationMessage>> Handle(GetSessionMessagesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Retrieving messages for session {SessionId}", request.SessionId);

                // We can use FindNodesAsync to find all nodes with label CollaborationMessage
                // and property SessionId = request.SessionId

                var properties = new Dictionary<string, object>
                {
                    { "SessionId", request.SessionId }
                };

                // Note: FindNodesAsync might return nodes of type CollaborationMessage if the generic type matches
                // However, IKnowledgeGraphManager.FindNodesAsync<T> might require T to have exact properties matched.
                // Since we stored it as CollaborationMessage, we can try to retrieve it as such.

                // Assuming FindNodesAsync internally filters by label if we could pass it, but the interface
                // only takes properties. If the graph is shared, this might fetch other nodes with same SessionId if any.
                // But generally only messages and participants have SessionId.
                // To be safe, we could check the type or rely on properties unique to messages.

                var messages = await _knowledgeGraphManager.FindNodesAsync<CollaborationMessage>(properties, cancellationToken);

                // In-memory sorting and pagination (since graph query interface is limited for complex ordering/paging)
                // If the dataset grows large, we should implement a custom Cypher query via QueryAsync.

                var orderedMessages = messages
                    .OrderByDescending(m => m.Timestamp) // Latest first usually
                    .AsEnumerable();

                if (!string.IsNullOrEmpty(request.BeforeMessageId))
                {
                    // This is simple pagination logic; requires finding the message and taking subsequent ones
                    // For now, simpler implementation:
                    // If BeforeMessageId is provided, we assume we want messages OLDER than that one.

                    var pivotMessage = messages.FirstOrDefault(m => m.Id == request.BeforeMessageId);
                    if (pivotMessage != null)
                    {
                        orderedMessages = orderedMessages.Where(m => m.Timestamp < pivotMessage.Timestamp);
                    }
                }

                return orderedMessages.Take(request.Limit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetSessionMessagesQuery for session {SessionId}", request.SessionId);
                throw;
            }
        }
    }
}
