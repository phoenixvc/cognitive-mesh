using MediatR;

namespace AgencyLayer.HumanCollaboration.Features.Messages
{
    /// <summary>
    /// Query to retrieve messages for a session.
    /// </summary>
    public class GetSessionMessagesQuery : IRequest<IEnumerable<CollaborationMessage>>
    {
        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        public required string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of messages to retrieve.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the message ID to fetch messages before (pagination).
        /// </summary>
        public string? BeforeMessageId { get; set; }
    }
}
