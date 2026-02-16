using MediatR;

namespace AgencyLayer.HumanCollaboration.Features.Messages
{
    /// <summary>
    /// Command to add a message to a collaboration session.
    /// </summary>
    public class AddMessageCommand : IRequest<CollaborationMessage>
    {
        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        public required string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the sender ID.
        /// </summary>
        public required string SenderId { get; set; }

        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        public required string Content { get; set; }

        /// <summary>
        /// Gets or sets the message type (e.g. text).
        /// </summary>
        public required string MessageType { get; set; }

        /// <summary>
        /// Gets or sets optional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
